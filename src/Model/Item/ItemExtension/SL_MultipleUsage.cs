namespace SideLoader
{
    public class SL_MultipleUsage : SL_ItemExtension
    {
        public bool? AppliedOnPrice;
        public bool? AppliedOnWeight;
        public bool? AutoStack;
        public int? MaxStackAmount;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as MultipleUsage;

            if (this.AppliedOnPrice != null)
            {
                comp.AppliedOnPrice = (bool)this.AppliedOnPrice;
            }
            if (this.AppliedOnWeight != null)
            {
                comp.AppliedOnWeight = (bool)this.AppliedOnWeight;
            }
            if (this.AutoStack != null)
            {
                comp.AutoStack = (bool)this.AutoStack;
            }
            if (this.MaxStackAmount != null)
            {
                comp.m_maxStackAmount = (int)this.MaxStackAmount;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as MultipleUsage;

            this.AppliedOnPrice = comp.AppliedOnPrice;
            this.AppliedOnWeight = comp.AppliedOnWeight;
            this.AutoStack = comp.AutoStack;
            this.MaxStackAmount = comp.m_maxStackAmount;
        }
    }
}
