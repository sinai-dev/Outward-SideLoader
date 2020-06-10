using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_MostRecentCondition : SL_EffectCondition
    {
        public string StatusIdentifierToCheck;
        public string StatusIdentifierToCompareTo;

        public override void ApplyToComponent<T>(T component)
        {
            var status1 = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(StatusIdentifierToCheck);
            var status2 = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(StatusIdentifierToCompareTo);

            if (!status1 || !status2)
            {
                SL.Log("SL_MostRecentCondition: Could not get required status effects!");
                return;
            }

            (component as MostRecentCondition).StatusEffectPrefab = status1;
            (component as MostRecentCondition).StatusEffectToCompare = status2;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var holder = template as SL_MostRecentCondition;
            var comp = component as MostRecentCondition;

            holder.StatusIdentifierToCheck = comp.StatusEffectPrefab?.IdentifierName;
            holder.StatusIdentifierToCompareTo = comp.StatusEffectToCompare?.IdentifierName;
        }
    }
}
