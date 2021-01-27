namespace SideLoader
{
    public class SL_CorruptionLevelCondition : SL_EffectCondition
    {
        public float Value;
        public AICondition.NumericCompare CompareType = AICondition.NumericCompare.Equal;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as CorruptionLevelCondition;

            comp.Value = this.Value;
            comp.CompareType = this.CompareType;
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as CorruptionLevelCondition;

            Value = comp.Value;
            CompareType = comp.CompareType;
        }
    }
}
