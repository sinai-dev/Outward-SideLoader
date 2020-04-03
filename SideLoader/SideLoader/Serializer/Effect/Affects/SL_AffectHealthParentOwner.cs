using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectHealthParentOwner : SL_Effect
    {
        public float AffectQuantity;
        public bool Requires_AffectedChar;
        public bool IsModifier;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<AffectHealthParentOwner>();

            component.AffectQuantity = this.AffectQuantity;
            component.OnlyIfHasAffectedChar = this.Requires_AffectedChar;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectHealthParentOwner ParseAffectHealthParentOwner(AffectHealthParentOwner affectHealthParent, SL_Effect _effectHolder)
        {
            var affectHealthHolder = new SL_AffectHealthParentOwner
            {
                AffectQuantity = affectHealthParent.AffectQuantity,
                Requires_AffectedChar = affectHealthParent.OnlyIfHasAffectedChar,
                IsModifier = affectHealthParent.IsModifier
            };

            At.InheritBaseValues(affectHealthHolder, _effectHolder);

            return affectHealthHolder;
        }
    }
}
