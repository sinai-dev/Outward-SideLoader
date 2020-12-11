using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SideLoader.Helpers;
using UnityEngine;

namespace SideLoader
{
    public class SL_Bag : SL_Equipment
    {
        public float? Capacity;
        public bool? Restrict_Dodge;
        public float? InventoryProtection;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            if (this.Capacity != null)
            {
                // set container capacity
                var container = item.transform.Find("Content").GetComponent<ItemContainerStatic>();
                At.SetField(container, "m_baseContainerCapacity", (float)this.Capacity);
            }

            // set restrict dodge 
            if (this.Restrict_Dodge != null)
            {
                At.SetField(item as Bag, "m_restrictDodge", (bool)this.Restrict_Dodge);
            }

            if (this.InventoryProtection != null)
            {
                // set invent prot
                At.SetField(item as Bag, "m_inventoryProtection", this.InventoryProtection);
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var bag = item as Bag;
            var template = holder as SL_Bag;

            template.Capacity = bag.BagCapacity;
            template.Restrict_Dodge = bag.RestrictDodge;
            template.InventoryProtection = bag.InventoryProtection;
        }
    }
}
