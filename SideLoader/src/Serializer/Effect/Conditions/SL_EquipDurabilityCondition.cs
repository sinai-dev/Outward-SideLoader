using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_EquipDurabilityCondition : SL_EffectCondition
    {
        public EquipmentSlot.EquipmentSlotIDs EquipmentSlot;
        public float MinimumDurability;

        public override void ApplyToComponent<T>(T component)
        {
            (component as EquipDurabilityCondition).EquipmentSlot = this.EquipmentSlot;
            (component as EquipDurabilityCondition).DurabilityRequired = this.MinimumDurability;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_EquipDurabilityCondition).MinimumDurability = (component as EquipDurabilityCondition).DurabilityRequired;
            (template as SL_EquipDurabilityCondition).EquipmentSlot = (component as EquipDurabilityCondition).EquipmentSlot;
        }
    }
}
