using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectMana : SL_Effect
    {
        public AffectMana.AffectTypes AffectType;
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AffectMana;

            comp.Value = this.AffectQuantity;
            comp.IsModifier = this.IsModifier;
            comp.AffectType = this.AffectType;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var comp = effect as AffectMana;
            var template = holder as SL_AffectMana;

            template.AffectQuantity = comp.Value;
            template.IsModifier =     comp.IsModifier;
            template.AffectType =     comp.AffectType;
        }
    }
}
