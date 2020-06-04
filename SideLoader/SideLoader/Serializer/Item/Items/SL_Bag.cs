using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Bag : SL_Equipment
    {
        public float? Capacity;
        public bool? Restrict_Dodge;
        public float? InventoryProtection;

        public float? Preserver_Amount = -1;
        public bool? Nullify_Perish = false;

        public void ApplyToItem(Bag item)
        {
            // set container capacity
            var container = item.transform.Find("Content").GetComponent<ItemContainerStatic>();
            if (this.Capacity != null)
            {
                At.SetValue((float)this.Capacity, typeof(ItemContainer), container as ItemContainer, "m_baseContainerCapacity");
            }

            // set restrict dodge 
            if (this.Restrict_Dodge != null)
            {
                At.SetValue((bool)this.Restrict_Dodge, typeof(Bag), item, "m_restrictDodge");
            }

            // set invent prot
            At.SetValue(this.InventoryProtection, typeof(Bag), item, "m_inventoryProtection");

            if (this.Preserver_Amount != null || this.Nullify_Perish == true)
            {
                var preserver = container.transform.GetOrAddComponent<Preserver>();

                var nullperish = this.Nullify_Perish == null || this.Nullify_Perish == false;

                if (!nullperish)
                {
                    var elements = new List<Preserver.PreservedElement>()
                    {
                        new Preserver.PreservedElement()
                        {
                            Preservation = (float)this.Preserver_Amount,
                            Tag = new TagSourceSelector(CustomItems.GetTag("Food"))
                        }
                    };

                    At.SetValue(elements, typeof(Preserver), preserver, "m_preservedElements");
                }
                else
                {
                    preserver.NullifyPerishing = true;
                }
            }
        }

        public static SL_Bag ParseBag(Bag bag, SL_Equipment equipmentHolder)
        {
            var bagHolder = new SL_Bag
            {
                Capacity = bag.BagCapacity,
                Restrict_Dodge = bag.RestrictDodge,
                InventoryProtection = bag.InventoryProtection
            };

            if (bag.GetComponentInChildren<Preserver>() is Preserver p
                && At.GetValue(typeof(Preserver), p, "m_preservedElements") is List<Preserver.PreservedElement> list && list.Count > 0)
            {
                bagHolder.Preserver_Amount = list[0].Preservation;
                bagHolder.Nullify_Perish = p.NullifyPerishing;
            }

            At.CopyFieldValues(bagHolder, equipmentHolder);

            return bagHolder;
        }
    }
}
