using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectStamina : SL_Effect
    {
        public float AffectQuantity;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectStamina>();

            component.AffectQuantity = this.AffectQuantity;
        }


        public static SL_AffectStamina ParseAffectStamina(AffectStamina affectStamina, SL_Effect _effectHolder)
        {
            var affectStaminaHolder = new SL_AffectStamina
            {
                AffectQuantity = affectStamina.AffectQuantity
            };

            At.InheritBaseValues(affectStaminaHolder, _effectHolder);

            return affectStaminaHolder;
        }
    }
}
