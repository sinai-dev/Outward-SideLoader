using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_WeaponStats : SL_EquipmentStats
    {
        public float AttackSpeed;
        public List<SL_Damage> BaseDamage = new List<SL_Damage>();
        public float Impact;

        //public int AttackCount;
        public WeaponStats.AttackData[] Attacks;

        public void ApplyToItem(WeaponStats stats)
        {
            stats.AttackSpeed = this.AttackSpeed;
            stats.Impact = this.Impact;
            stats.BaseDamage = SL_Damage.GetDamageList(this.BaseDamage);

            for (int i = 0; i < this.Attacks.Length && i < stats.Attacks.Length; i++)
            {
                var data = stats.Attacks[i];
                var template = this.Attacks[i];

                data.AttackSpeed = template.AttackSpeed;
                data.Damage = template.Damage;
                data.Knockback = template.Knockback;
                data.StamCost = template.StamCost;
            }
        }

        public static SL_WeaponStats ParseWeaponStats(WeaponStats stats, SL_EquipmentStats equipmentStatsHolder)
        {
            var weaponStatsHolder = new SL_WeaponStats
            {
                //AttackCount = stats.AttackCount,
                Attacks = stats.Attacks,
                AttackSpeed = stats.AttackSpeed,
                Impact = stats.Impact
            };

            weaponStatsHolder.BaseDamage = SL_Damage.ParseDamageList(stats.BaseDamage);

            At.InheritBaseValues(weaponStatsHolder, equipmentStatsHolder);

            return weaponStatsHolder;
        }
    }
}
