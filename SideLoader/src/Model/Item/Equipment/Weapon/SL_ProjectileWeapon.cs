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
                weapon.AutoLoad = (bool)this.AutoLoad;

            if (this.UnloadOnSheathe != null)
                weapon.UnloadOnSheathe = (bool)this.UnloadOnSheathe;

            if (this.UnloadOnEquip != null)
                weapon.UnloadOnUnequip = (bool)this.UnloadOnEquip;

            if (this.UnloadOnIncompleteShot != null)
                weapon.UnloadOnIncompleteShot = (bool)this.UnloadOnIncompleteShot;

            if (this.LocomotionEnabledOnReload != null)
                weapon.LocomotionEnableOnReload = (bool)this.LocomotionEnabledOnReload;

            if (this.LoadAnim != null)
                weapon.LoadAnim = (Character.SpellCastType)this.LoadAnim;

            if (this.FullyBentSound != null)
                weapon.FullyBentSound = (GlobalAudioManager.Sounds)this.FullyBentSound;
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var weapon = item as ProjectileWeapon;

            AutoLoad = weapon.AutoLoad;
            UnloadOnEquip = weapon.UnloadOnUnequip;
            UnloadOnIncompleteShot = weapon.UnloadOnIncompleteShot;
            UnloadOnSheathe = weapon.UnloadOnSheathe;
            LocomotionEnabledOnReload = weapon.LocomotionEnableOnReload;
            LoadAnim = weapon.LoadAnim;
            FullyBentSound = weapon.FullyBentSound;
        }
    }
}
