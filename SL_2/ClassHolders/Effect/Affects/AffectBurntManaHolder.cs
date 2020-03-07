using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectBurntManaHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool IsModifier;

        public static AffectBurntManaHolder ParseAffectBurntMana(AffectBurntMana affectBurntMana, EffectHolder _effectHolder)
        {
            var affectBurntManaHolder = new AffectBurntManaHolder
            {
                AffectQuantity = affectBurntMana.AffectQuantity,
                IsModifier = affectBurntMana.IsModifier
            };

            At.InheritBaseValues(affectBurntManaHolder, _effectHolder);

            return affectBurntManaHolder;
        }
    }
}
