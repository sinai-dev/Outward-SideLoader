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

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as TimeDelayCondition;

            DelayRange = comp.DelayRange;
            TimeFormat = comp.TimeFormat;
            IgnoreFirstCheck = comp.IgnoreFirstCheck;
        }
    }
}
