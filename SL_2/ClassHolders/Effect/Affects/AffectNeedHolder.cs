using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectNeedHolder : EffectHolder
    {
        public string Need;
        public float AffectQuantity;

        public static AffectNeedHolder ParseAffectNeed(AffectNeed affectNeed, EffectHolder _effectHolder)
        {
            var affectDrinkHolder = new AffectNeedHolder
            {
                AffectQuantity = (float)At.GetValue(typeof(AffectNeed), affectNeed, "m_affectQuantity"),
                Need = affectNeed.GetType().ToString()
            };

            At.InheritBaseValues(affectDrinkHolder, _effectHolder);

            return affectDrinkHolder;
        }
    }
}