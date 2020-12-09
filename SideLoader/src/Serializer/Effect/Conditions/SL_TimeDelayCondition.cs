using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_TimeDelayCondition : SL_EffectCondition
    {
        public Vector2 DelayRange;
        public TimeDelayCondition.TimeType TimeFormat;
        public bool IgnoreFirstCheck = true;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as TimeDelayCondition;

            comp.DelayRange = this.DelayRange;
            comp.TimeFormat = this.TimeFormat;
            comp.IgnoreFirstCheck = this.IgnoreFirstCheck;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var holder = template as SL_TimeDelayCondition;
            var comp = component as TimeDelayCondition;

            holder.DelayRange = comp.DelayRange;
            holder.TimeFormat = comp.TimeFormat;
            holder.IgnoreFirstCheck = comp.IgnoreFirstCheck;
        }
    }
}
