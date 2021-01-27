namespace SideLoader
{
    public class SL_ProbabilityCondition : SL_EffectCondition
    {
        public int ChancePercent;

        public override void ApplyToComponent<T>(T component)
        {
            (component as ProbabilityCondition).ProbabilityChances = ChancePercent;
        }

        public override void SerializeEffect<T>(T component)
        {
            ChancePercent = (component as ProbabilityCondition).ProbabilityChances;
        }
    }
}
