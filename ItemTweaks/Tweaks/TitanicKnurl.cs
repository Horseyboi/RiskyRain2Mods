using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using System;
using static ItemTweaks.ItemTweaks;

namespace ItemTweaks.Tweaks {
    internal static class TitanicKnurl {

        internal static void ChangeItem() {
            //Make the config and all that
            var EnableKnurl = ItemTweaks.BindConfig<bool>(
                "Titanic Knurl",
                "Enabled",
                true,
                "Enables Titanic Knurl changes."
                );
            var KnurlHealthType = ItemTweaks.BindConfig<SettingMode>(
                "Titanic Knurl",
                "Health Increase Type",
                SettingMode.Percent,
                "Sets whether Titanic Knurl's health increase is a fixed value or percentage of maximum health."
                );
            var KnurlHealthIncrease = ItemTweaks.BindConfig<float>(
                "Titanic Knurl",
                "Health Increase",
                0.08f,
                "Sets Titanic Knurl's health increase.\nIn Percent mode, this is a percent of your max health (e.g. 0.15 = 15%); in Fixed mode, this is a static value."
                );
            var KnurlRegenIncrease = ItemTweaks.BindConfig<float>(
                "Titanic Knurl",
                "Regen Increase",
                1.6f,
                "Sets how much health regen Titanic Knurl confers."
                );

            if (EnableKnurl.Value) {
                //description changes
                String knurlDesc = "<style=cIsHealing>Increase maximum health</style> by <style=cIsHealing>";
                if (KnurlHealthType.Value == SettingMode.Percent) {
                    knurlDesc += KnurlHealthIncrease.Value * 100 + "%</style> <style=cStack>(+" + KnurlHealthIncrease.Value * 100 + "% per stack)</style>";
                } else if (KnurlHealthType.Value == SettingMode.Fixed) {
                    knurlDesc += KnurlHealthIncrease.Value + "</style> <style=cStack>(+" + KnurlHealthIncrease.Value + " per stack)</style>";
                }
                knurlDesc += "and <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+" + KnurlRegenIncrease.Value + " hp/s <style=cStack>(+" + KnurlRegenIncrease.Value + " hp/s per stack)</style>.";
                LanguageAPI.Add("ITEM_KNURL_DESC", knurlDesc);

                IL.RoR2.CharacterBody.RecalculateStats += (il) => {
                    //Match the location where Knurl health calculation happens
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

                    //match location where knurl regen calculation happens
                    c.GotoNext(
                        x => x.MatchLdloc(14),
                        x => x.MatchConvR4(),
                        x => x.MatchLdcR4(1.6f),
                        x => x.MatchMul()
                        );
                    c.Index += 2;
                    c.Remove(); //1.6f removed
                    c.Emit(OpCodes.Ldc_R4, KnurlRegenIncrease.Value); //emit our new knurl regen value
                };
            }
        }
    }
}
