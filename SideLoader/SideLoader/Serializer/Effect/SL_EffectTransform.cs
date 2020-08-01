using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_EffectTransform
    {
        public string TransformName = "";

        public Vector3? Position;
        public Vector3? Rotation;
        public Vector3? Scale;

        public List<SL_Effect> Effects = new List<SL_Effect>();
        public List<SL_EffectCondition> EffectConditions = new List<SL_EffectCondition>();
        public List<SL_EffectTransform> ChildEffects = new List<SL_EffectTransform>();

        /// <summary>
        /// Returns true if this Transform contains any Effects or Conditions, or has Children which do.
        /// </summary>
        [XmlIgnore]
        public bool HasContent
        {
            get
            {
                if ((Effects != null && Effects.Count > 0) 
                    || (EffectConditions != null && EffectConditions.Count > 0))
                {
                    return true;
                }

                if (ChildEffects != null)
                {
                    foreach (var child in ChildEffects)
                    {
                        if (child.HasContent)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

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

            if (transformsToApply == null)
            {
                return;
            }

            foreach (var child in transformsToApply)
            {
                if (behaviour == EffectBehaviours.OverrideEffects && parent.Find(child.TransformName) is Transform existing)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                child.ApplyToTransform(parent, behaviour);
            }
        }

        /// <summary>
        /// Pass the desired parent Transform, this method will create 'this.TransformName' on it, then apply the Effects and Conditions.
        /// </summary>
        /// <param name="parent">The PARENT transform to apply to (the Item, StatusEffect.Signature, Blast/Projectile, etc)</param>
        /// <param name="behaviour">Desired EffectBehaviour</param>
        public Transform ApplyToTransform(Transform parent, EffectBehaviours behaviour)
        {
            var transform = new GameObject(this.TransformName).transform;
            transform.parent = parent;

            if (this.Position != null)
            {
                transform.position = (Vector3)this.Position;
            }
            if (this.Rotation != null)
            {
                transform.rotation = Quaternion.Euler((Vector3)this.Rotation);
            }
            if (this.Scale != null)
            {
                transform.localScale = (Vector3)this.Scale;
            }

            // apply effects
            if (this.Effects != null)
            {
                foreach (var effect in this.Effects)
                {
                    effect.ApplyToTransform(transform);
                }
            }

            // apply conditions
            if (this.EffectConditions != null)
            {
                foreach (var condition in this.EffectConditions)
                {
                    condition.ApplyToTransform(transform);
                }
            }

            if (ChildEffects != null && ChildEffects.Count > 0)
            {
                ApplyTransformList(transform, ChildEffects, behaviour);
            }

            return transform;
        }

        public static SL_EffectTransform ParseTransform(Transform transform)
        {
            var holder = new SL_EffectTransform
            {
                TransformName = transform.name
            };

            if (transform.position != Vector3.zero)
            {
                holder.Position = transform.position;
            }
            if (transform.rotation.eulerAngles != Vector3.zero)
            {
                holder.Rotation = transform.rotation.eulerAngles;
            }
            if (transform.localScale != Vector3.one)
            {
                holder.Scale = transform.localScale;
            }

            foreach (Effect effect in transform.GetComponents<Effect>())
            {
                if (!effect.enabled)
                {
                    continue;
                }

                if (SL_Effect.ParseEffect(effect) is SL_Effect slEffect)
                {
                    holder.Effects.Add(slEffect);
                }
            }

            foreach (EffectCondition condition in transform.GetComponents<EffectCondition>())
            {
                if (!condition.enabled)
                {
                    continue;
                }

                if (SL_EffectCondition.ParseCondition(condition) is SL_EffectCondition slCondition)
                {
                    holder.EffectConditions.Add(slCondition);
                }
            }

            foreach (Transform child in transform)
            {
                if (child.name == "ExplosionFX" || child.name == "ProjectileFX")
                {
                    // visual effects, we cant serialize these yet.
                    continue;
                }

                var transformHolder = ParseTransform(child);

                if (transformHolder.HasContent)
                {
                    holder.ChildEffects.Add(transformHolder);
                }
            }

            return holder;
        }
    }
}
