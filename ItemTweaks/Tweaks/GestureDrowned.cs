using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace ItemTweaks.Tweaks {
    internal static class GestureDrowned {

        internal static void ChangeItem() {
            //Make all the config stuff
            var EnableGesture = ItemTweaks.BindConfig<bool>(
                "Gesture of the Drowned",
                "Enabled",
                true,
                "Enables Gesture of the Drowned changes."
                );
            var GestureFailChance = ItemTweaks.BindConfig<float>(
                "Gesture of the Drowned",
                "Fail Chance",
                0.10f,
                "Chance for equipment to fail on each use when gesture is active. Percentage value (0.1 = 10%).\nYou'll see shattering glass and no equipment effect if it fails."
                );

            if (EnableGesture.Value) {
                //langauge changes
                LanguageAPI.Add("ITEM_AUTOCASTEQUIPMENT_PICKUP", "Dramatically reduce Equipment cooldown... <color=#FF7F7F>BUT it automatically activates and has a chance to fail.</color>");
                LanguageAPI.Add("ITEM_AUTOCASTEQUIPMENT_DESC", "<style=cIsUtility>Reduce Equipment cooldown</style> by <style=cIsUtility>50%</style> <style=cStack>(+15% per stack)</style>. Forces your Equipment to <style=cIsUtility>activate</style> whenever it is off <style=cIsUtility>cooldown</style>, and it has a <style=cIsHealth>" + GestureFailChance.Value * 100 + "% chance to fail when used</style>.");

                GameObject brittlevfx = Resources.Load<GameObject>("Prefabs/Effects/BrittleDeath"); //TODO: create unique VFX for this maybe

                On.RoR2.EquipmentSlot.ExecuteIfReady += (orig, self) => {
                    Inventory myInv = self.GetFieldValue<Inventory>("inventory");
                    if (self.stock > 0 && self.equipmentIndex != EquipmentIndex.None) {
                        if (myInv.GetItemCount(ItemIndex.AutoCastEquipment) > 0 && !Util.CheckRoll((1 - GestureFailChance.Value) * 100, 0, self.characterBody.master)) { //wacko calc in case I decide to make luck relevant
                            myInv.DeductEquipmentCharges(self.activeEquipmentSlot, 1); //whoops your equipment failed goodbye charge
                            EffectData effectData = new EffectData {
                                origin = self.characterBody.corePosition
                            };
                            EffectManager.SpawnEffect(brittlevfx, effectData, true);
                            return false;
                        } else {
                            orig(self); //you got lucky, this time (assuming you even had gesture)
                        }
                    }
                    return false; //this line should never run but the first if checks the same conditions as the original function so returning false here should be fine
                };
            }
        }
    }
}
