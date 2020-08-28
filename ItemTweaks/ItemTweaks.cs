using System;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Horseyboi.ItemTweaks {

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Horseyboi.ItemTweaks", "Item Tweaks", "1.0.0")]

    public class ItemTweaks : BaseUnityPlugin {

        //TODO: add more configurability for tweaks
        static ConfigEntry<bool> EnablePlates { get; set; }
        static ConfigEntry<float> ArmorPlateDR { get; set; }
        static ConfigEntry<SettingMode> PlateDRType { get; set; }

        static ConfigEntry<bool> EnableUrn { get; set; }

        static ConfigEntry<bool> EnableKnurl { get; set; }
        static ConfigEntry<float> KnurlHealthIncrease { get; set; }
        static ConfigEntry<SettingMode> KnurlHealthType { get; set; }

        private enum SettingMode {
            Percent,
            Fixed
        };

        public void Awake() {
            EnablePlates = Config.Bind<bool>(
                "Repulsion Armor Plate",
                "Enabled",
                true,
                "Enables Repulsion Armor Plate changes."
                );
            PlateDRType = Config.Bind<SettingMode>(
                "Repulsion Armor Plate",
                "Damage Resistance Type",
                SettingMode.Percent,
                "Sets whether Repulsion Armor Plate's damage reduction is a fixed value or a percentage of maximum health."
                );
            ArmorPlateDR = Config.Bind<float>(
                "Repulsion Armor Plate",
                "Damage Resistance",
                0.01f,
                "Sets the amount of damage resistance Repulsion Armor Plate confers.\nIn Percent mode, this is a decimal as percent health; in Fixed mode, this is a static value."
                );            

            EnableUrn = Config.Bind<bool>(
                "Mired Urn",
                "Enabled",
                true,
                "Forces Mired Urn to only target enemies."
                );

            EnableKnurl = Config.Bind<bool>(
                "Titanic Knurl",
                "Enabled",
                true,
                "Enables Titanic Knurl changes."
                );
            KnurlHealthType = Config.Bind<SettingMode>(
                "Titanic Knurl",
                "Health Increase Type",
                SettingMode.Percent,
                "Sets whether Titanic Knurl's health increase is a fixed value or percentage of maximum health."
                );
            KnurlHealthIncrease = Config.Bind<float>(
                "Titanic Knurl",
                "Health Increase",
                0.15f,
                "Sets Titanic Knurl's health increase.\nIn Percent mode, this is a decimal (e.g. 15% = 0.15); in Fixed mode, this is a static value."
                );


            if (EnablePlates.Value) {
                ChangeArmorPlates();
            }
            if (EnableUrn.Value) {
                ChangeMiredUrn(); //figure out a way to make urn only tar enemies?
            }
            if (EnableKnurl.Value) {
                ChangeKnurl();
            }
        }

        private void ChangeArmorPlates() {
            IL.RoR2.HealthComponent.TakeDamage += (il) => {

                //Match the location of the armor plate calculation and set the cursor to the right line
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdcR4(1),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdcR4(5),
                    x => x.MatchLdarg(0)
                    );
                c.Index += 2;
                c.Remove();

                if (PlateDRType.Value == SettingMode.Percent) {
                    //add the character's combined health to the stack
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Call, typeof(HealthComponent).GetMethod("get_fullCombinedHealth"));
                    c.EmitDelegate<Func<float, float>>((health) => {
                        health *= ArmorPlateDR.Value; //get 1% of it as damage reduction and return it to the stack
                        return health;
                    });
                } else if (PlateDRType.Value == SettingMode.Fixed) {
                    c.Emit(OpCodes.Ldc_R4, ArmorPlateDR.Value);
                }
             };
        }

        private void ChangeMiredUrn() {
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
        }

        private void ChangeKnurl() {
            IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                //Match the location where Knurl calculations happen
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdloc(41),
                    x => x.MatchLdloc(14),
                    x => x.MatchConvR4(),
                    x => x.MatchLdcR4(40)
                    );                
                c.Index += 3;
                c.Remove(); //40 removed

                //get num35 i.e. the max health calc
                if (KnurlHealthType.Value == SettingMode.Percent) {
                    c.Emit(OpCodes.Ldloc_S, (byte)41);
                    c.EmitDelegate<Func<float, float>>((health) => {
                        health *= KnurlHealthIncrease.Value;
                        return health;
                    });
                } else if (KnurlHealthType.Value == SettingMode.Fixed) {
                    c.Emit(OpCodes.Ldc_R4, KnurlHealthIncrease.Value);
                }
            };
        }
    }
}
