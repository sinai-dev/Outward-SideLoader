namespace SideLoader
{
    public class SL_Equipment : SL_Item
    {
        public EquipmentSlot.EquipmentSlotIDs? EquipSlot;
        public Equipment.TwoHandedType? TwoHandType;
        public Equipment.IKMode? IKType;

        public float? VisualDetectabilityAdd;

        public PlayerSystem.PlayerTypes? RequiredPlayerType;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var equipment = item as Equipment;

            if (this.RequiredPlayerType != null)
                equipment.RequiredPType = (PlayerSystem.PlayerTypes)this.RequiredPlayerType;

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

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var equipment = item as Equipment;

            EquipSlot = equipment.EquipSlot;
            VisualDetectabilityAdd = equipment.VisualDetectabilityAdd;
            TwoHandType = equipment.TwoHand;
            IKType = equipment.IKType;
            RequiredPlayerType = equipment.RequiredPType;
        }
    }
}
