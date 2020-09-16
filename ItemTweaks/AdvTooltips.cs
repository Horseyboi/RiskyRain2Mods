using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using R2API;
using R2API.Utils;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using ItemStats;
using ItemStats.Stat;
using ItemStats.ValueFormatters;

namespace Horseyboi.ItemTweaks {
    public class AdvTooltips : BaseUnityPlugin {

        public static void DoStuff() {
            //change armor plates
            if (ItemTweaks.EnablePlates.Value) {
                if (ItemTweaks.PlateDRType.Value == ItemTweaks.RAPSettingMode.Percent) {
                    var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.ArmorPlate);
                    itemDef.Stats = new List<ItemStat> {
                        new ItemStat(
                            (itemCount, ctx) => itemCount * ItemTweaks.ArmorPlateDR.Value,
                            (value, ctx) => $"Reduced damage: {value.FormatPercentage()} Max HP"
                        )
                    };
                } else if (ItemTweaks.PlateDRType.Value == ItemTweaks.RAPSettingMode.Fixed) {
                    var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.ArmorPlate);
                    itemDef.Stats = new List<ItemStat> {
                        new ItemStat(
                            (itemCount, ctx) => itemCount * ItemTweaks.ArmorPlateDR.Value,
                            (value, ctx) => $"Reduced damage: {value.FormatInt()}"
                        )
                    };
                } else if (ItemTweaks.PlateDRType.Value == ItemTweaks.RAPSettingMode.Armor) {
                    var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.ArmorPlate);
                    itemDef.Stats = new List<ItemStat> {
                        new ItemStat(
                            (itemCount, ctx) => itemCount * ItemTweaks.ArmorPlateDR.Value,
                            (value, ctx) => $"Armor bonus: {value.FormatInt()}"
                        )
                    };
                }
            }

            if (ItemTweaks.EnableKnurl.Value) {
                if (ItemTweaks.KnurlHealthType.Value == ItemTweaks.SettingMode.Percent) {
                    var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.Knurl);
                    itemDef.Stats[0] = new ItemStat(
                        (itemCount, ctx) => itemCount * ItemTweaks.KnurlHealthIncrease.Value,
                        (value, ctx) => $"Bonus Health: {value.FormatPercentage()}"
                    );
                } else if (ItemTweaks.KnurlHealthType.Value == ItemTweaks.SettingMode.Fixed) {
                    var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.Knurl);
                    itemDef.Stats[0] = new ItemStat(
                        (itemCount, ctx) => itemCount * ItemTweaks.KnurlHealthIncrease.Value,
                        (value, ctx) => $"Bonus Health: {value.FormatInt("HP")}"
                    );
                }
            }

            if (ItemTweaks.EnableHoof.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.Hoof);
                itemDef.Stats = new List<ItemStat> {
                    new ItemStat(
                        (itemCount, ctx) => (ItemTweaks.InitialHoof.Value - ItemTweaks.StackHoof.Value) + itemCount * ItemTweaks.StackHoof.Value,
                        (value, ctx) => $"Movement Speed Increase: {value.FormatPercentage()}"
                    )
                };
            }

            if (ItemTweaks.EnableNRG.Value) {
                var itemDef = ItemStatsMod.GetItemStatDef(ItemIndex.SprintBonus);
                itemDef.Stats = new List<ItemStat> {
                    new ItemStat(
                        (itemCount, ctx) => (ItemTweaks.InitialNRG.Value - ItemTweaks.StackNRG.Value) + itemCount * ItemTweaks.StackNRG.Value,
                        (value, ctx) => $"Sprint Increase: {value.FormatPercentage()}"
                    )
                };
            }
        }
    }
}