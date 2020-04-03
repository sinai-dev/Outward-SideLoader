using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Bag : SL_Equipment
    {
        public float Capacity;
        public bool Restrict_Dodge;
        public float InventoryProtection;

        public float Preserver_Amount = -1;
        public bool Nullify_Perish = false;

        public void ApplyToItem(Bag item)
        {
            // set container capacity
            var container = item.transform.Find("Content").GetComponent<ItemContainerStatic>();
            At.SetValue(this.Capacity, typeof(ItemContainer), container as ItemContainer, "m_baseContainerCapacity");

            // set restrict dodge 
            At.SetValue(this.Restrict_Dodge, typeof(Bag), item, "m_restrictDodge");

            // set invent prot
            At.SetValue(this.InventoryProtection, typeof(Bag), item, "m_inventoryProtection");

            if (this.Preserver_Amount > 0 || this.Nullify_Perish == true)
            {
                var preserver = item.transform.GetOrAddComponent<Preserver>();

                if (!this.Nullify_Perish)
                {
                    var elements = new List<Preserver.PreservedElement>()
                    {
                        new Preserver.PreservedElement()
                        {
                            Preservation = this.Preserver_Amount,
                            Tag = new TagSourceSelector(CustomItems.GetTag("Food"))
                        }
                    };

                    At.SetValue(elements, typeof(Preserver), preserver, "m_preservedElements");
                }
                else
                {
                    preserver.NullifyPerishing = this.Nullify_Perish;
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
                foreach (var element in list)
                {
                    SL.Log("Preserver element: " + element.Tag.Tag.TagName + ", float: " + element.Preservation);
                }

                bagHolder.Preserver_Amount = list[0].Preservation;
                bagHolder.Nullify_Perish = p.NullifyPerishing;
            }

            At.InheritBaseValues(bagHolder, equipmentHolder);

            return bagHolder;
        }
    }
}
