using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectBurntStaminaHolder : EffectHolder
    {
        public float AffectQuantity;
        public bool IsModifier;

        public static AffectBurntStaminaHolder ParseAffectBurntStamina(AffectBurntStamina affectBurntStamina, EffectHolder _effectHolder)
        {
            var affectBurntStaminaHolder = new AffectBurntStaminaHolder
            {
                AffectQuantity = affectBurntStamina.AffectQuantity,
                IsModifier = affectBurntStamina.IsModifier
            };

            At.InheritBaseValues(affectBurntStaminaHolder, _effectHolder);

            return affectBurntStaminaHolder;
        }
    }
}