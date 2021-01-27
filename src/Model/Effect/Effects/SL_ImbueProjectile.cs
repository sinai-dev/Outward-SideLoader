namespace SideLoader
{
    public class SL_ImbueProjectile : SL_ImbueObject
    {
        public bool UnloadProjectile;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as ImbueProjectile;

            comp.UnloadProjectile = this.UnloadProjectile;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as ImbueProjectile;

            UnloadProjectile = comp.UnloadProjectile;
        }
    }
}
