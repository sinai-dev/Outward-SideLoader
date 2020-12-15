using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_GiveOrder : SL_Effect
    {
        public int OrderID;

        public override void ApplyToComponent<T>(T component)
        {
            (component as GiveOrder).OrderID = this.OrderID;
        }

        public override void SerializeEffect<T>(T effect)
        {
            OrderID = (effect as GiveOrder).OrderID;
        }
    }
}
