namespace SideLoader
{
    public class SL_Throwable : SL_Item
    {
        public float? DestroyDelay;

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            if (this.DestroyDelay != null)
                (item as Throwable).DestroyDelay = (float)this.DestroyDelay;
        }

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            this.DestroyDelay = (item as Throwable).DestroyDelay;
        }
    }
}
