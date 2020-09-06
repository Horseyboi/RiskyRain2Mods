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
    [BepInPlugin("com.Horseyboi.ItemTweaks", "Item Tweaks", "1.1.0")]

    public class ItemTweaks : BaseUnityPlugin {

        static ConfigEntry<bool> EnablePlates { get; set; }
        static ConfigEntry<float> ArmorPlateDR { get; set; }
        static ConfigEntry<RAPSettingMode> PlateDRType { get; set; }
        static ConfigEntry<float> MonsterPlateDR { get; set; }

        static ConfigEntry<bool> EnableUrn { get; set; }

        static ConfigEntry<bool> EnableKnurl { get; set; }
        static ConfigEntry<float> KnurlHealthIncrease { get; set; }
        static ConfigEntry<SettingMode> KnurlHealthType { get; set; }

        static ConfigEntry<bool> EnableHoof { get; set; }
        static ConfigEntry<float> InitialHoof { get; set; }
        static ConfigEntry<float> StackHoof { get; set; }

        static ConfigEntry<bool> EnableNRG { get; set; }
        static ConfigEntry<float> InitialNRG { get; set; }
        static ConfigEntry<float> StackNRG { get; set; }

        private enum RAPSettingMode {
            Percent,
            Fixed,
            Armor
        };

        private enum SettingMode {
            Percent,
            Fixed
        };

        public void Awake() {
            //Plates
            EnablePlates = Config.Bind<bool>(
                "Repulsion Armor Plate",
                "Enabled",
                true,
                "Enables Repulsion Armor Plate changes."
                );
            PlateDRType = Config.Bind<RAPSettingMode>(
                "Repulsion Armor Plate",
                "Damage Resistance Type",
                RAPSettingMode.Percent,
                "Sets whether Repulsion Armor Plate's damage reduction is fixed value, a percentage of maximum health, or given as Armor."
                );
            ArmorPlateDR = Config.Bind<float>(
                "Repulsion Armor Plate",
                "Damage Resistance",
                0.01f,
                "Sets the amount of damage resistance Repulsion Armor Plate confers.\nIn Percent mode, this is a percent of your max health (e.g. 0.01 reduces damage by 1% of maximum health);\nIn Fixed mode, this is a static value;\nIn Armor mode this is the amount of Armor Repulsion Armor Plates confer per stack (I recommend somewhere around 7 for this mode)."
                );
            MonsterPlateDR = Config.Bind<float>(
                "Repulsion Armor Plate",
                "Monster Damage Resistance",
                5f,
                "Sets how much damage resistance monsters get from Repulsion Armor Plates. This is fixed like vanilla."
                );
            //Urn
            EnableUrn = Config.Bind<bool>(
                "Mired Urn",
                "Enabled",
                true,
                "Forces Mired Urn to only target enemies."
                );
            //Knurl
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
                "Sets Titanic Knurl's health increase.\nIn Percent mode, this is a percent of your max health (e.g. 0.15 = 15%); in Fixed mode, this is a static value."
                );
            //Hoof
            EnableHoof = Config.Bind<bool>(
                "Pauls Goat Hoof",
                "Enabled",
                true,
                "Enables Paul's Goat Hoof changes."
                );
            InitialHoof = Config.Bind<float>(
                "Pauls Goat Hoof",
                "Initial Bonus",
                0.14f,
                "Sets the initial bonus Hoof grants."
                );
            StackHoof = Config.Bind<float>(
                "Pauls Goat Hoof",
                "Stack Bonus",
                0.09f,
                "Sets the bonus Hoof grants per additional stack."
                );
            //Drink
            EnableNRG = Config.Bind<bool>(
                "Energy Drink",
                "Enabled",
                false,
                "Enables Energy Drink changes."
                );
            InitialNRG = Config.Bind<float>(
                "Energy Drink",
                "Initial Bonus",
                0.30f,
                "Sets the initial bonus Energy Drink grants."
                );
            StackNRG = Config.Bind<float>(
                "Energy Drink",
                "Stack Bonus",
                0.20f,
                "Sets the bonus Energy Drink grants per additional stack."
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
            if (EnableHoof.Value) {
                ChangeHoof();
            }
            if (EnableNRG.Value) {
                ChangeNRG();
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
                
                //add the character's to the stack
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<CharacterBody, float>>((self) => {

                    if (self.teamComponent.teamIndex == TeamIndex.Player) { //Is this a player?
                        if (PlateDRType.Value == RAPSettingMode.Percent) {
                            return self.healthComponent.fullCombinedHealth * ArmorPlateDR.Value / 100; //get the chosen percent and return it to the stack to use as damage reduction

                        } else if (PlateDRType.Value == RAPSettingMode.Fixed) {
                            return ArmorPlateDR.Value; //just return the normal value

                        } else if (PlateDRType.Value == RAPSettingMode.Armor) {
                            return 0f; //make RAP do nothing here if we want armor instead

                        } else {
                            Debug.LogWarning("RAP not set to valid mode!");
                            return 5f; //something went wrong
                        }
                    } else { //This is not a player and probably a monster
                        return MonsterPlateDR.Value;
                    }
                });
            };

            if (PlateDRType.Value == RAPSettingMode.Armor) {
                IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                    //find the armor calculations and put the cursor in the right spot
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld(typeof(RoR2.CharacterBody).GetField("baseArmor")),
                        x => x.MatchLdarg(0)
                        );
                    c.Index += 9;

                    //push the CharacterBody onto the stack
                    c.Emit(OpCodes.Ldarg_0);
                    //consume it for the context to do all the stuff here
                    c.EmitDelegate<Action<CharacterBody>>((self) => {
                        var numPlates = self.inventory.GetItemCount(ItemIndex.ArmorPlate);
                        //armor is readonly so Reflection is used to tiptoe around that
                        self.InvokeMethod("set_armor", self.armor + numPlates * ArmorPlateDR.Value);
                    });
                };
            }
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
                    c.Emit(OpCodes.Ldc_R4, KnurlHealthIncrease.Value/100);
                }
            };
        }

        private void ChangeHoof() {
            IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                //Locate hoof speed calculations
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdloc(54),
                    x => x.MatchLdloc(3),
                    x => x.MatchConvR4(),
                    x => x.MatchLdcR4(0.14f)
                    );                
                c.Index += 3;
                //remove the default instructions for paul's goat hoof; we makin our own
                c.RemoveRange(3);
                c.EmitDelegate<Func<float, float, float>>((speed, hoofs) => {
                    if (hoofs > 0) {
                        speed += InitialHoof.Value - StackHoof.Value; //Get the fixed bonus
                        speed += StackHoof.Value * hoofs; //Add the bonus per hoof
                        }
                    return speed;
                });
            };
        }

        private void ChangeNRG() {
            IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                //Locate drink speed calc
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdloc(54),
                    x => x.MatchLdcR4(0.1f),
                    x => x.MatchLdcR4(0.2f),
                    x => x.MatchLdloc(17)
                    );
                c.Index += 1;
                //remove the default instructions for energy drink
                c.RemoveRange(9);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<CharacterBody, float>>((self) => {
                    return (InitialNRG.Value + StackNRG.Value * (self.inventory.GetItemCount(ItemIndex.SprintBonus) - 1)) / self.sprintingSpeedMultiplier;
                });
            };
        }
    }
}
