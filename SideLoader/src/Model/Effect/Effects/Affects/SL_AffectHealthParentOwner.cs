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

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectHealthParentOwner).AffectQuantity = this.AffectQuantity;
            (component as AffectHealthParentOwner).OnlyIfHasAffectedChar = this.Requires_AffectedChar;
            (component as AffectHealthParentOwner).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect)
        {
            AffectQuantity = (effect as AffectHealthParentOwner).AffectQuantity;
            Requires_AffectedChar = (effect as AffectHealthParentOwner).OnlyIfHasAffectedChar;
            IsModifier = (effect as AffectHealthParentOwner).IsModifier;
        }
    }
}
