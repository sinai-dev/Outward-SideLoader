using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ContainedWaterCondition : SL_EffectCondition
    {
        public WaterType ValidWaterType;

        public override void ApplyToComponent<T>(T component)
        {
            (component as ContainedWaterCondition).ValidWaterType = this.ValidWaterType;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_ContainedWaterCondition).ValidWaterType = (component as ContainedWaterCondition).ValidWaterType;
        }
    }
}
