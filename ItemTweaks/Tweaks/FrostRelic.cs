using MonoMod.Cil;
using R2API;

namespace ItemTweaks.Tweaks {
    class FrostRelic {
        internal static void ChangeItem() {
            //Make all the config stuff
            var EnableRelic = ItemTweaks.BindConfig<bool>(
                "Frost Relic",
                "Enabled",
                true,
                "Enables Frost Relic changes."
                );
            var RelicDuration = ItemTweaks.BindConfig<float>(
                "Frost Relic",
                "Base Duration",
                10f,
                "Sets the base duration for Frost Relic aura."
                );
            var RelicCamera = ItemTweaks.BindConfig<bool>(
                "Frost Relic",
                "Disable Camera Change",
                true,
                "Should the camera angle change be removed?"
                );

            if (EnableRelic.Value) {
                //description changes
                string relicDesc = "Killing an enemy surrounds you with an <style=cIsDamage>ice storm</style> that lasts for <style=cIsUtility>" + RelicDuration.Value + " seconds</style> and deals <style=cIsDamage>600% damage per second</style>. The storm <style=cIsDamage>grows with every kill</style>, increasing its radius by <style=cIsDamage>2m</style>. Stacks up to <style=cIsDamage>12m</style> <style=cStack>(+6m per stack)</style>.";
                LanguageAPI.Add("ITEM_ICICLE_DESC", relicDesc);

                On.RoR2.IcicleAuraController.Awake += (orig, self) => {
                    orig(self);
                    self.icicleDuration = RelicDuration.Value;
                };

                if (RelicCamera.Value) {
                    IL.RoR2.IcicleAuraController.OnIciclesActivated += (il) => {
                        ILCursor c = new ILCursor(il); //can't figure out how to do this hook well
                        c.GotoNext(
                            x => x.MatchStloc(0),
                            x => x.MatchLdcI4(0),
                            x => x.MatchStloc(1)
                            );
                        c.Index -= 7;
                        c.RemoveRange(5);
                    };
                }
            }
        }

    }
}
