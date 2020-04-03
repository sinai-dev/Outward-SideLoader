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

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectBurntHealth>();

            component.AffectQuantity = this.AffectQuantity;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectBurntHealth ParseAffectBurntHealth(AffectBurntHealth affectBurntHealth, SL_Effect _effectHolder)
        {
            var affectBurntHealthHolder = new SL_AffectBurntHealth
            {
                AffectQuantity = affectBurntHealth.AffectQuantity,
                IsModifier = affectBurntHealth.IsModifier
            };

            At.InheritBaseValues(affectBurntHealthHolder, _effectHolder);

            return affectBurntHealthHolder;
        }
    }
}
