using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_BooleanCondition : SL_EffectCondition
    {
        public bool Valid;

        public override void ApplyToComponent<T>(T component)
        {
            (component as BooleanCondition).Valid = this.Valid;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_BooleanCondition).Valid = (component as BooleanCondition).Valid;   
        }
    }
}
