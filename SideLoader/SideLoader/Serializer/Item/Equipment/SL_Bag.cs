using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
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
                At.SetValue((float)this.Capacity, typeof(ItemContainer), container, "m_baseContainerCapacity");
            }

            // set restrict dodge 
            if (this.Restrict_Dodge != null)
            {
                At.SetValue((bool)this.Restrict_Dodge, typeof(Bag), item, "m_restrictDodge");
            }

            if (this.InventoryProtection != null)
            {
                // set invent prot
                At.SetValue(this.InventoryProtection, typeof(Bag), item, "m_inventoryProtection");
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
