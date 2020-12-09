using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Cough : SL_Effect
    {
        public int ChanceToTrigger;

        public override void ApplyToComponent<T>(T component)
        {
            (component as Cough).ChancesToTrigger = this.ChanceToTrigger;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_Cough).ChanceToTrigger = (effect as Cough).ChancesToTrigger;
        }
    }
}
