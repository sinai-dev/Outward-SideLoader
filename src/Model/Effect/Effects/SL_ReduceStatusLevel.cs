using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ReduceStatusLevel : SL_Effect
    {
        public string StatusIdentifierToReduce;
        public int ReduceAmount;

        public override void ApplyToComponent<T>(T component)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusIdentifierToReduce);

            if (!status)
            {
                SL.Log("SL_ReduceStatusLevel: Could not find a status with the identifier '" + StatusIdentifierToReduce + "'!");
                return;
            }

            (component as ReduceStatusLevel).LevelAmount = this.ReduceAmount;
            (component as ReduceStatusLevel).StatusEffectToReduce = status;
        }

        public override void SerializeEffect<T>(T effect)
        {
            ReduceAmount = (effect as ReduceStatusLevel).LevelAmount;
            StatusIdentifierToReduce = (effect as ReduceStatusLevel).StatusEffectToReduce.IdentifierName;
        }
    }
}
