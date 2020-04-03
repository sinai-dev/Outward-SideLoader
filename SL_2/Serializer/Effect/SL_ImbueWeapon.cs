using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_ImbueWeapon : SL_Effect
    {
        public float Lifespan;
        public int ImbueEffect_Preset_ID;
        public Weapon.WeaponSlot Imbue_Slot;

        public new void ApplyToTransform(Transform t)
        {
            var preset = ResourcesPrefabManager.Instance.GetEffectPreset(this.ImbueEffect_Preset_ID);

            if (!preset)
            {
                SL.Log("Could not find imbue effect preset of ID " + this.ImbueEffect_Preset_ID);
                return;
            }

            var component = t.gameObject.AddComponent<ImbueWeapon>();
            component.SetLifespanImbue(this.Lifespan);
            component.ImbuedEffect = preset as ImbueEffectPreset;
            component.AffectSlot = this.Imbue_Slot;
        }

        public static SL_ImbueWeapon ParseImbueWeapon(ImbueWeapon imbueWeapon, SL_Effect _effectHolder)
        {
            var imbueWeaponHolder = new SL_ImbueWeapon
            {
                ImbueEffect_Preset_ID = imbueWeapon.ImbuedEffect.PresetID,
                Imbue_Slot = imbueWeapon.AffectSlot,
                Lifespan = imbueWeapon.LifespanImbue
            };

            At.InheritBaseValues(imbueWeaponHolder, _effectHolder);

            return imbueWeaponHolder;
        }
    }
}
