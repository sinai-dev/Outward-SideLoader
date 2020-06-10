using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectMana : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectMana).Value = this.AffectQuantity;
            (component as AffectMana).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectMana).AffectQuantity = (effect as AffectMana).Value;
            (holder as SL_AffectMana).IsModifier = (effect as AffectMana).IsModifier;
        }
    }
}
