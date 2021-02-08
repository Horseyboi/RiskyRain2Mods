using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ItemTweaks.Tweaks {
    internal static class QueensGland {

        internal static void ChangeItem() {
            //Make the config and all that
            var EnableGland = ItemTweaks.BindConfig<bool>(
                "Queens Gland",
                "Enabled",
                true,
                "Prevents friendly Beetle Guards from trying retaliate if you accidentally harm one."
                );

            if (EnableGland.Value) {

                IL.RoR2.CharacterBody.UpdateBeetleGuardAllies += (il) => {
                    //Match the location where beeble guard is spawned
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchStloc(5),
                        x => x.MatchLdloc(3),
                        x => x.MatchCallvirt<UnityEngine.GameObject>("GetComponent"),
                        x => x.MatchStloc(6)
                        );
                    c.Index += 4;
                    c.Emit(OpCodes.Ldloc, 6);
                    c.EmitDelegate<Action<RoR2.CharacterAI.BaseAI>>((ai) => {
                        ai.neverRetaliateFriendlies = true;
                    });
                };
            }
        }

    }
}
