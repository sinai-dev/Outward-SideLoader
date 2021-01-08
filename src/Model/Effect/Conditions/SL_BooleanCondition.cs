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

        public override void SerializeEffect<T>(T component)
        {
            Valid = (component as BooleanCondition).Valid;   
        }
    }
}
