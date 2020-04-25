using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_WeaponDamage : SL_PunctualDamage
    {
        // todo
        public DamageType.Types OverrideType;

        public bool ForceOnlyLeftHand;

        public float Damage_Multiplier;
        public float Damage_Multiplier_Kback;
        public float Damage_Multiplier_Kdown;

        public float Impact_Multiplier;
        public float Impact_Multiplier_Kback;
        public float Impact_Multiplier_Kdown;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var weaponDamage = component as WeaponDamage;

            weaponDamage.ForceOnlyLeftHand = this.ForceOnlyLeftHand;
            weaponDamage.OverrideDType = this.OverrideType;
            weaponDamage.WeaponDamageMult = this.Damage_Multiplier;
            weaponDamage.WeaponDamageMultKBack = this.Damage_Multiplier_Kback;
            weaponDamage.WeaponDamageMultKDown = this.Damage_Multiplier_Kdown;
            weaponDamage.WeaponKnockbackMult = this.Impact_Multiplier;
            weaponDamage.WeaponKnockbackMultKBack = this.Impact_Multiplier_Kback;
            weaponDamage.WeaponKnockbackMultKDown = this.Impact_Multiplier_Kdown;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            var weaponDamage = effect as WeaponDamage;
            var wdHolder = holder as SL_WeaponDamage;

            wdHolder.ForceOnlyLeftHand = weaponDamage.ForceOnlyLeftHand;
            wdHolder.OverrideType = weaponDamage.OverrideDType;
            wdHolder.Damage_Multiplier = weaponDamage.WeaponDamageMult;
            wdHolder.Damage_Multiplier_Kback = weaponDamage.WeaponDamageMultKBack;
            wdHolder.Damage_Multiplier_Kdown = weaponDamage.WeaponDamageMultKDown;
            wdHolder.Impact_Multiplier = weaponDamage.WeaponKnockbackMult;
            wdHolder.Impact_Multiplier_Kback = weaponDamage.WeaponKnockbackMultKBack;
            wdHolder.Impact_Multiplier_Kdown = weaponDamage.WeaponKnockbackMultKDown;
        }
    }
}
