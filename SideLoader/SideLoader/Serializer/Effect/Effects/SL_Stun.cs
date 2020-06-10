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

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_Stun).Duration = (effect as Stun).Duration;
        }
    }
}
