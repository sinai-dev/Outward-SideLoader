using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_EffectTransform
    {
        public string TransformName = "";
        public List<SL_Effect> Effects = new List<SL_Effect>();

        //public List<EffectConditionHolder> EffectConditions = new List<EffectConditionHolder>();

        public List<SL_EffectTransform> ChildEffects = new List<SL_EffectTransform>();

        /// <summary>
        /// Pass the desired parent Transform, this method will create or find the existing 'this.TransformName' on it.
        /// </summary>
        /// <param name="parent">The PARENT transform to apply to (the Item or StatusEffect)</param>
        public void ApplyToTransform(Transform parent)
        {
            var child = parent.Find(this.TransformName);
            if (!child)
            {
                child = new GameObject(this.TransformName).transform;
                child.parent = parent;
            }

            // apply effects
            foreach (var effect in this.Effects)
            {
                effect.ApplyToTransform(child);
            }
        }

        [Obsolete("Use EffectTransform.ApplyToTransform instead.")]
        public void ApplyToItem(Item item)
        {
            ApplyToTransform(item.transform);
        }

        public static SL_EffectTransform ParseTransform(Transform transform)
        {
            var effectTransformHolder = new SL_EffectTransform
            {
                TransformName = transform.name
            };

            foreach (Effect effect in transform.GetComponents<Effect>())
            {
                var holder = SL_Effect.ParseEffect(effect);
                if (holder != null)
                {
                    effectTransformHolder.Effects.Add(holder);
                }
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
