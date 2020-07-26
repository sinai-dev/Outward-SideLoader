using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var holder = template as SL_CorruptionLevelCondition;
            var comp = component as CorruptionLevelCondition;

            holder.Value = comp.Value;
            holder.CompareType = comp.CompareType;
        }
    }
}
