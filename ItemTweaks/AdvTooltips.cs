using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;
using static ItemTweaks.ItemTweaks;

namespace ItemTweaks {
    internal static class AdvTooltips {

        internal static void AddTooltips() {
            //grab repulsion armor plate config
            ConfigEntry<bool> EnablePlates;
            ItemTweaks.instance.Config.TryGetEntry<bool>("Repulsion Armor Plate", "Enabled", out EnablePlates);
            ConfigEntry<RAPSettingMode> PlateDRType;
            ItemTweaks.instance.Config.TryGetEntry<RAPSettingMode>("Repulsion Armor Plate", "Damage Resistance Type", out PlateDRType);
            ConfigEntry<float> PlateDR;
            ItemTweaks.instance.Config.TryGetEntry<float>("Repulsion Armor Plate", "Damage Resistance", out PlateDR);

            //grab titanic knurl config
            ConfigEntry<bool> EnableKnurl;
            ItemTweaks.instance.Config.TryGetEntry<bool>("Titanic Knurl", "Enabled", out EnableKnurl);
            ConfigEntry<SettingMode> KnurlMode;
            ItemTweaks.instance.Config.TryGetEntry<SettingMode>("Titanic Knurl", "Health Increase Type", out KnurlMode);
            ConfigEntry<float> KnurlHealth;
            ItemTweaks.instance.Config.TryGetEntry<float>("Titanic Knurl", "Health Increase", out KnurlHealth);
            ConfigEntry <float> KnurlRegen;
            ItemTweaks.instance.Config.TryGetEntry<float>("Titanic Knurl", "Regen Increase", out KnurlRegen);

            //grab goat hoof config
            ConfigEntry<bool> EnableHoof;
            ItemTweaks.instance.Config.TryGetEntry<bool>("Pauls Goat Hoof", "Enabled", out EnableHoof);
            ConfigEntry<float> InitialHoof;
            ItemTweaks.instance.Config.TryGetEntry<float>("Pauls Goat Hoof", "Initial Bonus", out InitialHoof);
            ConfigEntry<float> StackHoof;
            ItemTweaks.instance.Config.TryGetEntry <float>("Pauls Goat Hoof", "Stack Bonus", out StackHoof);

            //grab energy drink config
            ConfigEntry<bool> EnableDrink;
            ItemTweaks.instance.Config.TryGetEntry<bool>("Energy Drink", "Enabled", out EnableDrink);
            ConfigEntry<float> InitialDrink;
            ItemTweaks.instance.Config.TryGetEntry<float>("Energy Drink", "Initial Bonus", out InitialDrink);
            ConfigEntry<float> StackDrink;
            ItemTweaks.instance.Config.TryGetEntry<float>("Energy Drink", "Stack Bonus", out StackDrink);

            //grab fresh meat config
            ConfigEntry<bool> EnableMeat;
            ItemTweaks.instance.Config.TryGetEntry<bool>("Fresh Meat", "Enabled", out EnableMeat);
            ConfigEntry<float> BaseMeatDur;
            ItemTweaks.instance.Config.TryGetEntry<float>("Fresh Meat", "Base Duration", out BaseMeatDur);
            ConfigEntry<float> StackMeatDur;
            ItemTweaks.instance.Config.TryGetEntry<float>("Fresh Meat", "Stack Duration", out StackMeatDur);
            ConfigEntry<float> BaseMeatRegen;
            ItemTweaks.instance.Config.TryGetEntry<float>("Fresh Meat", "Base Regen", out BaseMeatRegen);
            ConfigEntry<float> StackMeatRegen;
            ItemTweaks.instance.Config.TryGetEntry<float>("Fresh Meat", "Stack Regen", out StackMeatRegen);

            //grab gesture of the drowned config
            ConfigEntry<bool> EnableGesture;
            ItemTweaks.instance.Config.TryGetEntry<bool>("Gesture of the Drowned", "Enabled", out EnableGesture);
            ConfigEntry<float> GestureFailChance;
            ItemTweaks.instance.Config.TryGetEntry<float>("Gesture of the Drowned", "Fail Chance", out GestureFailChance);

            //set new desc for armor plates
            if (EnablePlates.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.ArmorPlate);
                if (PlateDRType.Value == RAPSettingMode.Percent) {                    
                    itemDef.Stats = new List<ItemStat> {
                        new ItemStat(
                            (itemCount, ctx) => itemCount * PlateDR.Value,
                            (value, ctx) => $"Reduced damage: {value.FormatPercentage()} Max HP"
                        )
                    };
                } else if (PlateDRType.Value == ItemTweaks.RAPSettingMode.Fixed) {
                    itemDef.Stats = new List<ItemStat> {
                        new ItemStat(
                            (itemCount, ctx) => itemCount * PlateDR.Value,
                            (value, ctx) => $"Reduced damage: {value.FormatInt()}"
                        )
                    };
                } else if (PlateDRType.Value == RAPSettingMode.Armor) {
                    itemDef.Stats = new List<ItemStat> {
                        new ItemStat(
                            (itemCount, ctx) => itemCount * PlateDR.Value,
                            (value, ctx) => $"Armor bonus: {value.FormatInt()}"
                        )
                    };
                }
            }

            //set new desc for titanic knurl
            if (EnableKnurl.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.Knurl);
                if (KnurlMode.Value == ItemTweaks.SettingMode.Percent) {
                    itemDef.Stats[0] = new ItemStat(
                        (itemCount, ctx) => itemCount * KnurlHealth.Value,
                        (value, ctx) => $"Bonus Health: {value.FormatPercentage()}"
                    );
                } else if (KnurlMode.Value == ItemTweaks.SettingMode.Fixed) {
                    itemDef.Stats[0] = new ItemStat(
                        (itemCount, ctx) => itemCount * KnurlHealth.Value,
                        (value, ctx) => $"Bonus Health: {value.FormatInt("HP")}"
                    );
                }
                itemDef.Stats[1] = new ItemStat(
                        (itemCount, ctx) => itemCount * KnurlRegen.Value,
                        (value, ctx) => $"Bonus Health Regen: {value.FormatInt("HP/s", 1, true)}"
                    );
            }

            //set new desc for goat hoof
            if (EnableHoof.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.Hoof);
                itemDef.Stats = new List<ItemStat> {
                    new ItemStat(
                        (itemCount, ctx) => (InitialHoof.Value - StackHoof.Value) + itemCount * StackHoof.Value,
                        (value, ctx) => $"Movement Speed Increase: {value.FormatPercentage()}"
                    )
                };
            }

            //set new desc for energy drink
            if (EnableDrink.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.SprintBonus);
                itemDef.Stats = new List<ItemStat> {
                    new ItemStat(
                        (itemCount, ctx) => (InitialDrink.Value - StackDrink.Value) + itemCount * StackDrink.Value,
                        (value, ctx) => $"Sprint Increase: {value.FormatPercentage()}"
                    )
                };
            }

            //set new desc for fresh meat
            if (EnableMeat.Value)
            {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.RegenOnKill);
                itemDef.Stats = new List<ItemStat>
                {
                    new ItemStat(
                        (itemCount, ctx) => BaseMeatDur.Value + (itemCount - 1) * StackMeatDur.Value,
                        (value, ctx) => $"Buff Duration: {value.FormatInt("s")}"
                        ),
                    new ItemStat(
                        (itemCount, ctx) => BaseMeatRegen.Value + (itemCount - 1) * BaseMeatRegen.Value,
                        (value, ctx) => $"Bonus Health Regen: {value.FormatInt("HP/s", 1, true)}"
                        )
                };
            }

            //set new desc for gesture of the drowned
            if (EnableGesture.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.AutoCastEquipment);
                itemDef.Stats.Add(
                    new ItemStat(
                        (itemCount, ctx) => GestureFailChance.Value,
                        (value, ctx) => $"Equipment Fail Chance: {value.FormatPercentage()}"
                        )
                   );
            }
        }
    }
}
