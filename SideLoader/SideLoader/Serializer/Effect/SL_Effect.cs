using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader
{
    [SL_Serialized]
    public abstract class SL_Effect
    {
        /// <summary>The time, in seconds, after which the effects will be applied. Default is 0.</summary>
        public float Delay = 0f;
        /// <summary>Sync type determines the networking behaviour.</summary>
        public Effect.SyncTypes SyncType = Effect.SyncTypes.Everyone;
        /// <summary>Override the SL_EffectTransform.TransformName category with a manual value.</summary>
        public EffectSynchronizer.EffectCategories OverrideCategory = EffectSynchronizer.EffectCategories.None;

        public abstract void ApplyToComponent<T>(T component) where T : Effect;

        /// <summary>Adds and applies this effect to the provided Transform.</summary>
        public Effect ApplyToTransform(Transform t)
        {
            var type = this.GetType();

            if (Serializer.GetGameType(type) is Type game_type)
            {
                var comp = t.gameObject.AddComponent(game_type) as Effect;

                // set base fields
                comp.Delay = this.Delay;
                comp.SyncType = this.SyncType;
                comp.OverrideEffectCategory = this.OverrideCategory;

                ApplyToComponent(comp);

                return comp;
            }
            else
            {
                SL.Log("Could not get Game type for SL_type: " + type, 1);
                return null;
            }
        }

        public abstract void SerializeEffect<T>(T effect, SL_Effect holder) where T : Effect;

        /// <summary>Serialize an effect and get the equivalent SL_Effect.</summary>
        public static SL_Effect ParseEffect(Effect effect)
        {
            var type = effect.GetType();

            if (Serializer.GetBestSLType(type) is Type sl_type && !sl_type.IsAbstract)
            {
                var holder = Activator.CreateInstance(sl_type) as SL_Effect;
                holder.Delay = effect.Delay;
                holder.OverrideCategory = effect.OverrideEffectCategory;
                holder.SyncType = effect.SyncType;

                holder.SerializeEffect(effect, holder);
                return holder;
            }
            else
            {
                SL.Log(type + " is not supported yet, sorry!");
                return null;
            }
        }
    }
}
