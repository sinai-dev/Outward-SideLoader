namespace SideLoader
{
    public class SL_HasQuantityItemsCondition : SL_EffectCondition
    {
        public int TotalItemsRequired;

        public override void ApplyToComponent<T>(T component)
        {
            (component as HasQuantityItemsCondition).ItemQuantityRequired = TotalItemsRequired;
        }

        public override void SerializeEffect<T>(T component)
        {
            TotalItemsRequired = (component as HasQuantityItemsCondition).ItemQuantityRequired;
        }
    }
}
