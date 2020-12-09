using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectBurntMana : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectBurntMana).AffectQuantity = this.AffectQuantity;
            (component as AffectBurntMana).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectBurntMana).AffectQuantity = (effect as AffectBurntMana).AffectQuantity;
            (holder as SL_AffectBurntMana).IsModifier = (effect as AffectBurntMana).IsModifier;
        }
    }
}
