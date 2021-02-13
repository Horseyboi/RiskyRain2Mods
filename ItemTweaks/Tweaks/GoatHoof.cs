using MonoMod.Cil;
using R2API;
using System;
using RoR2;

namespace ItemTweaks.Tweaks {
    internal static class GoatHoof {
        internal static void ChangeItem() {
            //Make the config and all that
            var EnableHoof = ItemTweaks.BindConfig<bool>(
                "Pauls Goat Hoof",
                "Enabled",
                true,
                "Enables Paul's Goat Hoof changes."
                );
            var InitialHoof = ItemTweaks.BindConfig<float>(
                "Pauls Goat Hoof",
                "Initial Bonus",
                0.14f,
                "Sets the initial bonus Hoof grants; percentage value."
                );
            var StackHoof = ItemTweaks.BindConfig<float>(
                "Pauls Goat Hoof",
                "Stack Bonus",
                0.09f,
                "Sets the bonus Hoof grants per additional stack; percentage value."
                );

            if (EnableHoof.Value) {
                //description change
                LanguageAPI.Add("ITEM_HOOF_DESC", "Increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>" + (InitialHoof.Value * 100).ToString() + "%</style> <style=cStack>(+" + (StackHoof.Value * 100).ToString() + "% per stack)</style>.");

                IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                    //Locate hoof speed calculations
                    ILCursor c = new ILCursor(il);

                    int hoofCountLoc = 0;

                    c.GotoNext(MoveType.After,
                        x => x.MatchLdcI4((int)ItemIndex.Hoof),
                        x => x.MatchCallOrCallvirt<Inventory>("GetItemCount"),
                        x => x.MatchStloc(out hoofCountLoc)
                    );

                    c.GotoNext(MoveType.After,
                        x => x.MatchLdloc(hoofCountLoc),
                        x => x.MatchConvR4()
                    );

                    //remove the default instructions for paul's goat hoof; we makin our own
                    c.RemoveRange(3);
                    c.EmitDelegate<Func<float, float, float>>((speed, hoofs) => {
                        if (hoofs > 0) {
                            speed += InitialHoof.Value; //Get the fixed bonus
                            speed += StackHoof.Value * (hoofs - 1); //Add the bonus per hoof
                        }
                        return speed;
                    });
                };
            }
        }
    }
}
