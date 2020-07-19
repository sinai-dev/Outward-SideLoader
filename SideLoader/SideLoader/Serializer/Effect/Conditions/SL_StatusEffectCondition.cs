using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_StatusEffectCondition : SL_EffectCondition
    {
        public string StatusIdentifier;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as StatusEffectCondition;

            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusIdentifier);

            if (!status)
            {
                SL.Log("SL_StatusEffectCondition: Could not get a status effect with the Identifier '" + StatusIdentifier + "'!", 0);
                return;
            }

            comp.Invert = false;
            comp.Inverse = this.Invert;

            comp.StatusEffectPrefab = status;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var holder = template as SL_StatusEffectCondition;
            var comp = component as StatusEffectCondition;

            holder.StatusIdentifier = comp.StatusEffectPrefab.IdentifierName;
            holder.Invert = comp.Inverse;
        }
    }
}
