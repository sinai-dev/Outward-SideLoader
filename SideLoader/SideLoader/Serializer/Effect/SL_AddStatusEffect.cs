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

        public override void ApplyToComponent<T>(T component)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusEffect);

            if (!status)
            {
                SL.Log("Error getting status effect on AddStatusEffectHolder. Could not find " + this.StatusEffect);
                return;
            }

            (component as AddStatusEffect).BaseChancesToContract = this.ChanceToContract;
            (component as AddStatusEffect).Status = status;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            if ((effect as AddStatusEffect).Status != null)
            {
                (holder as SL_AddStatusEffect).StatusEffect = (effect as AddStatusEffect).Status.IdentifierName;
                (holder as SL_AddStatusEffect).ChanceToContract = (effect as AddStatusEffect).BaseChancesToContract;
            }
        }
    }
}
