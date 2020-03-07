using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectHealthHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool IsModifier;
        public float AffectQuanityAI;

        public static AffectHealthHolder ParseAffectHealth(AffectHealth affectHealth, EffectHolder _effectHolder)
        {
            var affectHealthHolder = new AffectHealthHolder
            {
                AffectQuantity = affectHealth.AffectQuantity,
                IsModifier = affectHealth.IsModifier,
                AffectQuanityAI = affectHealth.AffectQuantityOnAI
            };

            At.InheritBaseValues(affectHealthHolder, _effectHolder);

            return affectHealthHolder;
        }
    }
}
