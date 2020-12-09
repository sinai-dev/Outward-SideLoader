using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_DelayEffectCondition : SL_EffectCondition
    {
        public float Delay;
        public DelayEffectCondition.DelayTypes DelayType;

        public override void ApplyToComponent<T>(T component)
        {
            (component as DelayEffectCondition).Delay = this.Delay;
            (component as DelayEffectCondition).DelayType = this.DelayType;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_DelayEffectCondition).Delay = (component as DelayEffectCondition).Delay;
            (template as SL_DelayEffectCondition).DelayType = (component as DelayEffectCondition).DelayType;
        }
    }
}
