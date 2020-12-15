using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ReduceDurability : SL_Effect
    {
        public float Durability;
        public EquipmentSlot.EquipmentSlotIDs EquipmentSlot;

        public override void ApplyToComponent<T>(T component)
        {
            (component as ReduceDurability).Durability = this.Durability;
            (component as ReduceDurability).EquipmentSlot = this.EquipmentSlot;
        }

        public override void SerializeEffect<T>(T effect)
        {
            EquipmentSlot = (effect as ReduceDurability).EquipmentSlot;
            Durability = (effect as ReduceDurability).Durability;
        }
    }
}
