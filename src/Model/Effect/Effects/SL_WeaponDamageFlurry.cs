namespace SideLoader
{
    public class SL_WeaponDamageFlurry : SL_WeaponDamage
    {
        public float HitDelay;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as WeaponDamageFlurry).HitDelay = this.HitDelay;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            HitDelay = (effect as WeaponDamageFlurry).HitDelay;
        }
    }
}
