using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace ItemTweaks.Tweaks {
    internal static class MiredUrn {

        internal static void ChangeItem() {
            //Make the config and all that
            var EnableUrn = ItemTweaks.BindConfig<bool>(
                "Mired Urn",
                "Enabled",
                true,
                "Forces Mired Urn to only target enemies."
                );
            var UrnDamage = ItemTweaks.BindConfig<float>(
                "Mired Urn",
                "Damage Coefficient",
                0.45f,
                "Forces Mired Urn to only target enemies."
                );

            if (EnableUrn.Value) {
                //description change -- funnily enough no itemstats change is necessary as it already erroneously says "enemies"
                LanguageAPI.Add("ITEM_SIPHONONLOWHEALTH_PICKUP", "Siphon health from nearby enemies while in combat.");
                LanguageAPI.Add("ITEM_SIPHONONLOWHEALTH_DESC", "While in combat, the nearest 1 <style=cStack>(+1 per stack)</style> enemies to you within <style=cIsDamage>13m</style> will be 'tethered' to you, dealing <style=cIsDamage>" + (UrnDamage.Value * 100).ToString() + "%</style> damage per second, applying <style=cIsDamage>tar</style>, and <style=cIsHealing>healing</style> you for <style=cIsHealing>100%</style> of the damage dealt.");

                On.RoR2.SiphonNearbyController.SearchForTargets += (orig, self, dest) => {

                    //use reflection to get values we can't access
                    SphereSearch sphereSearch = self.GetFieldValue<SphereSearch>("sphereSearch");
                    NetworkedBodyAttachment body = self.GetFieldValue<NetworkedBodyAttachment>("networkedBodyAttachment");
                    //set up a TeamMask
                    TeamMask teamMask = TeamMask.allButNeutral;
                    teamMask.RemoveTeam(body.attachedBody.teamComponent.teamIndex);

                    //mostly copied stuff from the vanilla code
                    sphereSearch.mask = LayerIndex.entityPrecise.mask;
                    sphereSearch.origin = self.transform.position;
                    sphereSearch.radius = self.radius;
                    sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                    sphereSearch.RefreshCandidates();
                    sphereSearch.OrderCandidatesByDistance();
                    sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask); //our shiny new teamfilter line
                    sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                    sphereSearch.GetHurtBoxes(dest);
                    sphereSearch.ClearCandidates();
                };

                On.RoR2.SiphonNearbyController.Awake += (orig, self) => {
                    orig(self);
                    self.damagePerSecondCoefficient = UrnDamage.Value;
                };
            }
        }
    }
}
