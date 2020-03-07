using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectManaHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool IsModifier;

        public static AffectManaHolder ParseAffectMana(AffectMana affectMana, EffectHolder _effectHolder)
        {
            var affectManaHolder = new AffectManaHolder
            {
                AffectQuantity = affectMana.Value,
                IsModifier = affectMana.IsModifier
            };

            At.InheritBaseValues(affectManaHolder, _effectHolder);

            return affectManaHolder;
        }
    }
}
