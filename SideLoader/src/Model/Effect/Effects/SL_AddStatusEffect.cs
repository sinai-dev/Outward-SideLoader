using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AddStatusEffect : SL_Effect
    {
        /// <summary>Must use a Status Identifier, not the actual name of the status effect.</summary>
        public string StatusEffect = "";
        /// <summary>Usually this is 100 for 100%, but it can be between 0 and 100.</summary>
        public int ChanceToContract = 100;
        /// <summary>If true, overrides the affected character to be the creator of this effect. Used for HitEffects.</summary>
        public bool AffectController = false;
        /// <summary>For LevelStatusEffects (ie Alert), adds additional stacks to the level.</summary>
        public int AdditionalLevel = 0;
        /// <summary>If true, forces this effect to not know who applied it.</summary>
        public bool NoDealer = false;

        public override void ApplyToComponent<T>(T component)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusEffect);

            if (!status)
            {
                SL.LogWarning($"{this.GetType().Name}: Could not find any effect with the identifier '{this.StatusEffect}'");
                return;
            }

            (component as AddStatusEffect).BaseChancesToContract = this.ChanceToContract;
            (component as AddStatusEffect).Status = status;
        }

        public override void SerializeEffect<T>(T effect)
        {
            if ((effect as AddStatusEffect).Status)
            {
                StatusEffect = (effect as AddStatusEffect).Status.IdentifierName;
                ChanceToContract = (effect as AddStatusEffect).BaseChancesToContract;
            }
        }
    }
}
