namespace SideLoader
{
    public class SL_ProximitySoulSpotCondition : SL_EffectCondition
    {
        public float Distance;

        public override void ApplyToComponent<T>(T component)
        {
            (component as ProximitySoulSpotCondition).ProximityDist = this.Distance;
        }

        public override void SerializeEffect<T>(T component)
        {
            Distance = (component as ProximitySoulSpotCondition).ProximityDist;
        }
    }
}
