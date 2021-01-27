namespace SideLoader
{
    public class SL_HeightCondition : SL_EffectCondition
    {
        public bool AllowEqual;
        public HeightCondition.CompareTypes CompareType;
        public float HeightThreshold;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as HeightCondition;

            comp.AllowEqual = this.AllowEqual;
            comp.CompareType = this.CompareType;
            comp.HeightThreshold = this.HeightThreshold;
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as HeightCondition;

            CompareType = comp.CompareType;
            AllowEqual = comp.AllowEqual;
            HeightThreshold = comp.HeightThreshold;
        }
    }
}
