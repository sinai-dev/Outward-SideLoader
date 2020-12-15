using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_LoadWeapon : SL_Effect
    {
        public bool UnloadFirst;
        public Weapon.WeaponSlot WeaponSlot;

        public override void ApplyToComponent<T>(T component)
        {
            (component as LoadWeapon).UnloadFirst = this.UnloadFirst;
            (component as LoadWeapon).WeaponSlot = this.WeaponSlot;
        }

        public override void SerializeEffect<T>(T effect)
        {
            WeaponSlot = (effect as LoadWeapon).WeaponSlot;
            UnloadFirst = (effect as LoadWeapon).UnloadFirst;
        }
    }
}
