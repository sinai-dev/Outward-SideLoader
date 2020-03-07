using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class EffectHolder
    {
        public static EffectHolder ParseEffect(Effect effect)
        {
            var effectHolder = new EffectHolder();

            // there's probably a more elegant solution than this...

            if (effect is PunctualDamage)
            {
                return PunctualDamageHolder.ParsePunctualDamage(effect as PunctualDamage, effectHolder);
            }
            else if (effect is AddStatusEffect)
            {
                return AddStatusEffectHolder.ParseAddStatusEffect(effect as AddStatusEffect, effectHolder);
            }
            else if (effect is AddStatusEffectBuildUp)
            {
                return AddStatusEffectBuildupHolder.ParseAddStatusEffectBuildup(effect as AddStatusEffectBuildUp, effectHolder);
            }
            else if (effect is ShootProjectile)
            {
                return ShootProjectileHolder.ParseShootProjectile(effect as ShootProjectile, effectHolder);
            }
            else if (effect is ShootBlast)
            {
                return ShootBlastHolder.ParseShootBlast(effect as ShootBlast, effectHolder);
            }
            else if (effect is ImbueWeapon)
            {
                return ImbueWeaponHolder.ParseImbueWeapon(effect as ImbueWeapon, effectHolder);
            }
            else if (effect is RemoveStatusEffect)
            {
                return RemoveStatusEffectHolder.ParseRemoveStatusEffect(effect as RemoveStatusEffect, effectHolder);
            }
            else if (effect is ReduceDurability)
            {
                return ReduceDurabilityHolder.ParseReduceDurability(effect as ReduceDurability, effectHolder);
            }
            else if (effect is AffectStat)
            {
                return AffectStatHolder.ParseAffectStat(effect as AffectStat, effectHolder);
            }
            else if (effect is AffectBurntHealth)
            {
                return AffectBurntHealthHolder.ParseAffectBurntHealth(effect as AffectBurntHealth, effectHolder);
            }
            else if (effect is AffectBurntMana)
            {
                return AffectBurntManaHolder.ParseAffectBurntMana(effect as AffectBurntMana, effectHolder);
            }
            else if (effect is AffectBurntStamina)
            {
                return AffectBurntStaminaHolder.ParseAffectBurntStamina(effect as AffectBurntStamina, effectHolder);
            }
            else if (effect is AffectNeed)
            {
                return AffectNeedHolder.ParseAffectNeed(effect as AffectNeed, effectHolder);
            }
            else if (effect is AffectHealth)
            {
                return AffectHealthHolder.ParseAffectHealth(effect as AffectHealth, effectHolder);
            }
            else if (effect is AffectHealthParentOwner)
            {
                return AffectHealthParentOwnerHolder.ParseAffectHealthParentOwner(effect as AffectHealthParentOwner, effectHolder);
            }
            else if (effect is AffectMana)
            {
                return AffectManaHolder.ParseAffectMana(effect as AffectMana, effectHolder);
            }
            else if (effect is AffectStability)
            {
                return AffectStabilityHolder.ParseAffectStability(effect as AffectStability, effectHolder);
            }
            else if (effect is AffectStamina)
            {
                return AffectStaminaHolder.ParseAffectStamina(effect as AffectStamina, effectHolder);
            }
            else
            {
                if (effect.GetType() != typeof(PlaySoundEffect) 
                    && effect.GetType() != typeof(PlayVFX) 
                    && effect.GetType() != typeof(UseLoadoutAmunition)
                    && effect.GetType() != typeof(UnloadWeapon))
                {
                    Debug.LogWarning("[ParseEffect] Unsupported effect of type: " + effect.GetType());
                }

                return null;
            }
        }
    }
}
