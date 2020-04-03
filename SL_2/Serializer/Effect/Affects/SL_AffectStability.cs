using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_AffectStability : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectBurntHealth>();

            component.AffectQuantity = this.AffectQuantity;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectStability ParseAffectStability(AffectStability affectStability, SL_Effect _effectHolder)
        {
            var affectStabilityHolder = new SL_AffectStability
            {
                AffectQuantity = affectStability.AffectQuantity,
                IsModifier = affectStability.IsModifier
            };

            At.InheritBaseValues(affectStabilityHolder, _effectHolder);

            return affectStabilityHolder;
        }
    }
}
