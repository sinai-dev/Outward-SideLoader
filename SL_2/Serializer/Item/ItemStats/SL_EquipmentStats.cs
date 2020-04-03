using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_EquipmentStats : SL_ItemStats
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

        public void ApplyToItem(EquipmentStats stats)
        {
            At.SetValue(this.Damage_Resistance, typeof(EquipmentStats), stats, "m_damageResistance");
            At.SetValue(this.Impact_Resistance, typeof(EquipmentStats), stats, "m_impactResistance");
            At.SetValue(new float[9] { this.Damage_Protection, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, typeof(EquipmentStats), stats, "m_damageProtection");
            At.SetValue(this.Damage_Bonus, typeof(EquipmentStats), stats, "m_damageAttack");
            At.SetValue(this.Stamina_Use_Penalty, typeof(EquipmentStats), stats, "m_staminaUsePenalty");
            At.SetValue(this.Mana_Use_Modifier, typeof(EquipmentStats), stats, "m_manaUseModifier");
            At.SetValue(this.Pouch_Bonus, typeof(EquipmentStats), stats, "m_pouchCapacityBonus");
            At.SetValue(this.Heat_Protection, typeof(EquipmentStats), stats, "m_heatProtection");
            At.SetValue(this.Cold_Protection, typeof(EquipmentStats), stats, "m_coldProtection");
            At.SetValue(this.Corruption_Protection, typeof(EquipmentStats), stats, "m_corruptionProtection");

            if (this is SL_WeaponStats weaponStatsHolder)
            {
                weaponStatsHolder.ApplyToItem(stats as WeaponStats);
            }
        }

        public static SL_EquipmentStats ParseEquipmentStats(EquipmentStats stats, SL_ItemStats itemStatsHolder)
        {
            var equipmentStatsHolder = new SL_EquipmentStats();
            
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
