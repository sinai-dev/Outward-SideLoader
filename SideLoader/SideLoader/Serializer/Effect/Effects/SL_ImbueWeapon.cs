using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ImbueWeapon : SL_Effect
    {
        public float Lifespan;
        public int ImbueEffect_Preset_ID;
        public Weapon.WeaponSlot Imbue_Slot;

        public override void ApplyToComponent<T>(T component)
        {
            var preset = ResourcesPrefabManager.Instance.GetEffectPreset(this.ImbueEffect_Preset_ID);

            if (!preset)
            {
                SL.Log("Could not find imbue effect preset of ID " + this.ImbueEffect_Preset_ID);
                return;
            }

            (component as ImbueWeapon).SetLifespanImbue(this.Lifespan);
            (component as ImbueWeapon).ImbuedEffect = preset as ImbueEffectPreset;
            (component as ImbueWeapon).AffectSlot = this.Imbue_Slot;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_ImbueWeapon).ImbueEffect_Preset_ID = (effect as ImbueWeapon).ImbuedEffect.PresetID;
            (holder as SL_ImbueWeapon).Imbue_Slot = (effect as ImbueWeapon).AffectSlot;
            (holder as SL_ImbueWeapon).Lifespan = (effect as ImbueWeapon).LifespanImbue;
        }
    }
}
