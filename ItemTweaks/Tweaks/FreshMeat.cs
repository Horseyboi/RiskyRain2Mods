using MonoMod.Cil;
using R2API;
using RoR2;
using System;

namespace ItemTweaks.Tweaks {
    internal static class FreshMeat {

        internal static void ChangeItem() {
            //Make all the config stuff
            var EnableMeat = ItemTweaks.BindConfig<bool>(
                "Fresh Meat",
                "Enabled",
                true,
                "Enables Fresh Meat changes."
                );
            var BaseMeatRegen = ItemTweaks.BindConfig<float>(
                "Fresh Meat",
                "Base Regen",
                3f,
                "Sets the base health regeneration of the Fresh Meat buff."
                );
            var StackMeatRegen = ItemTweaks.BindConfig<float>(
                "Fresh Meat",
                "Stack Regen",
                1.5f,
                "How much extra health regeneration the Fresh Meat buff gets per stack."
                );
            var BaseMeatDur = ItemTweaks.BindConfig<float>(
                "Fresh Meat",
                "Base Duration",
                3f,
                "Sets the base duration of the Fresh Meat buff."
                );
            var StackMeatDur = ItemTweaks.BindConfig<float > (
                "Fresh Meat",
                "Stack Duration",
                3f,
                "How much extra duration the Fresh Meat buff gets per stack."
                );

            if (EnableMeat.Value) {
                //description change
                String meatDesc = "Increases <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>" + BaseMeatRegen.Value.ToString() + " hp/s</style>";
                if (StackMeatRegen.Value > 0) {
                    meatDesc += "<style=cStack> (+" + StackMeatRegen.Value.ToString() + " hp/s per stack)</style>";
                }
                meatDesc += " for <style=cIsUtility> " + BaseMeatDur.Value.ToString() + "s</style>";
                if (StackMeatDur.Value > 0) {
                    meatDesc += " <style=cStack> (+" + StackMeatDur.Value.ToString() + "s per stack)</style>";
                }
                meatDesc += " after killing an enemy.";
                LanguageAPI.Add("ITEM_REGENONKILL_DESC", meatDesc);

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
                    c.RemoveRange(8); //remove default instructions for meat regen
                    c.EmitDelegate<Func<CharacterBody, float>>((self) => {
                        if (self.HasBuff(BuffIndex.MeatRegenBoost)) {
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
                    c.RemoveRange(6); //remove default instructions for meat duration
                    c.EmitDelegate<Action<CharacterBody>>((self) => {
                        int meatNum = self.inventory.GetItemCount(ItemIndex.RegenOnKill);
                        self.AddTimedBuff(BuffIndex.MeatRegenBoost, BaseMeatDur.Value + (meatNum - 1) * StackMeatDur.Value);
                    });
                };
            }
        }
    }
}
