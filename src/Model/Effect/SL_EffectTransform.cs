using SideLoader.Helpers;
using System.Collections.Generic;
using System.Linq;
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

        public SL_Effect[] Effects;
        public SL_EffectCondition[] EffectConditions;
        public SL_EffectTransform[] ChildEffects;

        public override string ToString()
        {
            string ret = this.TransformName;

            if (this.Effects?.Length > 0)
                ret += $" ({Effects.Length} effects)";

            if (this.EffectConditions?.Length > 0)
                ret += $" ({EffectConditions.Length} conditions)";

            if (this.ChildEffects?.Length > 0)
                ret += $" ({ChildEffects.Length} children)";

            return ret;
        }

        /// <summary>
        /// Returns true if this Transform contains any Effects or Conditions, or has Children which do.
        /// </summary>
        [XmlIgnore]
        public bool HasContent
        {
            get
            {
                if (Effects?.Length > 0 || EffectConditions?.Length > 0)
                    return true;

                if (ChildEffects != null)
                {
                    if (ChildEffects.Where(it => it.HasContent).Any())
                        return true;
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
        public static void ApplyTransformList(Transform parent, SL_EffectTransform[] transformsToApply, EditBehaviours behaviour)
        {
            if (behaviour == EditBehaviours.Destroy)
                UnityHelpers.DestroyChildren(parent);

            if (transformsToApply == null)
                return;

            foreach (var child in transformsToApply)
            {
                if (behaviour == EditBehaviours.Override && parent.Find(child.TransformName) is Transform existing)
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);

                child.ApplyToTransform(parent, behaviour);
            }
        }

        /// <summary>
        /// Pass the desired parent Transform, this method will create 'this.TransformName' on it, then apply the Effects and Conditions.
        /// </summary>
        /// <param name="parent">The PARENT transform to apply to (the Item, StatusEffect.Signature, Blast/Projectile, etc)</param>
        /// <param name="behaviour">Desired EffectBehaviour</param>
        public Transform ApplyToTransform(Transform parent, EditBehaviours behaviour)
        {
            Transform transform;
            if (parent.Find(this.TransformName) is Transform found)
                transform = found;
            else
            {
                transform = new GameObject(this.TransformName).transform;
                transform.parent = parent;
            }

            if (this.Position != null)
                transform.localPosition = (Vector3)this.Position;
            if (this.Rotation != null)
                transform.localRotation = Quaternion.Euler((Vector3)this.Rotation);
            if (this.Scale != null)
                transform.localScale = (Vector3)this.Scale;

            // apply effects
            if (this.Effects != null)
            {
                foreach (var effect in this.Effects)
                    effect.ApplyToTransform(transform);
            }

            // apply conditions
            if (this.EffectConditions != null)
            {
                foreach (var condition in this.EffectConditions)
                    condition.ApplyToTransform(transform);
            }

            if (ChildEffects != null)
                ApplyTransformList(transform, ChildEffects, behaviour);

            return transform;
        }

        public static SL_EffectTransform ParseTransform(Transform transform)
        {
            var holder = new SL_EffectTransform
            {
                TransformName = transform.name
            };

            if (transform.localPosition != Vector3.zero)
                holder.Position = transform.localPosition;
            if (transform.localEulerAngles != Vector3.zero)
                holder.Rotation = transform.localEulerAngles;
            if (transform.localScale != Vector3.one)
                holder.Scale = transform.localScale;

            var slEffects = new List<SL_Effect>();
            foreach (Effect effect in transform.GetComponents<Effect>())
            {
                if (!effect.enabled)
                    continue;

                if (SL_Effect.ParseEffect(effect) is SL_Effect slEffect)
                    slEffects.Add(slEffect);
            }
            holder.Effects = slEffects.ToArray();

            var slConditions = new List<SL_EffectCondition>();
            foreach (EffectCondition condition in transform.GetComponents<EffectCondition>())
            {
                if (!condition.enabled)
                    continue;

                if (SL_EffectCondition.ParseCondition(condition) is SL_EffectCondition slCondition)
                    slConditions.Add(slCondition);
            }
            holder.EffectConditions = slConditions.ToArray();

            var children = new List<SL_EffectTransform>();
            foreach (Transform child in transform)
            {
                if (child.name == "ExplosionFX" || child.name == "ProjectileFX")
                    continue;

                var transformHolder = ParseTransform(child);

                if (transformHolder.HasContent)
                    children.Add(transformHolder);
            }
            holder.ChildEffects = children.ToArray();

            return holder;
        }
    }
}
