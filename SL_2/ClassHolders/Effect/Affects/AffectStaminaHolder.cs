using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectStaminaHolder : EffectHolder
    {
        public float AffectQuantity;

        public static AffectStaminaHolder ParseAffectStamina(AffectStamina affectStamina, EffectHolder _effectHolder)
        {
            var affectStaminaHolder = new AffectStaminaHolder
            {
                AffectQuantity = affectStamina.AffectQuantity
            };

            At.InheritBaseValues(affectStaminaHolder, _effectHolder);

            return affectStaminaHolder;
        }
    }
}
