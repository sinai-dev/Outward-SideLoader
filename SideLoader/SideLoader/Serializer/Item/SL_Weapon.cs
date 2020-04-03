using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Weapon : SL_Equipment
    {
        public Weapon.WeaponType WeaponType;

        public void ApplyToItem(Weapon item)
        {
            item.Type = this.WeaponType;
        }

        public static SL_Weapon ParseWeapon(Weapon weapon, SL_Equipment equipmentHolder)
        {
            var weaponHolder = new SL_Weapon
            {
                WeaponType = weapon.Type
            };

            At.InheritBaseValues(weaponHolder, equipmentHolder);

            weaponHolder.StatsHolder = SL_WeaponStats.ParseWeaponStats(weapon.Stats, equipmentHolder.StatsHolder as SL_EquipmentStats);

            return weaponHolder;
        }
    }
}
