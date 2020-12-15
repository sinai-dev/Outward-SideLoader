using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_AutoKnock : SL_Effect
    {
        public bool KnockDown;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AutoKnock).Down = this.KnockDown;
        }

        public override void SerializeEffect<T>(T effect)
        {
            KnockDown = (effect as AutoKnock).Down;
        }
    }
}
