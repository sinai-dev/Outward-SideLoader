using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ProjectileWeapon : SL_Weapon
    {
        public bool? AutoLoad;
        public bool? UnloadOnSheathe;
        public bool? UnloadOnEquip;
        public bool? UnloadOnIncompleteShot;
        public bool? LocomotionEnabledOnReload;

        public Character.SpellCastType? LoadAnim;
        public GlobalAudioManager.Sounds? FullyBentSound;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var weapon = item as ProjectileWeapon;

            if (this.AutoLoad != null)
            {
                weapon.AutoLoad = (bool)this.AutoLoad;
            }
            if (this.UnloadOnSheathe != null)
            {
                weapon.UnloadOnSheathe = (bool)this.UnloadOnSheathe;
            }
            if (this.UnloadOnEquip != null)
            {
                weapon.UnloadOnUnequip = (bool)this.UnloadOnEquip;
            }
            if (this.UnloadOnIncompleteShot != null)
            {
                weapon.UnloadOnIncompleteShot = (bool)this.UnloadOnIncompleteShot;
            }
            if (this.LocomotionEnabledOnReload != null)
            {
                weapon.LocomotionEnableOnReload = (bool)this.LocomotionEnabledOnReload;
            }
            if (this.LoadAnim != null)
            {
                weapon.LoadAnim = (Character.SpellCastType)this.LoadAnim;
            }
            if (this.FullyBentSound != null)
            {
                weapon.FullyBentSound = (GlobalAudioManager.Sounds)this.FullyBentSound;
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_ProjectileWeapon;
            var weapon = item as ProjectileWeapon;

            template.AutoLoad = weapon.AutoLoad;
            template.UnloadOnEquip = weapon.UnloadOnUnequip;
            template.UnloadOnIncompleteShot = weapon.UnloadOnIncompleteShot;
            template.UnloadOnSheathe = weapon.UnloadOnSheathe;
            template.LocomotionEnabledOnReload = weapon.LocomotionEnableOnReload;
            template.LoadAnim = weapon.LoadAnim;
            template.FullyBentSound = weapon.FullyBentSound;
        }
    }
}
