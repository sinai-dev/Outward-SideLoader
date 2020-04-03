using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Equipment : SL_Item
    {
        public EquipmentSlot.EquipmentSlotIDs EquipSlot;
        public Equipment.TwoHandedType TwoHandType;
        public Equipment.IKMode IKType;

        public float VisualDetectabilityAdd;

        public void ApplyToItem(Equipment item)
        {
            item.EquipSlot = this.EquipSlot;
            item.TwoHand = this.TwoHandType;
            item.IKType = this.IKType;

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

            equipmentHolder.StatsHolder = SL_EquipmentStats.ParseEquipmentStats(equipment.Stats, itemHolder.StatsHolder);

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
