using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace SideLoader
{
    public class SL_Weapon : SL_Equipment
    {
        [Obsolete("Use a SL_WeaponLoadout in SL_Item.ItemExtensions instead")]
        [XmlIgnore]
        public int? MaxProjectileShots;

        public Weapon.WeaponType? WeaponType;
        public bool? Unblockable;
        public SwingSoundWeapon? SwingSound;
        public bool? SpecialIsZoom;

        public float? HealthLeechRatio;
        public float? HealthBurnLeechRatio;

        public bool? IgnoreHalfResistances;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var weapon = item as Weapon;

            if (this.WeaponType != null)
                weapon.Type = (Weapon.WeaponType)this.WeaponType;

            if (this.Unblockable != null)
                weapon.Unblockable = (bool)this.Unblockable;

            if (this.SwingSound != null)
                weapon.SwingSoundType = (SwingSoundWeapon)this.SwingSound;

            if (this.SpecialIsZoom != null)
                weapon.SpecialIsZoom = (bool)this.SpecialIsZoom;

            if (this.HealthLeechRatio != null)
                weapon.BaseMaxHealthAbsorbRatio = (float)this.HealthLeechRatio;

            if (this.HealthBurnLeechRatio != null)
                weapon.BaseHealthAbsorbRatio = (float)this.HealthBurnLeechRatio;

            if (this.IgnoreHalfResistances != null)
                weapon.IgnoreHalfResistances = (bool)this.IgnoreHalfResistances;
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var weapon = item as Weapon;

            WeaponType = weapon.Type;
            Unblockable = weapon.Unblockable;
            SwingSound = weapon.SwingSoundType;
            SpecialIsZoom = weapon.SpecialIsZoom;
            IgnoreHalfResistances = weapon.IgnoreHalfResistances;
        }
    }
}
