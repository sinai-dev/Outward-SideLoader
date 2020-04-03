using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    public class SL_EffectTransform
    {
        public string TransformName;
        public List<SL_Effect> Effects = new List<SL_Effect>();

        //public List<EffectConditionHolder> EffectConditions = new List<EffectConditionHolder>();

        public List<SL_EffectTransform> ChildEffects = new List<SL_EffectTransform>();

        public void ApplyToItem(Item item, bool destroyExisting)
        {
            if (destroyExisting)
            {
                SL.Log("Destroying existing effects for " + item.Name);
                CustomItems.DestroyChildren(item.transform);
            }

            // get transform of this.TransformName
            var transform = item.transform.Find(this.TransformName);
            if (!transform)
            {
                transform = new GameObject(this.TransformName).transform;
                transform.parent = item.transform;
            }

            // apply effects
            foreach (var effect in this.Effects)
            {
                effect.ApplyToTransform(transform);
            }
        }

        public static SL_EffectTransform ParseTransform(Transform transform)
        {
            var effectTransformHolder = new SL_EffectTransform
            {
                TransformName = transform.name
            };

            foreach (Effect effect in transform.GetComponents<Effect>())
            {
                var effectHolder = SL_Effect.ParseEffect(effect);
                if (effectHolder != null)
                    effectTransformHolder.Effects.Add(effectHolder);
            }

            //foreach (EffectCondition condition in transform.GetComponents<EffectCondition>())
            //{
            //    var effectConditionHolder = EffectConditionHolder.ParseEffectCondition(condition);
            //    effectTransformHolder.EffectConditions.Add(effectConditionHolder);
            //}

            foreach (Transform child in transform)
            {
                if (child.name == "ExplosionFX" || child.name == "ProjectileFX")
                {
                    // visual effects, we dont care about these
                    continue;
                }

                var transformHolder = ParseTransform(child);
                if (transformHolder.ChildEffects.Count > 0 || transformHolder.Effects.Count > 0) // || transformHolder.EffectConditions.Count > 0)
                {
                    effectTransformHolder.ChildEffects.Add(transformHolder);
                }
            }

            return effectTransformHolder;
        }
    }
}
