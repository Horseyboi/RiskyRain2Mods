using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace ItemTweaks.Tweaks {
    internal static class LeechingSeed {

        internal static void ChangeItem() {
            var EnableSeed = ItemTweaks.BindConfig<bool>(
                "Leeching Seed",
                "Enabled",
                true,
                "Enables Leeching Seed changes."
                );
            var SeedHeal = ItemTweaks.BindConfig<float>(
                "Leeching Seed",
                "Heal Amount",
                3f,
                "How much health Leeching Seed heals on hit."
                );

            if (EnableSeed.Value) {
                IL.RoR2.GlobalEventManager.OnHitEnemy += (il) => {
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchLdloc(19),
                        x => x.MatchLdloc(18),
                        x => x.MatchConvR4(),
                        x => x.MatchLdarg(1)
                        );
                    c.Index += 5;
                    c.Emit(OpCodes.Ldc_R4, SeedHeal.Value);
                    c.Emit(OpCodes.Mul);
                };
            }
        }
    }
}
