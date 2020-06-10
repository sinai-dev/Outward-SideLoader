using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectBurntStamina : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectBurntStamina).AffectQuantity = this.AffectQuantity;
            (component as AffectBurntStamina).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectBurntStamina).AffectQuantity = (effect as AffectBurntStamina).AffectQuantity;
            (holder as SL_AffectBurntStamina).IsModifier = (effect as AffectBurntStamina).IsModifier;
        }
    }
}