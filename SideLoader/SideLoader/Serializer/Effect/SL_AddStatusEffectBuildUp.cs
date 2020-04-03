using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AddStatusEffectBuildUp : SL_Effect
    {
        public string StatusEffect;
        public float Buildup;

        public new void ApplyToTransform(Transform t)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusEffect);

            if (!status)
            {
                SL.Log("Error getting status effect on AddStatusEffectBuildupHolder. Could not find " + this.StatusEffect);
                return;
            }

            var component = t.gameObject.AddComponent<AddStatusEffectBuildUp>();

            component.Status = status;
            component.BuildUpValue = this.Buildup;
        }

        public static SL_AddStatusEffectBuildUp ParseAddStatusEffectBuildup(AddStatusEffectBuildUp addStatusEffectBuildUp, SL_Effect effectHolder)
        {
            var addStatusEffectBuildupHolder = new SL_AddStatusEffectBuildUp();

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
