using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AddStatusEffect : SL_Effect
    {
        public string StatusEffect = "";
        public int ChanceToContract;

        public new void ApplyToTransform(Transform t)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusEffect);

            if (!status)
            {
                SL.Log("Error getting status effect on AddStatusEffectHolder. Could not find " + this.StatusEffect);
                return;
            }

            var component = t.gameObject.AddComponent<AddStatusEffect>();

            component.BaseChancesToContract = this.ChanceToContract;
            component.Status = status;

            if (this is SL_AddBoonEffect addBoonHolder)
            {
                addBoonHolder.ApplyToTransform(t);
            }
        }

        public static SL_AddStatusEffect ParseAddStatusEffect(AddStatusEffect addStatusEffect, SL_Effect effectHolder)
        {
            var addStatusEffectHolder = new SL_AddStatusEffect();

            At.InheritBaseValues(addStatusEffectHolder, effectHolder);

            if (addStatusEffect.Status != null)
            {
                addStatusEffectHolder.StatusEffect = addStatusEffect.Status.IdentifierName;
                addStatusEffectHolder.ChanceToContract = addStatusEffect.BaseChancesToContract;
            }

            if (addStatusEffect is AddBoonEffect addBoon)
            {
                return SL_AddBoonEffect.ParseAddBoonEffect(addBoon, addStatusEffectHolder);
            }

            return addStatusEffectHolder;
        }
    }
}
