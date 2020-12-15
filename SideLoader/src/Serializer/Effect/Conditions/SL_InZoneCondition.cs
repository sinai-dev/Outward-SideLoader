using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_InZoneCondition : SL_EffectCondition
    {
        public float Radius;

        public override void ApplyToComponent<T>(T component)
        {
            (component as InZoneCondition).Radius = this.Radius;
        }

        public override void SerializeEffect<T>(T component)
        {
            Radius = (component as InZoneCondition).Radius;
        }
    }
}
