using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.Helpers;
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
        public float? Health_Regen;
        public float? Cooldown_Reduction;

        public override void ApplyToItem(ItemStats stats)
        {
            base.ApplyToItem(stats);

            var eStats = stats as EquipmentStats;

            if (this.Damage_Resistance != null)
                At.SetField(eStats, "m_damageResistance", this.Damage_Resistance);
           
            if (this.Impact_Resistance != null)
                At.SetField(eStats, "m_impactResistance", (float)this.Impact_Resistance);
            
            if (this.Damage_Protection != null)
                At.SetField(eStats, "m_damageProtection", new float[9] { (float)this.Damage_Protection, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f });
            
            if (this.Damage_Bonus != null)
                At.SetField(eStats, "m_damageAttack", this.Damage_Bonus);
            
            if (this.Stamina_Use_Penalty != null)
                At.SetField(eStats, "m_staminaUsePenalty", (float)this.Stamina_Use_Penalty);
            
            if (this.Mana_Use_Modifier != null)
                At.SetField(eStats, "m_manaUseModifier", (float)this.Mana_Use_Modifier);
            
            if (this.Movement_Penalty != null)
                At.SetField(eStats, "m_movementPenalty", (float)this.Movement_Penalty);
            
            if (this.Pouch_Bonus != null)
                At.SetField(eStats, "m_pouchCapacityBonus", (float)this.Pouch_Bonus);
            
            if (this.Heat_Protection != null)
                At.SetField(eStats, "m_heatProtection", (float)this.Heat_Protection);
            
            if (this.Cold_Protection != null)
                At.SetField(eStats, "m_coldProtection", (float)this.Cold_Protection);
            
            if (this.Corruption_Protection != null)
                At.SetField(eStats, "m_corruptionProtection", (float)this.Corruption_Protection);
            
            if (this.Cooldown_Reduction != null)
                At.SetField(eStats, "m_baseCooldownReductionBonus", (float)this.Cooldown_Reduction);
            
            if (this.Health_Regen != null)
                At.SetField(eStats, "m_baseHealthRegenBonus", (float)this.Health_Regen);
        }

        public override void SerializeStats(ItemStats stats, SL_ItemStats holder)
        {
            base.SerializeStats(stats, holder);

            var equipmentStatsHolder = holder as SL_EquipmentStats;

            try
            {
                var eStats = stats as EquipmentStats;

                equipmentStatsHolder.Impact_Resistance = eStats.ImpactResistance;
                equipmentStatsHolder.Damage_Protection = eStats.GetDamageProtection(DamageType.Types.Physical);
                equipmentStatsHolder.Stamina_Use_Penalty = eStats.StaminaUsePenalty;
                equipmentStatsHolder.Mana_Use_Modifier = (float)At.GetField(stats as EquipmentStats, "m_manaUseModifier");
                equipmentStatsHolder.Movement_Penalty = eStats.MovementPenalty;
                equipmentStatsHolder.Pouch_Bonus = eStats.PouchCapacityBonus;
                equipmentStatsHolder.Heat_Protection = eStats.HeatProtection;
                equipmentStatsHolder.Cold_Protection = eStats.ColdProtection;
                equipmentStatsHolder.Corruption_Protection = eStats.CorruptionResistance;

                equipmentStatsHolder.Damage_Bonus = At.GetField(stats as EquipmentStats, "m_damageAttack") as float[];
                equipmentStatsHolder.Damage_Resistance = At.GetField(stats as EquipmentStats, "m_damageResistance") as float[];
            }
            catch (Exception e)
            {
                Debug.Log("Exception getting EquipmentStats of " + stats.name + "\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
        }
    }
}
