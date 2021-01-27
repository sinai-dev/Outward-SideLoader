namespace SideLoader
{
    public class SL_WeaponCharger : SL_ItemExtension
    {
        public float? ChargingStaminaCost;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as WeaponCharger;

            if (this.ChargingStaminaCost != null)
            {
                comp.ChargingStaminaCost = (float)this.ChargingStaminaCost;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            this.ChargingStaminaCost = (extension as WeaponCharger).ChargingStaminaCost;
        }
    }
}
