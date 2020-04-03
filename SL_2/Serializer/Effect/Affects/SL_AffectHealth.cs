using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_AffectHealth : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;
        public float AffectQuantityAI;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectHealth>();

            component.AffectQuantity = this.AffectQuantity;
            component.AffectQuantityOnAI = this.AffectQuantityAI;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectHealth ParseAffectHealth(AffectHealth affectHealth, SL_Effect _effectHolder)
        {
            var affectHealthHolder = new SL_AffectHealth
            {
                AffectQuantity = affectHealth.AffectQuantity,
                IsModifier = affectHealth.IsModifier,
                AffectQuantityAI = affectHealth.AffectQuantityOnAI
            };

            At.InheritBaseValues(affectHealthHolder, _effectHolder);

            return affectHealthHolder;
        }
    }
}
