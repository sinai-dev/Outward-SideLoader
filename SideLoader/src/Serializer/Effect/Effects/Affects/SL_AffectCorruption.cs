using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_AffectCorruption : SL_Effect
    {
        public float AffectQuantity;
        public bool IsRaw;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectCorruption).AffectQuantity = this.AffectQuantity;
            (component as AffectCorruption).IsRaw = this.IsRaw;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectCorruption).AffectQuantity = (effect as AffectCorruption).AffectQuantity;
            (holder as SL_AffectCorruption).IsRaw = (effect as AffectCorruption).IsRaw;
        }
    }
}
