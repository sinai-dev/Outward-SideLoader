using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_IsEquipSlotFilledCondition : SL_EffectCondition
    {
        public EquipmentSlot.EquipmentSlotIDs EquipmentSlot;

        public override void ApplyToComponent<T>(T component)
        {
            (component as IsEquipSlotFilledCondition).EquipmentSlot = this.EquipmentSlot;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_IsEquipSlotFilledCondition).EquipmentSlot = (component as IsEquipSlotFilledCondition).EquipmentSlot;   
        }
    }
}
