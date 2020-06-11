using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_EffectTransform
    {
        public string TransformName = "";

        public List<SL_Effect> Effects = new List<SL_Effect>();
        public List<SL_EffectCondition> EffectConditions = new List<SL_EffectCondition>();
        public List<SL_EffectTransform> ChildEffects = new List<SL_EffectTransform>();

        /// <summary>
        /// Applies a list of SL_EffectTransforms to a transform parent, with the provided EffectBehaviour.
        /// </summary>
        /// <param name="parent">The parent to apply to, ie. the Item, StatusEffect.Signature, or Blast/Projectile, etc</param>
        /// <param name="transformsToApply">The list of SL_EffectTransforms to apply.</param>
        /// <param name="behaviour">The desired behaviour for these transoforms (remove original, overwrite, or none)</param>
        public static void ApplyTransformList(Transform parent, List<SL_EffectTransform> transformsToApply, EffectBehaviours behaviour)
        {
            if (behaviour == EffectBehaviours.DestroyEffects)
            {
                SL.DestroyChildren(parent);
            }

            foreach (var child in transformsToApply)
            {
                // The position and rotation of the effect can actually be important in some cases.
                // Eg for Backstab, its actually used to determine to angle offset.
                // So in some cases when overriding, we need to keep these values.
                bool copyTranslation = false;
                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                if (behaviour == EffectBehaviours.OverrideEffects && parent.Find(child.TransformName) is Transform existing)
                {
                    copyTranslation = true;
                    pos = existing.position;
                    rot = existing.rotation;

                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                child.ApplyToTransform(parent, behaviour);

                if (copyTranslation)
                {
                    var transform = parent.Find(child.TransformName);
                    transform.position = pos;
                    transform.rotation = rot;
                }
            }
        }

        /// <summary>
        /// Pass the desired parent Transform, this method will create or find the existing 'this.TransformName' on it, then apply the Effects and Conditions.
        /// </summary>
        /// <param name="parent">The PARENT transform to apply to (the Item, StatusEffect.Signature, Blast/Projectile, etc)</param>
        /// <param name="behaviour">Desired EffectBehaviour</param>
        public Transform ApplyToTransform(Transform parent, EffectBehaviours behaviour)
        {
            var child = new GameObject(this.TransformName).transform;
            child.parent = parent;

            // apply effects
            if (this.Effects != null)
            {
                foreach (var effect in this.Effects)
                {
                    effect.ApplyToTransform(child);
                }
            }

            // apply conditions
            if (this.EffectConditions != null)
            {
                foreach (var condition in this.EffectConditions)
                {
                    condition.ApplyToTransform(child);
                }
            }

            if (ChildEffects != null && ChildEffects.Count > 0)
            {
                var newParent = parent.Find(TransformName);
                ApplyTransformList(newParent, ChildEffects, behaviour);
            }

            return child;
        }

        public static SL_EffectTransform ParseTransform(Transform transform)
        {
            var effectTransformHolder = new SL_EffectTransform
            {
                TransformName = transform.name
            };

            foreach (Effect effect in transform.GetComponents<Effect>())
            {
                if (!effect.enabled)
                {
                    continue;
                }

                if (SL_Effect.ParseEffect(effect) is SL_Effect holder)
                {
                    effectTransformHolder.Effects.Add(holder);
                }
            }

            foreach (EffectCondition condition in transform.GetComponents<EffectCondition>())
            {
                var effectConditionHolder = SL_EffectCondition.ParseCondition(condition);
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
