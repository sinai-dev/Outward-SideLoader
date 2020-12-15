using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectBurntHealth : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectBurntHealth).AffectQuantity = this.AffectQuantity;
            (component as AffectBurntHealth).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect)
        {
            AffectQuantity = (effect as AffectBurntHealth).AffectQuantity;
            IsModifier = (effect as AffectBurntHealth).IsModifier;
        }
    }
}
