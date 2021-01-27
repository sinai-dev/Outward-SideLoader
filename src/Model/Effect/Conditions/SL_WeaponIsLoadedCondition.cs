namespace SideLoader
{
    public class SL_WeaponIsLoadedCondition : SL_EffectCondition
    {
        public Weapon.WeaponSlot SlotToCheck;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as WeaponIsLoadedCondition;

            comp.SlotToCheck = this.SlotToCheck;
        }

        public override void SerializeEffect<T>(T component)
        {
            SlotToCheck = (component as WeaponIsLoadedCondition).SlotToCheck;
        }
    }
}
