using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class RemoveStatusEffectHolder : EffectHolder
    {
        public string StatusEffect;
        public string StatusEffect_ByTag;
        public string StatusEffect_Family;
        public string StatusEffect_ByName;

        public static RemoveStatusEffectHolder ParseRemoveStatusEffect(RemoveStatusEffect removeStatusEffect, EffectHolder _effectHolder)
        {
            var removeStatusEffectHolder = new RemoveStatusEffectHolder
            {
               StatusEffect = removeStatusEffect.StatusEffect ? removeStatusEffect.StatusEffect.IdentifierName : null,
               StatusEffect_ByName = removeStatusEffect.StatusName,
               StatusEffect_ByTag = removeStatusEffect.StatusType.Tag.TagName
            };

            At.InheritBaseValues(removeStatusEffectHolder, _effectHolder);

            if (removeStatusEffect.StatusFamily != null
                && StatusEffectFamilyLibrary.Instance.GetStatusEffect(removeStatusEffect.StatusFamily.SelectorValue) is StatusEffectFamily statusFamily)
            {
                removeStatusEffectHolder.StatusEffect_Family = statusFamily.Name;
            }

            return removeStatusEffectHolder;
        }
    }
}
