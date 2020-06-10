using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectHealth : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;
        public float AffectQuantityAI;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectHealth).AffectQuantity = this.AffectQuantity;
            (component as AffectHealth).AffectQuantityOnAI = this.AffectQuantityAI;
            (component as AffectHealth).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectHealth).AffectQuantity = (effect as AffectHealth).AffectQuantity;
            (holder as SL_AffectHealth).AffectQuantityAI = (effect as AffectHealth).AffectQuantityOnAI;
            (holder as SL_AffectHealth).IsModifier = (effect as AffectHealth).IsModifier;
        }
    }
}
