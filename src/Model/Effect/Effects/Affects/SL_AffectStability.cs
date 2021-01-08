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
        public bool SetStability;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AffectStability;
            comp.AffectQuantity = this.AffectQuantity;
            comp.SetStability = this.SetStability;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = (effect as AffectStability);
            AffectQuantity = comp.AffectQuantity;
            SetStability = comp.SetStability;
        }
    }
}
