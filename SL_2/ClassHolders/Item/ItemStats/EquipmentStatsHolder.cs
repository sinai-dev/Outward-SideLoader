using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class EquipmentStatsHolder : ItemStatsHolder
    {
        public float[] Damage_Resistance;
        public float Impact_Resistance;
        public float Damage_Protection;

        public float[] Damage_Bonus;

        public float Stamina_Use_Penalty;
        public float Mana_Use_Modifier;
        public float Movement_Penalty;

        public float Pouch_Bonus;
        public float Heat_Protection;
        public float Cold_Protection;
        public float Corruption_Protection; // for DLC

        public static EquipmentStatsHolder ParseEquipmentStats(EquipmentStats stats, ItemStatsHolder itemStatsHolder)
        {
            var equipmentStatsHolder = new EquipmentStatsHolder();
            
            if (stats == null || itemStatsHolder == null)
            {
                Debug.LogWarning("Equipment trying to be parsed with no stats");
            }
            else
            {
                try
                {
                    equipmentStatsHolder.Impact_Resistance = stats.ImpactResistance;
                    equipmentStatsHolder.Damage_Protection = stats.GetDamageProtection(DamageType.Types.Physical);
                    equipmentStatsHolder.Stamina_Use_Penalty = stats.StaminaUsePenalty;
                    equipmentStatsHolder.Mana_Use_Modifier = stats.ManaUseModifier;
                    equipmentStatsHolder.Movement_Penalty = stats.MovementPenalty;
                    equipmentStatsHolder.Pouch_Bonus = stats.PouchCapacityBonus;
                    equipmentStatsHolder.Heat_Protection = stats.HeatProtection;
                    equipmentStatsHolder.Cold_Protection = stats.ColdProtection;
                    equipmentStatsHolder.Corruption_Protection = stats.CorruptionProtection;

                    equipmentStatsHolder.Damage_Bonus = At.GetValue(typeof(EquipmentStats), stats, "m_damageAttack") as float[];
                    equipmentStatsHolder.Damage_Resistance = At.GetValue(typeof(EquipmentStats), stats, "m_damageResistance") as float[];

                    At.InheritBaseValues(equipmentStatsHolder, itemStatsHolder);
                }
                catch (Exception e)
                {
                    Debug.Log("Exception getting stats of " + stats.name + "\r\n" + e.Message + "\r\n" + e.StackTrace);
                }
            }

            return equipmentStatsHolder;
        }
    }
}
