namespace SideLoader
{
    public class SL_EquipDurabilityCondition : SL_EffectCondition
    {
        public EquipmentSlot.EquipmentSlotIDs EquipmentSlot;
        public float MinimumDurability;

        public override void ApplyToComponent<T>(T component)
        {
            (component as EquipDurabilityCondition).EquipmentSlot = this.EquipmentSlot;
            (component as EquipDurabilityCondition).DurabilityRequired = this.MinimumDurability;
        }

        public override void SerializeEffect<T>(T component)
        {
            MinimumDurability = (component as EquipDurabilityCondition).DurabilityRequired;
            EquipmentSlot = (component as EquipDurabilityCondition).EquipmentSlot;
        }
    }
}
