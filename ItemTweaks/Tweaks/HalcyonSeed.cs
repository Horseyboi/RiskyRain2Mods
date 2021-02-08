using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using R2API.Utils;

namespace ItemTweaks.Tweaks {
    internal static class HalcyonSeed {

        internal static void ChangeItem() {
            //Make the config and all that
            var EnableHalcyon = ItemTweaks.BindConfig<bool>(
                "Halcyon Seed",
                "Enabled",
                true,
                "Prevents the friendly Aurelionite from trying retaliate if you accidentally harm it."
                );

            if (EnableHalcyon.Value) {

                IL.RoR2.TeleporterInteraction.ChargingState.TrySpawnTitanGoldServer += (il) => {
                    //Match the location where aurelionite is spawned
                    ILCursor c = new ILCursor(il);
                    c.GotoNext(
                        x => x.MatchLdloc(5),
                        x => x.MatchLdloc(1),
                        x => x.MatchConvR4(),
                        x => x.MatchLdcR4(1)
                        );
                    c.Index += 8;
                    c.Emit(OpCodes.Ldarg, 0);
                    c.EmitDelegate<Action<UnityEngine.GameObject>>((telestate) => {
                        var titanMaster = telestate.GetFieldValue<RoR2.CharacterMaster>("titanGoldBossMaster");
                        RoR2.CharacterAI.BaseAI[] ai = titanMaster.GetFieldValue<RoR2.CharacterAI.BaseAI[]>("aiComponents");
                        for (int i = 0; i < ai.Length; i++) {
                            ai[i].neverRetaliateFriendlies = true;
                        }
                    });
                };
            }
        }
    }
}
