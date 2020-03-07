using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectBurntHealthHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool IsModifier;

        public static AffectBurntHealthHolder ParseAffectBurntHealth(AffectBurntHealth affectBurntHealth, EffectHolder _effectHolder)
        {
            var affectBurntHealthHolder = new AffectBurntHealthHolder
            {
                AffectQuantity = affectBurntHealth.AffectQuantity,
                IsModifier = affectBurntHealth.IsModifier
            };

            At.InheritBaseValues(affectBurntHealthHolder, _effectHolder);

            return affectBurntHealthHolder;
        }
    }
}
