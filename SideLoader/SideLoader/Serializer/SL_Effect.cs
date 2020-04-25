using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader
{
    public abstract class SL_Effect
    {
        public float Delay = 0f;
        public Effect.SyncTypes SyncType = Effect.SyncTypes.Everyone;
        public EffectSynchronizer.EffectCategories OverrideCategory = EffectSynchronizer.EffectCategories.None;

        public Effect ApplyToTransform(Transform t)
        {
            var type = this.GetType();

            if (type == typeof(SL_Effect))
            {
                SL.Log("You cannot use the SL_Effect class directly! Use a subclass of it.", 1);
                return null;
            }

            if (GetGameEffect(type) is Type game_type)
            {
                var comp = t.gameObject.AddComponent(game_type) as Effect;
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

        public abstract void ApplyToComponent<T>(T component) where T: Effect;

        public static SL_Effect ParseEffect(Effect effect)
        {
            var type = effect.GetType();

            if (GetSLEffect(type) is Type sl_type)
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
                SL.Log("Could not find SL holder type for game type: " + type + ", it is probably not supported yet, sorry!", 1);
                return null;
            }
        }

        public abstract void SerializeEffect<T>(T effect, SL_Effect holder) where T : Effect;

        // ================ Type match dictionary ================

        /// <summary>
        /// Pass a SL_Effect to get the associated game Effect class
        /// </summary>
        /// <param name="sl_type">The SL_Effect type</param>
        /// <returns>The equivalent subclass of Effect</returns>
        public static Type GetGameEffect(Type sl_type)
        {
            if (!sl_type.IsSubclassOf(typeof(SL_Effect)))
            {
                SL.Log("Error: This type does not inherit from SL_Effect! Invalid.", 1);
                return null;
            }

            var entry = GameToSLEffect.First(x => x.Value == sl_type);
            return entry.Key;
        }

        /// <summary>
        /// Pass any "Effect" class to get the SL_Effect equivalent (if one exists)
        /// </summary>
        /// <param name="game_type">The Effect class to get the SL_Effect for</param>
        /// <returns>The SL_Effect equivalent, or null if its not supported</returns>
        public static Type GetSLEffect(Type game_type)
        {
            if (!game_type.IsSubclassOf(typeof(Effect)))
            {
                SL.Log("Error: This type does not inherit from Effect! Invalid.", 1);
                return null;
            }

            try
            {
                return GameToSLEffect.First(x => x.Key == game_type).Value;
            }
            catch
            {
                //SL.Log("Error: this type is not yet serializable, sorry!", 1);
                return null;
            }
        }

        /// <summary>
        /// Key: Game Effect (eg Effect)
        /// Value: SL Effect (eg SL_Effect)
        /// </summary>
        public static Dictionary<Type, Type> GameToSLEffect = new Dictionary<Type, Type>()
        {
            {
                typeof(PunctualDamage),
                typeof(SL_PunctualDamage)
            },
            {
                typeof(WeaponDamage),
                typeof(SL_WeaponDamage)
            },
            {
                typeof(AddStatusEffect),
                typeof(SL_AddStatusEffect)
            },
            {
                typeof(AddBoonEffect),
                typeof(SL_AddBoonEffect)
            },
            {
                typeof(AddStatusEffectBuildUp),
                typeof(SL_AddStatusEffectBuildUp)
            },
            {
                typeof(ImbueWeapon),
                typeof(SL_ImbueWeapon)
            },
            {
                typeof(RemoveStatusEffect),
                typeof(SL_RemoveStatusEffect)
            },
            {
                typeof(AffectStat),
                typeof(SL_AffectStat)
            },
            {
                typeof(AffectBurntHealth),
                typeof(SL_AffectBurntHealth)
            },
            {
                typeof(AffectBurntMana),
                typeof(SL_AffectBurntMana)
            },
            {
                typeof(AffectBurntStamina),
                typeof(SL_AffectBurntStamina)
            },
            {
                typeof(AffectHealth),
                typeof(SL_AffectHealth)
            },
            {
                typeof(AffectHealthParentOwner),
                typeof(SL_AffectHealthParentOwner)
            },
            {
                typeof(AffectMana),
                typeof(SL_AffectMana)
            },
            {
                typeof(AffectStability),
                typeof(SL_AffectStability)
            },
            {
                typeof(AffectStamina),
                typeof(SL_AffectStamina)
            }
        };
    }
}
