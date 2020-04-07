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
        public bool Unblockable = false;
        public SwingSoundWeapon SwingSound = SwingSoundWeapon.Default;
        public bool SpecialIsZoom = false;
        public int MaxProjectileShots = 1;

        public void ApplyToItem(Weapon item)
        {
            item.Type = this.WeaponType;
            item.Unblockable = this.Unblockable;
            item.SwingSoundType = this.SwingSound;
            item.SpecialIsZoom = this.SpecialIsZoom;
            if (item is ProjectileWeapon projectile && projectile.GetComponent<WeaponLoadout>() is WeaponLoadout loadout)
            {
                // must be >= 1
                loadout.MaxProjectileLoaded = (MaxProjectileShots <= 0) ? 1 : MaxProjectileShots;
            }
        }

        public static SL_Weapon ParseWeapon(Weapon weapon, SL_Equipment equipmentHolder)
        {
            var weaponHolder = new SL_Weapon
            {
                WeaponType = weapon.Type,
                Unblockable = weapon.Unblockable,
                SwingSound = weapon.SwingSoundType,
                SpecialIsZoom = weapon.SpecialIsZoom,
                MaxProjectileShots = -1,
            };

            if (weapon.GetComponent<WeaponLoadout>() is WeaponLoadout loadout)
            {
                weaponHolder.MaxProjectileShots = loadout.MaxProjectileLoaded;
            }

            At.InheritBaseValues(weaponHolder, equipmentHolder);

            weaponHolder.StatsHolder = SL_WeaponStats.ParseWeaponStats(weapon.Stats, equipmentHolder.StatsHolder as SL_EquipmentStats);

            return weaponHolder;
        }
    }
}
