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
                At.SetField(this.Damage_Resistance, "m_damageResistance", eStats);
           
            if (this.Impact_Resistance != null)
                At.SetField((float)this.Impact_Resistance, "m_impactResistance", eStats);
            
            if (this.Damage_Protection != null)
                At.SetField(new float[9] { (float)this.Damage_Protection, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, "m_damageProtection", eStats);
            
            if (this.Damage_Bonus != null)
                At.SetField(this.Damage_Bonus, "m_damageAttack", eStats);
            
            if (this.Stamina_Use_Penalty != null)
                At.SetField((float)this.Stamina_Use_Penalty, "m_staminaUsePenalty", eStats);
            
            if (this.Mana_Use_Modifier != null)
                At.SetField((float)this.Mana_Use_Modifier, "m_manaUseModifier", eStats);
            
            if (this.Movement_Penalty != null)
                At.SetField((float)this.Movement_Penalty, "m_movementPenalty", eStats);
            
            if (this.Pouch_Bonus != null)
                At.SetField((float)this.Pouch_Bonus, "m_pouchCapacityBonus", eStats);
            
            if (this.Heat_Protection != null)
                At.SetField((float)this.Heat_Protection, "m_heatProtection", eStats);
            
            if (this.Cold_Protection != null)
                At.SetField((float)this.Cold_Protection, "m_coldProtection", eStats);
            
            if (this.Corruption_Protection != null)
                At.SetField((float)this.Corruption_Protection, "m_corruptionProtection", eStats);
            
            if (this.Cooldown_Reduction != null)
                At.SetField((float)this.Cooldown_Reduction, "m_baseCooldownReductionBonus", eStats);
            
            if (this.Health_Regen != null)
                At.SetField((float)this.Health_Regen, "m_baseHealthRegenBonus", eStats);
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
                equipmentStatsHolder.Mana_Use_Modifier = (float)At.GetField("m_manaUseModifier", stats);
                equipmentStatsHolder.Movement_Penalty = eStats.MovementPenalty;
                equipmentStatsHolder.Pouch_Bonus = eStats.PouchCapacityBonus;
                equipmentStatsHolder.Heat_Protection = eStats.HeatProtection;
                equipmentStatsHolder.Cold_Protection = eStats.ColdProtection;
                equipmentStatsHolder.Corruption_Protection = eStats.CorruptionResistance;

                equipmentStatsHolder.Damage_Bonus = At.GetField("m_damageAttack", stats as EquipmentStats) as float[];
                equipmentStatsHolder.Damage_Resistance = At.GetField("m_damageResistance", stats as EquipmentStats) as float[];
            }
            catch (Exception e)
            {
                Debug.Log("Exception getting stats of " + stats.name + "\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
        }
    }
}
