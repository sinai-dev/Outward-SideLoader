using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class EquipmentHolder : ItemHolder
    {
        public string EquipmentSlot;
        public string IKType;
        public string TwoHandType;
        public float VisualDetectabilityAdd;

        public string ArmorClass;

        public static EquipmentHolder ParseEquipment(Equipment equipment, ItemHolder itemHolder)
        {
            var equipmentHolder = new EquipmentHolder
            {
                EquipmentSlot = equipment.EquipSlot.ToString(),
                VisualDetectabilityAdd =  equipment.VisualDetectabilityAdd,
                TwoHandType = equipment.TwoHand.ToString(),
                IKType = equipment.IKType.ToString()
            };

            At.InheritBaseValues(equipmentHolder, itemHolder);

            equipmentHolder.StatsHolder = EquipmentStatsHolder.ParseEquipmentStats(equipment.Stats, itemHolder.StatsHolder);

            if (equipment is Armor)
            {
                equipmentHolder.ArmorClass = (equipment as Armor).Class.ToString();
            }

            if (equipment is Bag)
            {
                return BagHolder.ParseBag(equipment as Bag, equipmentHolder);
            }
            else if (equipment is Weapon)
            {
                return WeaponHolder.ParseWeapon(equipment as Weapon, equipmentHolder);
            }
            else
            {
                if (equipment.GetType() != typeof(Equipment) && equipment.GetType() != typeof(Armor))
                {
                    Debug.Log("Equipment type not supported: " + equipment.GetType().ToString());
                }

                return equipmentHolder;
            }
        }
    }
}
