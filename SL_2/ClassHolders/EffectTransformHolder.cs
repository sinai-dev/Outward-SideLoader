using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class EffectTransformHolder
    {
        public string TransformName;
        public List<EffectHolder> Effects = new List<EffectHolder>();
        public List<EffectConditionHolder> EffectConditions = new List<EffectConditionHolder>();

        public List<EffectTransformHolder> ChildEffects = new List<EffectTransformHolder>();

        public static EffectTransformHolder ParseTransform(Transform transform)
        {
            var effectTransformHolder = new EffectTransformHolder
            {
                TransformName = transform.name
            };

            foreach (Effect effect in transform.GetComponents<Effect>())
            {
                var effectHolder = EffectHolder.ParseEffect(effect);
                if (effectHolder != null)
                    effectTransformHolder.Effects.Add(effectHolder);
            }

            foreach (EffectCondition condition in transform.GetComponents<EffectCondition>())
            {
                var effectConditionHolder = EffectConditionHolder.ParseEffectCondition(condition);
                effectTransformHolder.EffectConditions.Add(effectConditionHolder);
            }

            foreach (Transform child in transform)
            {
                if (child.name == "ExplosionFX" || child.name == "ProjectileFX")
                {
                    // visual effects, we dont care about these
                    continue;
                }

                var transformHolder = ParseTransform(child);
                if (transformHolder.ChildEffects.Count > 0 || transformHolder.Effects.Count > 0 || transformHolder.EffectConditions.Count > 0)
                {
                    effectTransformHolder.ChildEffects.Add(transformHolder);
                }
            }

            return effectTransformHolder;
        }
    }
}
