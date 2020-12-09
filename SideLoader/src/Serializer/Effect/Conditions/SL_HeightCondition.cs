using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var comp = component as HeightCondition;
            var holder = template as SL_HeightCondition;

            holder.CompareType = comp.CompareType;
            holder.AllowEqual = comp.AllowEqual;
            holder.HeightThreshold = comp.HeightThreshold;
        }
    }
}
