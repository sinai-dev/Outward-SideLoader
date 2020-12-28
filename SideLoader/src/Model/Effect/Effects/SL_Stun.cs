using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Stun : SL_Effect
    {
        public float Duration;

        public override void ApplyToComponent<T>(T component)
        {
            (component as Stun).Duration = this.Duration;
        }

        public override void SerializeEffect<T>(T effect)
        {
            Duration = (effect as Stun).Duration;
        }
    }
}
