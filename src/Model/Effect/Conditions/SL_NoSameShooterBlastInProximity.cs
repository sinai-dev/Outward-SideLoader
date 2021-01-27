namespace SideLoader
{
    public class SL_NoSameShooterBlastInProximity : SL_EffectCondition
    {
        public float Proximity;

        public override void ApplyToComponent<T>(T component)
        {
            (component as NoSameShooterBlastInProximity).Proximity = this.Proximity;
        }

        public override void SerializeEffect<T>(T component)
        {
            Proximity = (component as NoSameShooterBlastInProximity).Proximity;
        }
    }
}
