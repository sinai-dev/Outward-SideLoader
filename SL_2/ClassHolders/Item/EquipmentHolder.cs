using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class EquipmentHolder : ItemHolder
    {
        public EquipmentSlot.EquipmentSlotIDs Slot;
        public Equipment.TwoHandedType TwoHandType;
        public Equipment.IKMode IKType;

        public float VisualDetectabilityAdd;

       // public string ArmorClass;

        public static EquipmentHolder ParseEquipment(Equipment equipment, ItemHolder itemHolder)
        {
            var equipmentHolder = new EquipmentHolder
            {
                Slot = equipment.EquipSlot,
                VisualDetectabilityAdd =  equipment.VisualDetectabilityAdd,
                TwoHandType = equipment.TwoHand,
                IKType = equipment.IKType
            };

            At.InheritBaseValues(equipmentHolder, itemHolder);

            equipmentHolder.StatsHolder = EquipmentStatsHolder.ParseEquipmentStats(equipment.Stats, itemHolder.StatsHolder);

            if (equipment is Weapon)
            {
                return WeaponHolder.ParseWeapon(equipment as Weapon, equipmentHolder);
            }

            return equipmentHolder;

            //if (equipment is Armor)
            //{
            //    equipmentHolder.ArmorClass = (equipment as Armor).Class.ToString();
            //}

            //if (equipment is Bag)
            //{
            //    return BagHolder.ParseBag(equipment as Bag, equipmentHolder);
            //}
            //else if (equipment is Weapon)
            //{
            //    return WeaponHolder.ParseWeapon(equipment as Weapon, equipmentHolder);
            //}
            //else
            //{
            //    if (equipment.GetType() != typeof(Equipment) && equipment.GetType() != typeof(Armor))
            //    {
            //        Debug.Log("Equipment type not supported: " + equipment.GetType().ToString());
            //    }

            //    return equipmentHolder;
            //}
        }
    }
}
