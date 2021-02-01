using System;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API;
using R2API.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Horseyboi.ItemTweaks {

    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("dev.ontrigger.itemstats", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)] //technically this is default behavior but w/e
    [BepInPlugin("com.Horseyboi.ItemTweaks", "Item Tweaks", "1.3.0")]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]

    public class ItemTweaks : BaseUnityPlugin {

        public static ConfigEntry<bool> EnablePlates { get; set; }
        public static ConfigEntry<float> ArmorPlateDR { get; set; }
        public static ConfigEntry<RAPSettingMode> PlateDRType { get; set; }
        public static ConfigEntry<float> MonsterPlateDR { get; set; }
        public static ConfigEntry<RAPSettingMode> MonsterPlateDRType { get; set; }

        public static ConfigEntry<bool> EnableUrn { get; set; }

        public static ConfigEntry<bool> EnableKnurl { get; set; }
        public static ConfigEntry<float> KnurlHealthIncrease { get; set; }
        public static ConfigEntry<SettingMode> KnurlHealthType { get; set; }

        public static ConfigEntry<bool> EnableHoof { get; set; }
        public static ConfigEntry<float> InitialHoof { get; set; }
        public static ConfigEntry<float> StackHoof { get; set; }

        public static ConfigEntry<bool> EnableNRG { get; set; }
        public static ConfigEntry<float> InitialNRG { get; set; }
        public static ConfigEntry<float> StackNRG { get; set; }

        public static ConfigEntry<bool> EnableMeat { get; set; }
        public static ConfigEntry<float> BaseMeatRegen { get; set; }
        public static ConfigEntry<float> StackMeatRegen { get; set; }
        public static ConfigEntry<float> BaseMeatDur { get; set; }
        public static ConfigEntry<float> StackMeatDur { get; set; }

        public enum RAPSettingMode {
            Percent,
            Fixed,
            Armor
        };

        public enum SettingMode {
            Percent,
            Fixed
        };

        public void Awake() {
            //Set up configs

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
            MonsterPlateDRType = Config.Bind<RAPSettingMode>(
                "Repulsion Armor Plate",
                "Monster Damage Resistance Type",
                RAPSettingMode.Fixed,
                "Sets whether Repulsion Armor Plate, when used by monsters, has damage reduction is fixed value, a percentage of maximum health, or given as Armor."
                );
            MonsterPlateDR = Config.Bind<float>(
                "Repulsion Armor Plate",
                "Monster Damage Resistance",
                5f,
                "Sets how much damage resistance monsters get from Repulsion Armor Plates."
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

            //Meat
            EnableMeat = Config.Bind<bool>(
                "Fresh Meat",
                "Enabled",
                true,
                "Enables Fresh Meat changes."
                );
            BaseMeatRegen = Config.Bind<float>(
                "Fresh Meat",
                "Base Regen",
                3f,
                "Sets the base health regeneration of the Fresh Meat buff."
                );
            StackMeatRegen = Config.Bind<float>(
                "Fresh Meat",
                "Stack Regen",
                1.5f,
                "How much extra health regeneration the Fresh Meat buff gets per stack."
                );
            BaseMeatDur = Config.Bind<float>(
                "Fresh Meat",
                "Base Duration",
                3f,
                "Sets the base duration of the Fresh Meat buff."
                );
            StackMeatDur = Config.Bind<float>(
                "Fresh Meat",
                "Stack Duration",
                3f,
                "How much extra duration the Fresh Meat buff gets per stack."
                );


            //check if itemstats is installed so I can funk out some better item descs
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.ontrigger.itemstats")) {
                AdvTooltips.DoStuff();
            }

            if (EnablePlates.Value) {
                ChangeArmorPlates();
            }
            if (EnableUrn.Value) {
                ChangeMiredUrn(); //figure out a way to make urn only tar debuff enemies?
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
            if (EnableMeat.Value) {
                ChangeMeat();
            }
        }

        private void ChangeArmorPlates() {
            //language changes
            if (PlateDRType.Value == RAPSettingMode.Percent) {
                LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_DESC", "Reduce all <style=cIsDamage>incoming damage</style> by <style=cIsDamage>" + (ArmorPlateDR.Value * 100).ToString() + "%</style> of your <style=cIsHealing>maximum health and shields</style> <style=cStack> (+" + (ArmorPlateDR.Value * 100).ToString() + "% per stack)</style>. Cannot reduce damage below <style=cIsDamage>1</style>.");
            } else if (PlateDRType.Value == RAPSettingMode.Fixed) {
                LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_DESC", "Reduce all <style=cIsDamage>incoming damage</style> by <style=cIsDamage>" + ArmorPlateDR.Value.ToString() + "</style> <style=cStack> (+" + (ArmorPlateDR.Value).ToString() + " per stack)</style>. Cannot reduce damage below <style=cIsDamage>1</style>.");
            } else if (PlateDRType.Value == RAPSettingMode.Armor) {
                LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_PICKUP", "Reduce incoming damage.");
                LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_DESC", "<style=cIsHealing>Increase armor</style> by <style=cIsHealing>" + ArmorPlateDR.Value.ToString() + "</style> <style=cStack>(+" + ArmorPlateDR.Value.ToString() + " per stack)</style>.");
            }

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
                c.EmitDelegate<Func<HealthComponent, float>>((self) => {

                    if (self.body.teamComponent.teamIndex == TeamIndex.Player) { //Is this a player?
                        if (PlateDRType.Value == RAPSettingMode.Percent) {
                            return self.fullCombinedHealth * ArmorPlateDR.Value; //get the chosen percent and return it to the stack to use as damage reduction

                        } else if (PlateDRType.Value == RAPSettingMode.Fixed) {
                            return ArmorPlateDR.Value; //just return the normal value

                        } else if (PlateDRType.Value == RAPSettingMode.Armor) {
                            return 0f; //make RAP do nothing here if we want armor instead

                        } else {
                            Debug.LogWarning("Player RAP not set to valid mode!");
                            return 5f; //something went wrong
                        }
                    } else { //This is not a player and probably a monster -- it'd be funny to see a pot with items though
                        if (MonsterPlateDRType.Value == RAPSettingMode.Percent) {
                            return self.fullCombinedHealth * MonsterPlateDR.Value; //get the chosen percent and return it to the stack to use as damage reduction

                        } else if (MonsterPlateDRType.Value == RAPSettingMode.Fixed) {
                            return MonsterPlateDR.Value; //just return the normal value

                        } else if (MonsterPlateDRType.Value == RAPSettingMode.Armor) {
                            return 0f; //make RAP do nothing here if we want armor instead

                        } else {
                            Debug.LogWarning("Monster RAP not set to valid mode!");
                            return 5f; //something went wrong
                        }
                    }
                });
            };

            if (PlateDRType.Value == RAPSettingMode.Armor || MonsterPlateDRType.Value == RAPSettingMode.Armor) {
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
                        if (self.teamComponent.teamIndex == TeamIndex.Player) {
                            self.InvokeMethod("set_armor", self.armor + numPlates * ArmorPlateDR.Value);
                        } else {
                            self.InvokeMethod("set_armor", self.armor + numPlates * MonsterPlateDR.Value);
                        }
                    });
                };
            }
        }

        private void ChangeMiredUrn() {
            //description change -- funnily enough no itemstats change is necessary as it already erroneously says "enemies"
            LanguageAPI.Add("ITEM_SIPHONONLOWHEALTH_PICKUP", "Siphon health from nearby enemies while in combat.");
            LanguageAPI.Add("ITEM_SIPHONONLOWHEALTH_DESC", "While in combat, the nearest 1 <style=cStack>(+1 per stack)</style> enemies to you within <style=cIsDamage>13m</style> will be 'tethered' to you, dealing <style=cIsDamage>100%</style> damage per second, applying <style=cIsDamage>tar</style>, and <style=cIsHealing>healing</style> you for <style=cIsHealing>100%</style> of the damage dealt.");

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
            //description changes
            if (KnurlHealthType.Value == SettingMode.Percent) {
                LanguageAPI.Add("ITEM_KNURL_DESC", "<style=cIsHealing>Increase maximum health</style> by <style=cIsHealing>" + KnurlHealthIncrease.Value * 100 + "%</style> <style=cStack>(+" + KnurlHealthIncrease.Value * 100 + "% per stack)</style> and <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+1.6 hp/s<style=cStack> (+1.6 hp/s per stack)</style>.");
            } else if (KnurlHealthType.Value == SettingMode.Fixed) {
                LanguageAPI.Add("ITEM_KNURL_DESC", "<style=cIsHealing>Increase maximum health</style> by <style=cIsHealing>" + KnurlHealthIncrease.Value + "</style> <style=cStack>(+" + KnurlHealthIncrease.Value + " per stack)</style> and <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+1.6 hp/s<style=cStack> (+1.6 hp/s per stack)</style>.");
            }

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
                
                if (KnurlHealthType.Value == SettingMode.Percent) {
                    c.Emit(OpCodes.Ldloc_S, (byte)41); //get num35 i.e. the max health calc
                    c.EmitDelegate<Func<float, float>>((health) => {
                        health *= KnurlHealthIncrease.Value;
                        return health;
                    });
                } else if (KnurlHealthType.Value == SettingMode.Fixed) {
                    c.Emit(OpCodes.Ldc_R4, KnurlHealthIncrease.Value);
                }
            };
        }

        private void ChangeHoof() {
            //description change
            LanguageAPI.Add("ITEM_HOOF_DESC", "Increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>" + (InitialHoof.Value * 100).ToString() + "%</style> <style=cStack>(+" + (StackHoof.Value * 100).ToString() + "% per stack)</style>.");

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
            //description change
            LanguageAPI.Add("ITEM_SPRINTBONUS_DESC", "<style=cIsUtility>Sprint speed</style> is improved by <style=cIsUtility>" + (InitialNRG.Value * 100).ToString() + "%</style> <style=cStack>(+" + (StackNRG.Value * 100).ToString() + "% per stack)</style>.");

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

        private void ChangeMeat() {
            //description change
            LanguageAPI.Add("ITEM_REGENONKILL_DESC", "Increases <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>" + BaseMeatRegen.Value.ToString() + " hp/s</style> <style=cStack>(+" + StackMeatRegen.Value.ToString() + " hp/s per stack)</style> for <style=cIsUtility>" + BaseMeatDur.Value.ToString() + "s</style> <style=cStack>(+" + StackMeatDur.Value.ToString() + "s per stack)</style> after killing an enemy.");

            //change meat buff regen
            IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdcR4(2f),
                    x => x.MatchLdloc(45),
                    x => x.MatchMul(),
                    x => x.MatchStloc(48),
                    x => x.MatchLdarg(0)
                    );
                c.Index -= 5;
                c.RemoveRange(8);
                c.EmitDelegate<Func<CharacterBody, float>>((self) => {
                    if (self.HasBuff(BuffIndex.MeatRegenBoost))
                    {
                        int meatNum = self.inventory.GetItemCount(ItemIndex.RegenOnKill);
                        return BaseMeatRegen.Value + (meatNum - 1) * StackMeatRegen.Value;
                    }
                    return 0f;
                });                
            };

            //change meat buff duration
            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) => {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdcR4(3),
                    x => x.MatchLdloc(57),
                    x => x.MatchConvR4(),
                    x => x.MatchMul(),
                    x => x.MatchCallvirt<CharacterBody>("AddTimedBuff")
                    );
                c.Index -= 1;
                c.RemoveRange(6);
                //c.Emit(OpCodes.Ldloc_S,13);
                c.EmitDelegate<Action<CharacterBody>>((self) => {
                    int meatNum = self.inventory.GetItemCount(ItemIndex.RegenOnKill);
                    self.AddTimedBuff(BuffIndex.MeatRegenBoost, BaseMeatDur.Value + (meatNum - 1) * StackMeatDur.Value);
                });
            };
        }
    }
}
