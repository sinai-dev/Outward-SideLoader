using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_EquipmentStats : SL_ItemStats
    {
        public float[] Damage_Resistance = new float[9];
        public float? Impact_Resistance;
        public float? Damage_Protection;

        public float[] Damage_Bonus = new float[9];

        public float? Stamina_Use_Penalty;
        public float? Mana_Use_Modifier;
        public float? Movement_Penalty;

        public float? Pouch_Bonus;
        public float? Heat_Protection;
        public float? Cold_Protection;
        public float? Corruption_Protection; // for DLC

        public void ApplyToItem(EquipmentStats stats)
        {
            if (this.Damage_Resistance != null && this.Damage_Resistance.Length > 0)
            {
                At.SetValue(this.Damage_Resistance, typeof(EquipmentStats), stats, "m_damageResistance");
            }
            if (this.Impact_Resistance != null)
            {
                At.SetValue((float)this.Impact_Resistance, typeof(EquipmentStats), stats, "m_impactResistance");
            }
            if (this.Damage_Protection != null)
            {
                At.SetValue(new float[9] { (float)this.Damage_Protection, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, typeof(EquipmentStats), stats, "m_damageProtection");
            }
            if (this.Damage_Bonus != null && this.Damage_Bonus.Length > 0)
            {
                At.SetValue(this.Damage_Bonus, typeof(EquipmentStats), stats, "m_damageAttack");
            }
            if (this.Stamina_Use_Penalty != null)
            {
                At.SetValue((float)this.Stamina_Use_Penalty, typeof(EquipmentStats), stats, "m_staminaUsePenalty");
            }
            if (this.Mana_Use_Modifier != null)
            {
                At.SetValue((float)this.Mana_Use_Modifier, typeof(EquipmentStats), stats, "m_manaUseModifier");
            }
            if (this.Movement_Penalty != null)
            {
                At.SetValue((float)this.Movement_Penalty, typeof(EquipmentStats), stats, "m_movementPenalty");
            }
            if (this.Pouch_Bonus != null)
            {
                At.SetValue((float)this.Pouch_Bonus, typeof(EquipmentStats), stats, "m_pouchCapacityBonus");
            }
            if (this.Heat_Protection != null)
            {
                At.SetValue((float)this.Heat_Protection, typeof(EquipmentStats), stats, "m_heatProtection");
            }
            if (this.Cold_Protection != null)
            {
                At.SetValue((float)this.Cold_Protection, typeof(EquipmentStats), stats, "m_coldProtection");
            }
            if (this.Corruption_Protection != null)
            {
                At.SetValue((float)this.Corruption_Protection, typeof(EquipmentStats), stats, "m_corruptionProtection");
            }

            if (this is SL_WeaponStats weaponStatsHolder)
            {
                if (!(stats is WeaponStats))
                {
                    var newstats = stats.gameObject.AddComponent<WeaponStats>();
                    At.InheritBaseValues(newstats as EquipmentStats, stats);
                    GameObject.DestroyImmediate(stats);
                    stats = newstats;
                }

                weaponStatsHolder.ApplyToItem(stats as WeaponStats);
            }
        }

        public static SL_EquipmentStats ParseEquipmentStats(EquipmentStats stats, SL_ItemStats itemStatsHolder)
        {
            var equipmentStatsHolder = new SL_EquipmentStats();
            
            if (stats == null || itemStatsHolder == null)
            {
                if (stats is ItemStats)
                {
                    var newstats = new EquipmentStats();
                    At.InheritBaseValues(newstats as ItemStats, stats);
                    stats = newstats;
                }
                else
                {
                    Debug.LogWarning("Equipment trying to be parsed with no stats");
                    return itemStatsHolder as SL_EquipmentStats;
                }
            }

            try
            {
                equipmentStatsHolder.Impact_Resistance = stats.ImpactResistance;
                equipmentStatsHolder.Damage_Protection = stats.GetDamageProtection(DamageType.Types.Physical);
                equipmentStatsHolder.Stamina_Use_Penalty = stats.StaminaUsePenalty;
                equipmentStatsHolder.Mana_Use_Modifier = (float)At.GetValue(typeof(EquipmentStats), stats, "m_manaUseModifier");
                equipmentStatsHolder.Movement_Penalty = stats.MovementPenalty;
                equipmentStatsHolder.Pouch_Bonus = stats.PouchCapacityBonus;
                equipmentStatsHolder.Heat_Protection = stats.HeatProtection;
                equipmentStatsHolder.Cold_Protection = stats.ColdProtection;
                equipmentStatsHolder.Corruption_Protection = stats.CorruptionResistance;

                equipmentStatsHolder.Damage_Bonus = At.GetValue(typeof(EquipmentStats), stats, "m_damageAttack") as float[];
                equipmentStatsHolder.Damage_Resistance = At.GetValue(typeof(EquipmentStats), stats, "m_damageResistance") as float[];

                At.InheritBaseValues(equipmentStatsHolder, itemStatsHolder);
            }
            catch (Exception e)
            {
                Debug.Log("Exception getting stats of " + stats.name + "\r\n" + e.Message + "\r\n" + e.StackTrace);
            }

            return equipmentStatsHolder;
        }
    }
}
