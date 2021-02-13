using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;

namespace ItemTweaks.Tweaks {
    internal static class EnergyDrink {
        internal static void ChangeItem() {
            //Make the config and all that
            var EnableNRG = ItemTweaks.BindConfig<bool>(
                "Energy Drink",
                "Enabled",
                false,
                "Enables Energy Drink changes."
                );
            var InitialNRG = ItemTweaks.BindConfig<float>(
                "Energy Drink",
                "Initial Bonus",
                0.30f,
                "Sets the initial bonus Energy Drink grants; percentage value."
                );
            var StackNRG = ItemTweaks.BindConfig<float>(
                "Energy Drink",
                "Stack Bonus",
                0.20f,
                "Sets the bonus Energy Drink grants per additional stack; percentage value."
                );

            if (EnableNRG.Value) {
                //description change
                LanguageAPI.Add("ITEM_SPRINTBONUS_DESC", "<style=cIsUtility>Sprint speed</style> is improved by <style=cIsUtility>" + (InitialNRG.Value * 100).ToString() + "%</style> <style=cStack>(+" + (StackNRG.Value * 100).ToString() + "% per stack)</style>.");

                IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                    //Locate drink speed calc
                    ILCursor c = new ILCursor(il);

                    int nrgCountLoc = 0;

                    c.GotoNext(MoveType.After,
                        x => x.MatchLdcI4((int)ItemIndex.SprintBonus),
                        x => x.MatchCallOrCallvirt<Inventory>("GetItemCount"),
                        x => x.MatchStloc(out nrgCountLoc)
                    );

                    c.GotoNext(
                        x => x.MatchLdloc(nrgCountLoc),
                        x => x.MatchConvR4()
                    );
                    c.Index -= 2;

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
}
