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
                SL.Log("");
                return;
            }

            (component as ShooterPosStatusEffect).StatusEffect = status;
        }

        public override void SerializeEffect<T>(T component)
        {
            StatusIdentifier = (component as ShooterPosStatusEffect).StatusEffect.IdentifierName;
        }
    }
}
