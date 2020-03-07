using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AddStatusEffectBuildupHolder : EffectHolder
    {
        public string StatusEffect;
        public float Buildup;

        public static AddStatusEffectBuildupHolder ParseAddStatusEffectBuildup(AddStatusEffectBuildUp addStatusEffectBuildUp, EffectHolder effectHolder)
        {
            var addStatusEffectBuildupHolder = new AddStatusEffectBuildupHolder();

            At.InheritBaseValues(addStatusEffectBuildupHolder, effectHolder);

            if (addStatusEffectBuildUp.Status != null)
            {
                addStatusEffectBuildupHolder.StatusEffect = addStatusEffectBuildUp.Status.IdentifierName;
                addStatusEffectBuildupHolder.Buildup = addStatusEffectBuildUp.BuildUpValue;
            }

            return addStatusEffectBuildupHolder;
        }
    }
}
