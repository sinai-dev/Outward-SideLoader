namespace SideLoader
{
    public class SL_ReduceDurability : SL_Effect
    {
        public float Durability;
        public EquipmentSlot.EquipmentSlotIDs EquipmentSlot;
        public bool Percentage;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as ReduceDurability;

            comp.Durability = this.Durability;
            comp.EquipmentSlot = this.EquipmentSlot;
            comp.Percentage = this.Percentage;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as ReduceDurability;

            EquipmentSlot = comp.EquipmentSlot;
            Durability = comp.Durability;
            Percentage = comp.Percentage;
        }
    }
}
