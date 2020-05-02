using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Equipment : SL_Item
    {
        public EquipmentSlot.EquipmentSlotIDs? EquipSlot;
        public Equipment.TwoHandedType? TwoHandType;
        public Equipment.IKMode? IKType;

        public float? VisualDetectabilityAdd;

        public void ApplyToItem(Equipment item)
        {
            if (this.EquipSlot != null)
                item.EquipSlot = (EquipmentSlot.EquipmentSlotIDs)this.EquipSlot;

            if (this.TwoHandType != null)
                item.TwoHand = (Equipment.TwoHandedType)this.TwoHandType;

            if (this.IKType != null)
                item.IKType = (Equipment.IKMode)this.IKType;

            if (this.VisualDetectabilityAdd != null)
            {
                item.VisualDetectabilityAdd = (float)this.VisualDetectabilityAdd;
            }

            if (this is SL_Weapon weaponHolder)
            {
                weaponHolder.ApplyToItem(item as Weapon);
            }
            else if (this is SL_Bag bagHolder)
            {
                bagHolder.ApplyToItem(item as Bag);
            }
        }

        public static SL_Equipment ParseEquipment(Equipment equipment, SL_Item itemHolder)
        {
            var equipmentHolder = new SL_Equipment
            {
                EquipSlot = equipment.EquipSlot,
                VisualDetectabilityAdd =  equipment.VisualDetectabilityAdd,
                TwoHandType = equipment.TwoHand,
                IKType = equipment.IKType
            };

            At.InheritBaseValues(equipmentHolder, itemHolder);

            equipmentHolder.StatsHolder = SL_EquipmentStats.ParseEquipmentStats(equipment.GetComponent<EquipmentStats>(), itemHolder.StatsHolder);

            if (equipment is Bag)
            {
                return SL_Bag.ParseBag(equipment as Bag, equipmentHolder);
            }
            else if (equipment is Weapon)
            {
                return SL_Weapon.ParseWeapon(equipment as Weapon, equipmentHolder);
            }
            else
                return equipmentHolder;
        }
    }
}
