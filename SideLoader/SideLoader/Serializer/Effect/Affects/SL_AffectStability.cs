using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectStability : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectStability).AffectQuantity = this.AffectQuantity;
            (component as AffectStability).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectStability).AffectQuantity = (effect as AffectStability).AffectQuantity;
            (holder as SL_AffectStability).IsModifier = (effect as AffectStability).IsModifier;
        }
    }
}
