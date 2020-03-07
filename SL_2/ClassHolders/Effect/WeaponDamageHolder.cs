using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class WeaponDamageHolder : PunctualDamageHolder
    {
        // todo
        public string OverrideType;

        public float Damage_Multiplier;
        public float Damage_Multiplier_Kback;
        public float Damage_Multiplier_Kdown;

        public float Impact_Multiplier;
        public float Impact_Multiplier_Kback;
        public float Impact_Multiplier_Kdown;

        public static WeaponDamageHolder ParseWeaponDamage(WeaponDamage weaponDamage, PunctualDamageHolder punctualDamageHolder)
        {
            var weaponDamageHolder = new WeaponDamageHolder
            {
                OverrideType = weaponDamage.OverrideDType.ToString(),
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
