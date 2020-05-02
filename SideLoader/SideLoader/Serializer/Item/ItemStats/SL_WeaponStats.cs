using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_WeaponStats : SL_EquipmentStats
    {
        public float? AttackSpeed;
        public List<SL_Damage> BaseDamage = new List<SL_Damage>();
        public float? Impact;
        public float? StamCost;

        public bool AutoGenerateAttackData;
        public WeaponStats.AttackData[] Attacks;

        public void ApplyToItem(WeaponStats stats)
        {
            if (this.AttackSpeed != null)
            {
                stats.AttackSpeed = (float)this.AttackSpeed;
            }
            if (this.Impact != null)
            {
                stats.Impact = (float)this.Impact;
            }
            if (this.BaseDamage != null)
            {
                stats.BaseDamage = SL_Damage.GetDamageList(this.BaseDamage);
            }
            if (this.StamCost != null)
            {
                stats.StamCost = (float)this.StamCost;
            }

            // fix for m_activeBaseDamage
            var weapon = stats.GetComponent<Weapon>();
            At.SetValue(stats.BaseDamage, typeof(Weapon), weapon, "m_activeBaseDamage");

            if (AutoGenerateAttackData || this.Attacks == null || this.Attacks.Length <= 0)
            {
                SL.Log("Generating AttackData automatically");
                stats.Attacks = GetScaledAttackData(stats.GetComponent<Weapon>());
            }
            else
            {
                var newAttacks = new List<WeaponStats.AttackData>();
                foreach (var attack in this.Attacks)
                {
                    var data = new WeaponStats.AttackData()
                    {
                        Knockback = attack.Knockback,
                        StamCost = attack.StamCost,
                        Damage = attack.Damage
                    };
                    newAttacks.Add(data);
                }
                stats.Attacks = newAttacks.ToArray();
            }          
        }

        public static SL_WeaponStats ParseWeaponStats(WeaponStats stats, SL_EquipmentStats equipmentStatsHolder)
        {
            var weaponStatsHolder = new SL_WeaponStats
            {
                //AttackCount = stats.AttackCount,
                Attacks = stats.Attacks,
                AttackSpeed = stats.AttackSpeed,
                Impact = stats.Impact,
                StamCost = stats.StamCost
            };

            weaponStatsHolder.BaseDamage = SL_Damage.ParseDamageList(stats.BaseDamage);

            At.InheritBaseValues(weaponStatsHolder, equipmentStatsHolder);

            return weaponStatsHolder;
        }

        public static WeaponStats.AttackData[] GetScaledAttackData(Weapon weapon)
        {
            var type = weapon.Type;
            var stats = weapon.GetComponent<WeaponStats>();
            var damage = stats.BaseDamage;
            var impact = stats.Impact;
            var stamCost = stats.StamCost;

            if (!WeaponBaseDataDict.ContainsKey(type))
            {
                return new WeaponStats.AttackData[]
                {
                    new WeaponStats.AttackData()
                    {
                        Damage = DamageListToFloatArray(damage, 1.0f),
                        Knockback = impact,
                        StamCost = stamCost
                    }
                };
            }

            var basedata = WeaponBaseDataDict[type];

            var list = new List<WeaponStats.AttackData>();

            for (int i = 0; i < 5; i++)
            {
                var attackdata = new WeaponStats.AttackData
                {
                    Damage = DamageListToFloatArray(damage, basedata.DamageMult[i]),
                    Knockback = impact * basedata.ImpactMult[i],
                    StamCost = stamCost * basedata.StamMult[i]
                };
                list.Add(attackdata);
            }

            return list.ToArray();
        }

        private static List<float> DamageListToFloatArray(DamageList list, float multiplier)
        {
            var floats = new List<float>();

            foreach (var type in list.List)
            {
                floats.Add(type.Damage * multiplier);
            }

            return floats;
        }

        public static Dictionary<Weapon.WeaponType, WeaponStatData> WeaponBaseDataDict = new Dictionary<Weapon.WeaponType, WeaponStatData>()
        {
            {
                Weapon.WeaponType.Sword_1H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.3f, 1.1f, 1.1f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.3f, 1.1f, 1.1f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.2f, 1.1f, 1.1f }
                }
            },
            {
                Weapon.WeaponType.Sword_2H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.5f, 1.1f, 1.1f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.5f, 1.1f, 1.1f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.3f, 1.1f, 1.1f } 
                }
            },
            {
                Weapon.WeaponType.Axe_1H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.3f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.3f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.2f, 1.2f, 1.2f } 
                }
            },
            {
                Weapon.WeaponType.Axe_2H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.3f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.3f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.2f, 1.1f, 1.1f } 
                }
            },
            {
                Weapon.WeaponType.Mace_1H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.3f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 2.5f, 1.3f, 1.3f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.3f } 
                }
            },
            {
                Weapon.WeaponType.Mace_2H,
                new WeaponStatData()
                {   //                         1     2     3      4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 0.75f, 1.4f, 1.4f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 2.0f,  1.4f, 1.4f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.2f,  1.2f, 1.2f } 
                }
            },
            {
                Weapon.WeaponType.Halberd_2H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.7f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.3f, 1.3f, 1.7f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.25f, 1.25f, 1.75f } 
                }
            },
            {
                Weapon.WeaponType.Spear_2H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.4f, 1.3f, 1.2f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.2f, 1.2f, 1.1f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.25f, 1.25f, 1.25f }
                }
            },
            {
                Weapon.WeaponType.FistW_2H,
                new WeaponStatData()
                {   //                         1     2     3     4     5
                    DamageMult = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
                    ImpactMult = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
                    StamMult   = new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f }
                }
            }
        };

        public class WeaponStatData
        {
            public float[] DamageMult;
            public float[] ImpactMult;
            public float[] StamMult;
        }
    }
}
