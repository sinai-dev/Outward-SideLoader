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

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectHealthParentOwner).AffectQuantity = (effect as AffectHealthParentOwner).AffectQuantity;
            (holder as SL_AffectHealthParentOwner).Requires_AffectedChar = (effect as AffectHealthParentOwner).OnlyIfHasAffectedChar;
            (holder as SL_AffectHealthParentOwner).IsModifier = (effect as AffectHealthParentOwner).IsModifier;
        }
    }
}
