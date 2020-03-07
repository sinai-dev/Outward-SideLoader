using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectStabilityHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool IsModifier;

        public static AffectStabilityHolder ParseAffectStability(AffectStability affectStability, EffectHolder _effectHolder)
        {
            var affectStabilityHolder = new AffectStabilityHolder
            {
                AffectQuantity = affectStability.AffectQuantity,
                IsModifier = affectStability.IsModifier
            };

            At.InheritBaseValues(affectStabilityHolder, _effectHolder);

            return affectStabilityHolder;
        }
    }
}
