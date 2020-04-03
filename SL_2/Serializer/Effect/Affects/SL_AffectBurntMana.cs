using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_AffectBurntMana : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectBurntMana>();

            component.AffectQuantity = this.AffectQuantity;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectBurntMana ParseAffectBurntMana(AffectBurntMana affectBurntMana, SL_Effect _effectHolder)
        {
            var affectBurntManaHolder = new SL_AffectBurntMana
            {
                AffectQuantity = affectBurntMana.AffectQuantity,
                IsModifier = affectBurntMana.IsModifier
            };

            At.InheritBaseValues(affectBurntManaHolder, _effectHolder);

            return affectBurntManaHolder;
        }
    }
}
