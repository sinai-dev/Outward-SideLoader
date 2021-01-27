namespace SideLoader
{
    public class SL_PunctualDamageAoE : SL_PunctualDamage
    {
        public float Radius;
        public Shooter.TargetTypes TargetType;
        public bool IgnoreShooter;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as PunctualDamageAoE).Radius = this.Radius;
            (component as PunctualDamageAoE).TargetType = this.TargetType;
            (component as PunctualDamageAoE).IgnoreShooter = this.IgnoreShooter;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as PunctualDamageAoE;

            Radius = comp.Radius;
            TargetType = comp.TargetType;
            IgnoreShooter = comp.IgnoreShooter;
        }
    }
}
