using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class WeaponStatsHolder : EquipmentStatsHolder
    {
        public float AttackSpeed;
        public List<DamageHolder> BaseDamage = new List<DamageHolder>();
        public float Impact;

        public int AttackCount;
        public WeaponStats.AttackData[] Attacks;

        public static WeaponStatsHolder ParseWeaponStats(WeaponStats stats, EquipmentStatsHolder equipmentStatsHolder)
        {
            var weaponStatsHolder = new WeaponStatsHolder
            {
                AttackCount = stats.AttackCount,
                Attacks = stats.Attacks,
                AttackSpeed = stats.AttackSpeed,
                Impact = stats.Impact
            };

            weaponStatsHolder.BaseDamage = DamageHolder.ParseDamageList(stats.BaseDamage);

            At.InheritBaseValues(weaponStatsHolder, equipmentStatsHolder);

            return weaponStatsHolder;
        }
    }
}
