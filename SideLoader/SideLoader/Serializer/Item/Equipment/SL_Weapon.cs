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

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var weapon = item as Weapon;

            if (this.WeaponType != null)
            {
                weapon.Type = (Weapon.WeaponType)this.WeaponType;
            }
            if (this.Unblockable != null)
            {
                weapon.Unblockable = (bool)this.Unblockable;
            }
            if (this.SwingSound != null)
            {
                weapon.SwingSoundType = (SwingSoundWeapon)this.SwingSound;
            }
            if (this.SpecialIsZoom != null)
            {
                weapon.SpecialIsZoom = (bool)this.SpecialIsZoom;
            }

            if (this.HealthLeechRatio != null)
            {
                weapon.BaseMaxHealthAbsorbRatio = (float)this.HealthLeechRatio;
            }
            if (this.HealthBurnLeechRatio != null)
            {
                weapon.BaseHealthAbsorbRatio = (float)this.HealthBurnLeechRatio;
            }            
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var weapon = item as Weapon;
            var template = holder as SL_Weapon;

            template.WeaponType = weapon.Type;
            template.Unblockable = weapon.Unblockable;
            template.SwingSound = weapon.SwingSoundType;
            template.SpecialIsZoom = weapon.SpecialIsZoom;

            //template.MaxProjectileShots = -1;

            //if (weapon.GetComponent<WeaponLoadout>() is WeaponLoadout loadout)
            //{
            //    template.MaxProjectileShots = loadout.MaxProjectileLoaded;
            //}
        }
    }
}
