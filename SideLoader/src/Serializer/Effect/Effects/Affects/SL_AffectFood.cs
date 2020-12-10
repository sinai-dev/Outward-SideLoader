using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.Helpers;

namespace SideLoader
{
    public class SL_AffectFood : SL_Effect
    {
        public float AffectQuantity;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectFood).SetAffectFoodQuantity(this.AffectQuantity);
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectFood).AffectQuantity = (float)At.GetField("m_affectQuantity", effect as AffectNeed);
        }
    }
}
