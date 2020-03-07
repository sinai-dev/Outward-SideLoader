using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class ImbueWeaponHolder : EffectHolder
    {
        public float Lifespan;
        public string ImbueEffect_Preset_Name;
        public int ImbueEffect_Preset_ID;
        public string Imbue_Slot;

        public static ImbueWeaponHolder ParseImbueWeapon(ImbueWeapon imbueWeapon, EffectHolder _effectHolder)
        {
            var imbueWeaponHolder = new ImbueWeaponHolder
            {
                ImbueEffect_Preset_Name = imbueWeapon.ImbuedEffect.Name,
                ImbueEffect_Preset_ID = imbueWeapon.ImbuedEffect.PresetID,
                Imbue_Slot = imbueWeapon.AffectSlot.ToString(),
                Lifespan = imbueWeapon.LifespanImbue
            };

            At.InheritBaseValues(imbueWeaponHolder, _effectHolder);

            return imbueWeaponHolder;
        }
    }
}
