using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class BagHolder : EquipmentHolder
    {
        public float Capacity;
        public bool HasLanternSlot;
        public bool Restrict_Dodge;
        public float InventoryProtection;

        public float Preserver_Amount;
        public bool Nullify_Perish;

        public static BagHolder ParseBag(Bag bag, EquipmentHolder equipmentHolder)
        {
            var bagHolder = new BagHolder
            {
                Capacity = bag.BagCapacity,
                HasLanternSlot = bag.HasLanternSlot,
                Restrict_Dodge = bag.RestrictDodge,
                InventoryProtection = bag.InventoryProtection
            };

            if (bag.GetComponentInChildren<Preserver>() is Preserver p
                && At.GetValue(typeof(Preserver), p, "m_preservedElements") is List<Preserver.PreservedElement> list && list.Count > 0)
            {
                if (list.Count > 1)
                {
                    Debug.LogWarning("Bag has MORE THAN ONE preserver!?");
                }

                bagHolder.Preserver_Amount = list[0].Preservation;
                bagHolder.Nullify_Perish = p.NullifyPerishing;
            }

            At.InheritBaseValues(bagHolder, equipmentHolder);

            return bagHolder;
        }
    }
}
