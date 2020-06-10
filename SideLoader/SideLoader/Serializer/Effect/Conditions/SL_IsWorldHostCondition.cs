using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    // This EffectCondition has no fields, the name itself implies everything you need to know.

    public class SL_IsWorldHostCondition : SL_EffectCondition
    {
        public override void ApplyToComponent<T>(T component) { }

        public override void SerializeEffect<T>(EffectCondition component, T template) { }
    }
}
