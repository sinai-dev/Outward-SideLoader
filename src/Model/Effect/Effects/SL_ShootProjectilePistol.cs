namespace SideLoader
{
    public class SL_ShootProjectilePistol : SL_ShootProjectile
    {
        public bool UseShot;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as ShootProjectilePistol).UseShot = this.UseShot;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            UseShot = (effect as ShootProjectilePistol).UseShot;
        }
    }
}
