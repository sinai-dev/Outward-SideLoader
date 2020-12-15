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

        public override void SerializeEffect<T>(T effect)
        {
            AffectQuantity = (effect as AffectHealth).AffectQuantity;
            AffectQuantityAI = (effect as AffectHealth).AffectQuantityOnAI;
            IsModifier = (effect as AffectHealth).IsModifier;
        }
    }
}
