using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;

namespace ItemTweaks {
    
    /** TODO LIST:
     * Fireworks? On equipment use? Some might say fireworks are already stupid strong and gesture abuse exists but gesture is generally a dumb item
     * Sticky Bomb? 20% Damage per stack?
     * Queen's Gland? Beeble stats? Make it draw aggro (check if that's a thing)? part of problem I think is beeble can't fight air units
     * Happiest Mask chance? Damage? Make ghosts explode or something? mask is guaranteed but has a cooldown?
     * Option for Urn to attack everyone but only tar enemies? Probably would have to check target team before sending attack and decide accordingly but idk how much sorcery that is
     * Make Rusted Key cache more visible or something?
     * Increase Ghor's Tome chance?
     * Resonance Disk? Prioritize high HP enemies
     * Frost Relic -- investigate how in the duct taped popsicle sticks this thing even works
     */

    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("dev.ontrigger.itemstats", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)] //technically this is default behavior but w/e
    [BepInPlugin("com.Horseyboi.ItemTweaks", "Item Tweaks", "1.4.3")]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]

    public class ItemTweaks : BaseUnityPlugin {

        internal static ItemTweaks instance { get; set; }

        internal enum RAPSettingMode {
            Percent,
            Fixed,
            Armor
        };

        internal enum SettingMode {
            Percent,
            Fixed
        };

        public void Awake() {
            instance = this;

            //Run all the tweaks
            //Common
            Tweaks.ArmorPlates.ChangeItem(); //not sure how to go about improving this hook ngl
            Tweaks.EnergyDrink.ChangeItem();
            Tweaks.FreshMeat.ChangeItem(); //to fix
            Tweaks.GoatHoof.ChangeItem();

            //Uncommon
            Tweaks.LeechingSeed.ChangeItem(); //to fix

            //Rare
            Tweaks.FrostRelic.ChangeItem(); //to fix

            //Boss
            Tweaks.HalcyonSeed.ChangeItem(); //to fix
            Tweaks.MiredUrn.ChangeItem();
            Tweaks.QueensGland.ChangeItem(); //to fix
            Tweaks.TitanicKnurl.ChangeItem(); //to fix

            //Lunar
            Tweaks.GestureDrowned.ChangeItem();

            //check if itemstats is installed so I can funk out some better item descs
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.ontrigger.itemstats")) {
                AdvTooltips.AddTooltips();
            }
        }

        static internal ConfigEntry<T> BindConfig<T>(string section, string key, T val, string desc) {
            return instance.Config.Bind<T>(section, key, val, desc);
        }
    }
}
