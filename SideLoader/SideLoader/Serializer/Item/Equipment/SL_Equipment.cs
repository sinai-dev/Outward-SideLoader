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

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var equipment = item as Equipment;

            if (this.EquipSlot != null)
                equipment.EquipSlot = (EquipmentSlot.EquipmentSlotIDs)this.EquipSlot;

            if (this.TwoHandType != null)
                equipment.TwoHand = (Equipment.TwoHandedType)this.TwoHandType;

            if (this.IKType != null)
                equipment.IKType = (Equipment.IKMode)this.IKType;

            if (this.VisualDetectabilityAdd != null)
            {
                equipment.VisualDetectabilityAdd = (float)this.VisualDetectabilityAdd;
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_Equipment;
            var equipment = item as Equipment;

            template.EquipSlot = equipment.EquipSlot;
            template.VisualDetectabilityAdd = equipment.VisualDetectabilityAdd;
            template.TwoHandType = equipment.TwoHand;
            template.IKType = equipment.IKType;
        }
    }
}
