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

        public new void ApplyToTransform(Transform t)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.AmplifiedEffect);

            if (!status)
            {
                SL.Log("Error getting AmplifiedEffect status effect on AddBoonHolder. Could not find " + this.AmplifiedEffect);
                return;
            }

            var component = t.gameObject.AddComponent<AddBoonEffect>();

            var old = t.gameObject.GetComponent<AddStatusEffect>();
            var normalStatus = old.Status;
            At.InheritBaseValues(component as AddStatusEffect, old);
            GameObject.Destroy(old);

            component.BaseChancesToContract = this.ChanceToContract;
            component.Status = normalStatus;
            component.BoonAmplification = status;

        }

        public static SL_AddBoonEffect ParseAddBoonEffect(AddBoonEffect addBoonEffect, SL_AddStatusEffect addStatusHolder)
        {
            var addBoonHolder = new SL_AddBoonEffect();

            At.InheritBaseValues(addBoonHolder, addStatusHolder);

            if (addBoonEffect.BoonAmplification != null)
            {
                addBoonHolder.AmplifiedEffect = addBoonEffect.BoonAmplification.IdentifierName;
            }

            return addBoonHolder;
        }
    }
}
