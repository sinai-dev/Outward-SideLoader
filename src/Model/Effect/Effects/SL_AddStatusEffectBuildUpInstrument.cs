namespace SideLoader
{
    public class SL_AddStatusEffectBuildUpInstrument : SL_AddStatusEffectBuildUp
    {
        public float ChancesPerCharge;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as AddStatusEffectBuildUpInstrument).ChancesPerCharge = this.ChancesPerCharge;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            ChancesPerCharge = (effect as AddStatusEffectBuildUpInstrument).ChancesPerCharge;
        }
    }
}
