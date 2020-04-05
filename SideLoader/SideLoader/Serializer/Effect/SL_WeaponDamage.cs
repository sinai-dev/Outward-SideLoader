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

        public void ApplyToComponent(WeaponDamage weaponDamage)
        {
            weaponDamage.ForceOnlyLeftHand = this.ForceOnlyLeftHand;
            weaponDamage.OverrideDType = this.OverrideType;
            weaponDamage.WeaponDamageMult = this.Damage_Multiplier;
            weaponDamage.WeaponDamageMultKBack = this.Damage_Multiplier_Kback;
            weaponDamage.WeaponDamageMultKDown = this.Damage_Multiplier_Kdown;
            weaponDamage.WeaponKnockbackMult = this.Impact_Multiplier;
            weaponDamage.WeaponKnockbackMultKBack = this.Impact_Multiplier_Kback;
            weaponDamage.WeaponKnockbackMultKDown = this.Impact_Multiplier_Kdown;
        }

        public static SL_WeaponDamage ParseWeaponDamage(WeaponDamage weaponDamage, SL_PunctualDamage punctualDamageHolder)
        {
            var weaponDamageHolder = new SL_WeaponDamage
            {
                ForceOnlyLeftHand = weaponDamage.ForceOnlyLeftHand,
                OverrideType = weaponDamage.OverrideDType,
                Damage_Multiplier = weaponDamage.WeaponDamageMult,
                Damage_Multiplier_Kback = weaponDamage.WeaponDamageMultKBack,
                Damage_Multiplier_Kdown = weaponDamage.WeaponDamageMultKDown,
                Impact_Multiplier = weaponDamage.WeaponKnockbackMult,
                Impact_Multiplier_Kback = weaponDamage.WeaponKnockbackMultKBack,
                Impact_Multiplier_Kdown = weaponDamage.WeaponKnockbackMultKDown
            };

            At.InheritBaseValues(weaponDamageHolder, punctualDamageHolder);

            return weaponDamageHolder;
        }
    }
}
