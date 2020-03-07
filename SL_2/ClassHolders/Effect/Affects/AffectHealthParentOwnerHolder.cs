using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectHealthParentOwnerHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool Requires_AffectedChar;
        public bool IsModifier;

        public static AffectHealthParentOwnerHolder ParseAffectHealthParentOwner(AffectHealthParentOwner affectHealthParent, EffectHolder _effectHolder)
        {
            var affectHealthHolder = new AffectHealthParentOwnerHolder
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
