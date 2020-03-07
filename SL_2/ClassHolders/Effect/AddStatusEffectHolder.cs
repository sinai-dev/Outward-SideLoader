using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AddStatusEffectHolder : EffectHolder
    {
        public string StatusEffect;
        public float ChanceToContract;

        public static AddStatusEffectHolder ParseAddStatusEffect(AddStatusEffect addStatusEffect, EffectHolder effectHolder)
        {
            var addStatusEffectHolder = new AddStatusEffectHolder();

            At.InheritBaseValues(addStatusEffectHolder, effectHolder);

            if (addStatusEffect.Status != null)
            {
                addStatusEffectHolder.StatusEffect = addStatusEffect.Status.IdentifierName;
                addStatusEffectHolder.ChanceToContract = addStatusEffect.BaseChancesToContract;
            }

            return addStatusEffectHolder;
        }
    }
}
