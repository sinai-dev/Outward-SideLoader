using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class ReduceDurabilityHolder : EffectHolder
    {
        public string EquipmentSlot;
        public float DurabilityLoss;

        public static ReduceDurabilityHolder ParseReduceDurability(ReduceDurability reduceDurability, EffectHolder effectHolder)
        {
            var reduceDurabilityHolder = new ReduceDurabilityHolder
            {
                EquipmentSlot = reduceDurability.EquipmentSlot.ToString(),
                DurabilityLoss = reduceDurability.Durability
            };

            At.InheritBaseValues(reduceDurabilityHolder, effectHolder);

            return reduceDurabilityHolder;
        }

    }
}
