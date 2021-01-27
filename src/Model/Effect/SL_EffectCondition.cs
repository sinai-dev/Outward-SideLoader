using System;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public abstract class SL_EffectCondition
    {
        public bool Invert;

        public EffectCondition ApplyToTransform(Transform transform)
        {
            Type componentType = Serializer.GetGameType(this.GetType());

            if (componentType != null)
            {
                var comp = transform.gameObject.AddComponent(componentType) as EffectCondition;
                comp.Invert = this.Invert;

                ApplyToComponent(comp);

                return comp;
            }
            else
            {
                SL.Log("Could not get Game type for SL_type: " + this.ToString());
                return null;
            }
        }

        public abstract void ApplyToComponent<T>(T component) where T : EffectCondition;

        public static SL_EffectCondition ParseCondition(EffectCondition component)
        {
            Type slType = Serializer.GetBestSLType(component.GetType());

            if (slType != null && !slType.IsAbstract)
            {
                var holder = Activator.CreateInstance(slType) as SL_EffectCondition;

                holder.Invert = component.Invert;

                holder.SerializeEffect(component);
                return holder;
            }
            else
            {
                SL.Log(component.ToString() + " is not supported yet, sorry!");
                return null;
            }
        }

        public abstract void SerializeEffect<T>(T component) where T : EffectCondition;
    }
}
