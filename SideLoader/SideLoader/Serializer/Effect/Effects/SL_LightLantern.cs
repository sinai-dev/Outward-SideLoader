using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_LightLantern : SL_Effect
    {
        public bool Light;

        public override void ApplyToComponent<T>(T component)
        {
            (component as LightLantern).Light = this.Light;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_LightLantern).Light = (effect as LightLantern).Light;
        }
    }
}
