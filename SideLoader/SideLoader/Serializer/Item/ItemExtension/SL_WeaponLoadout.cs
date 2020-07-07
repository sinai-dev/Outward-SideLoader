using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_WeaponLoadout : SL_ItemExtension
    {
        public WeaponLoadout.CompatibleAmmunitionType? AmmunitionType;
        public int? CompatibleItemID;
        public Weapon.WeaponType? CompatibleEquipmentType;
        public int? MaxProjectileLoaded;
        public bool? SaveRemainingShots;
        public bool? ShowLoadedAmmunition;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as WeaponLoadout;

            if (this.AmmunitionType != null)
            {
                comp.AmunitionType = (WeaponLoadout.CompatibleAmmunitionType)this.AmmunitionType;
            }
            if (this.CompatibleItemID != null)
            {
                if (ResourcesPrefabManager.Instance.GetItemPrefab((int)this.CompatibleItemID) is Item compatibleItem)
                {
                    comp.CompatibleAmmunition = compatibleItem;
                }
            }
            if (this.CompatibleEquipmentType != null)
            {
                comp.CompatibleEquipment = (Weapon.WeaponType)this.CompatibleEquipmentType;
            }
            if (this.MaxProjectileLoaded != null)
            {
                comp.MaxProjectileLoaded = (int)this.MaxProjectileLoaded;
            }
            if (this.SaveRemainingShots != null)
            {
                comp.SaveRemainingShots = (bool)this.SaveRemainingShots;
            }
            if (this.ShowLoadedAmmunition != null)
            {
                comp.ShowLoadedAmmunition = (bool)this.ShowLoadedAmmunition;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as WeaponLoadout;

            this.AmmunitionType = comp.AmunitionType;
            this.CompatibleEquipmentType = comp.CompatibleEquipment;
            this.CompatibleItemID = comp.CompatibleAmmunition?.ItemID;
            this.MaxProjectileLoaded = comp.MaxProjectileLoaded;
            this.SaveRemainingShots = comp.SaveRemainingShots;
            this.ShowLoadedAmmunition = comp.ShowLoadedAmmunition;
        }
    }
}
