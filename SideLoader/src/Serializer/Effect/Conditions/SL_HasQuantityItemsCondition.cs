using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_HasQuantityItemsCondition : SL_EffectCondition
    {
        public int TotalItemsRequired;

        public override void ApplyToComponent<T>(T component)
        {
            (component as HasQuantityItemsCondition).ItemQuantityRequired = TotalItemsRequired;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_HasQuantityItemsCondition).TotalItemsRequired = (component as HasQuantityItemsCondition).ItemQuantityRequired;
        }
    }
}
