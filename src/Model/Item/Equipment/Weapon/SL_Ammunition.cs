namespace SideLoader
{
    public class SL_Ammunition : SL_Weapon
    {
        // public SL_Transform ProjectileFXPrefab;
        public int? PoolCapacity;
        public ProjectileItem.CollisionBehaviorTypes? BehaviorOnCollision;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var comp = (item as Ammunition);

            if (this.PoolCapacity != null)
                comp.PoolCapacity = (int)this.PoolCapacity;

            if (this.BehaviorOnCollision != null)
                comp.BehaviorOnCollision = (ProjectileItem.CollisionBehaviorTypes)this.BehaviorOnCollision;
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            PoolCapacity = (item as Ammunition).PoolCapacity;
            BehaviorOnCollision = (item as Ammunition).BehaviorOnCollision;
        }
    }
}
