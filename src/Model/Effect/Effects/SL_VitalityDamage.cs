using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_VitalityDamage : SL_Effect
    {
        public float PercentOfMax;

        public override void ApplyToComponent<T>(T component)
        {
            (component as VitalityDamage).PercentOfMax = this.PercentOfMax;
        }

        public override void SerializeEffect<T>(T effect)
        {
            PercentOfMax = (effect as VitalityDamage).PercentOfMax;
        }
    }
}
