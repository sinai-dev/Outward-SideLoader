using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    class SL_AddChargeInstrument : SL_Effect
    {
        public int Charges;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AddChargeInstrument).Charges = this.Charges;
        }

        public override void SerializeEffect<T>(T effect)
        {
            Charges = (effect as AddChargeInstrument).Charges;
        }
    }
}
