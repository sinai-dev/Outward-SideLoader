using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Weapon : SL_Equipment
    {
        public Weapon.WeaponType? WeaponType;
        public bool? Unblockable;
        public SwingSoundWeapon? SwingSound;
        public bool? SpecialIsZoom;
        public int? MaxProjectileShots;

        public void ApplyToItem(Weapon item)
        {
            if (this.WeaponType != null)
            {
                item.Type = (Weapon.WeaponType)this.WeaponType;
            }
            if (this.Unblockable != null)
            {
                item.Unblockable = (bool)this.Unblockable;
            }
            if (this.SwingSound != null)
            {
                item.SwingSoundType = (SwingSoundWeapon)this.SwingSound;
            }
            if (this.SpecialIsZoom != null)
            {
                item.SpecialIsZoom = (bool)this.SpecialIsZoom;
            }
            if (this.MaxProjectileShots != null && item is ProjectileWeapon projectile && projectile.GetComponent<WeaponLoadout>() is WeaponLoadout loadout)
            {
                int maxshots = (int)this.MaxProjectileShots;
                // must be >= 1
                loadout.MaxProjectileLoaded = (maxshots <= 0) ? 1 : maxshots;
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

            weaponHolder.StatsHolder = SL_WeaponStats.ParseWeaponStats(weapon.GetComponent<WeaponStats>(), equipmentHolder.StatsHolder as SL_EquipmentStats);

            return weaponHolder;
        }
    }
}
