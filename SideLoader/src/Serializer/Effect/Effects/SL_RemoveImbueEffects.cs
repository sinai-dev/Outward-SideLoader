using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_RemoveImbueEffects : SL_Effect
    {
        public Weapon.WeaponSlot AffectSlot;

        public override void ApplyToComponent<T>(T component)
        {
            (component as RemoveImbueEffects).AffectSlot = this.AffectSlot;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_RemoveImbueEffects).AffectSlot = (effect as RemoveImbueEffects).AffectSlot;
        }
    }
}
