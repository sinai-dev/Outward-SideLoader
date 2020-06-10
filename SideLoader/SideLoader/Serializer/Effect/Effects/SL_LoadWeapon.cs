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

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_LoadWeapon).WeaponSlot = (effect as LoadWeapon).WeaponSlot;
            (holder as SL_LoadWeapon).UnloadFirst = (effect as LoadWeapon).UnloadFirst;
        }
    }
}
