using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ProbabilityCondition : SL_EffectCondition
    {
        public int ChancePercent;

        public override void ApplyToComponent<T>(T component)
        {
            (component as ProbabilityCondition).ProbabilityChances = ChancePercent;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_ProbabilityCondition).ChancePercent = (component as ProbabilityCondition).ProbabilityChances;
        }
    }
}
