using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MapMagic;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public abstract class SL_EffectCondition
    {
        public bool Invert;
    
        public EffectCondition ApplyToTransform(Transform transform)
        {
            var type = this.GetType();

            if (Serializer.GetGameType(type) is Type game_type)
            {
                var comp = transform.gameObject.AddComponent(game_type) as EffectCondition;
                comp.Invert = this.Invert;

                ApplyToComponent(comp);

                return comp;
            }
            else
            {
                SL.Log("Could not get Game type for SL_type: " + type);
                return null;
            }
        }

        public abstract void ApplyToComponent<T>(T component) where T : EffectCondition;

        public static SL_EffectCondition ParseCondition(EffectCondition component)
        {
            var type = component.GetType();

            if (Serializer.GetSLType(type) is Type sl_type)
            {
                var holder = Activator.CreateInstance(sl_type) as SL_EffectCondition;

                holder.Invert = component.Invert;

                holder.SerializeEffect(component);
                return holder;
            }
            else
            {
                SL.Log(type + " is not supported yet, sorry!");
                return null;
            }
        }

        public abstract void SerializeEffect<T>(T component) where T : EffectCondition;
    }
}
