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

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectMana>();

            component.Value = this.AffectQuantity;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectMana ParseAffectMana(AffectMana affectMana, SL_Effect _effectHolder)
        {
            var affectManaHolder = new SL_AffectMana
            {
                AffectQuantity = affectMana.Value,
                IsModifier = affectMana.IsModifier
            };

            At.InheritBaseValues(affectManaHolder, _effectHolder);

            return affectManaHolder;
        }
    }
}
