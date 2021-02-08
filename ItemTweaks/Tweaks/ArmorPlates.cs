using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;
using static ItemTweaks.ItemTweaks;

namespace ItemTweaks.Tweaks {
    internal static class ArmorPlates {

        internal static void ChangeItem() {
            //Make the config and all that
            var EnablePlates = ItemTweaks.BindConfig<bool>(
                "Repulsion Armor Plate",
                "Enabled",
                true,
                "Enables Repulsion Armor Plate changes."
                );
            var PlateDRType = ItemTweaks.BindConfig<RAPSettingMode>(
                "Repulsion Armor Plate",
                "Damage Resistance Type",
                RAPSettingMode.Percent,
                "Sets whether Repulsion Armor Plate's damage reduction is fixed value, a percentage of maximum health, or given as Armor."
                );
            var ArmorPlateDR = ItemTweaks.BindConfig<float>(
                "Repulsion Armor Plate",
                "Damage Resistance",
                0.01f,
                "Sets the amount of damage resistance Repulsion Armor Plate confers.\nIn Percent mode, this is a percent of your max health (e.g. 0.01 reduces damage by 1% of maximum health);\nIn Fixed mode, this is a static value;\nIn Armor mode this is the amount of Armor Repulsion Armor Plates confer per stack (I recommend somewhere around 7 for this mode)."
                );
            var MonsterPlateDRType = ItemTweaks.BindConfig<RAPSettingMode>(
                "Repulsion Armor Plate",
                "Monster Damage Resistance Type",
                RAPSettingMode.Fixed,
                "Sets whether Repulsion Armor Plate, when used by monsters, has damage reduction is fixed value, a percentage of maximum health, or given as Armor."
                );
            var MonsterPlateDR = ItemTweaks.BindConfig<float>(
                "Repulsion Armor Plate",
                "Monster Damage Resistance",
                5f,
                "Sets how much damage resistance monsters get from Repulsion Armor Plates."
                );

            if (EnablePlates.Value) {
                //language changes
                string rapDesc;
                if (PlateDRType.Value == RAPSettingMode.Armor) {
                    LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_PICKUP", "Reduce incoming damage.");
                    rapDesc = "<style=cIsHealing>Increase armor</style> by <style=cIsHealing>" + ArmorPlateDR.Value.ToString() + "</style> <style=cStack>(+" + ArmorPlateDR.Value.ToString() + " per stack)</style>.";
                } else {
                    rapDesc = "Reduce all <style=cIsDamage>incoming damage</style> by <style=cIsDamage>";
                    if (PlateDRType.Value == RAPSettingMode.Percent) {
                        rapDesc += (ArmorPlateDR.Value * 100).ToString() + "%</style> of your <style=cIsHealing>maximum health and shields</style> <style=cStack> (+" + (ArmorPlateDR.Value * 100).ToString() + "% per stack)</style>.";
                    } else if (PlateDRType.Value == RAPSettingMode.Fixed) {
                        rapDesc += ArmorPlateDR.Value.ToString() + "</style> <style=cStack> (+" + (ArmorPlateDR.Value).ToString() + " per stack)</style>.";
                    }
                    rapDesc += " Cannot reduce damage below <style=cIsDamage>1</style>.";
                }
                LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_DESC", rapDesc);

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

                    //add the character's health component to the stack
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
                    IL.RoR2.CharacterBody.RecalculateStats += (il) => { //further analysis says I probably could have used an event hook but this seems to be fine
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
                            if (self.teamComponent.teamIndex == TeamIndex.Player && PlateDRType.Value == RAPSettingMode.Armor) { //make sure this is on a team that gets armor
                                self.InvokeMethod("set_armor", self.armor + numPlates * ArmorPlateDR.Value);
                            } else if (MonsterPlateDRType.Value == RAPSettingMode.Armor) { //same deal here
                                self.InvokeMethod("set_armor", self.armor + numPlates * MonsterPlateDR.Value);
                            }
                        });
                    };
                }
            }
        }
    }
}
