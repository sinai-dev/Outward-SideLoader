using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ShooterPosStatusEffect : SL_EffectCondition
    {
        public string StatusIdentifier;

        public override void ApplyToComponent<T>(T component)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(StatusIdentifier);

            if (!status)
            {
                SL.Log("", 0);
                return;
            }

            (component as ShooterPosStatusEffect).StatusEffect = status;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            (template as SL_ShooterPosStatusEffect).StatusIdentifier = (component as ShooterPosStatusEffect).StatusEffect.IdentifierName;
        }
    }
}
