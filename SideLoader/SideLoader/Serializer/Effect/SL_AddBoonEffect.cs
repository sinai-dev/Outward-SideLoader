using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AddBoonEffect : SL_AddStatusEffect
    {
        public string AmplifiedEffect = "";

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.AmplifiedEffect);

            if (!status)
            {
                SL.Log("Error getting AmplifiedEffect status effect on AddBoonHolder. Could not find " + this.AmplifiedEffect);
                return;
            }

            (component as AddBoonEffect).BoonAmplification = status;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            (holder as SL_AddBoonEffect).AmplifiedEffect = (effect as AddBoonEffect).BoonAmplification.IdentifierName;
        }
    }
}
