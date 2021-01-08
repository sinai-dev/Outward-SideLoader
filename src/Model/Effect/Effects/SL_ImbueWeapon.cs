using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ImbueWeapon : SL_ImbueObject
    {
        public Weapon.WeaponSlot Imbue_Slot;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as ImbueWeapon;

            comp.AffectSlot = this.Imbue_Slot;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as ImbueWeapon;

            Imbue_Slot = comp.AffectSlot;
        }
    }
}
