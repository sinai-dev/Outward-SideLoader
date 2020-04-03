using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_AffectBurntStamina : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectBurntStamina>();

            component.AffectQuantity = this.AffectQuantity;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectBurntStamina ParseAffectBurntStamina(AffectBurntStamina affectBurntStamina, SL_Effect _effectHolder)
        {
            var affectBurntStaminaHolder = new SL_AffectBurntStamina
            {
                AffectQuantity = affectBurntStamina.AffectQuantity,
                IsModifier = affectBurntStamina.IsModifier
            };

            At.InheritBaseValues(affectBurntStaminaHolder, _effectHolder);

            return affectBurntStaminaHolder;
        }
    }
}