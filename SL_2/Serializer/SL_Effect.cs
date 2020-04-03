using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader_2
{
    public class SL_Effect
    {
        // maybe put some base Effect fields here? like Lifespan or EffectType and stuff. Not sure if needed.


        // will replace this with abstract methods eventually. for now its a lazy method.

        public void ApplyToTransform(Transform t)
        {
            if (this is SL_PunctualDamage punctualDamageHolder)
            {
                punctualDamageHolder.ApplyToTransform(t);
            }
            else if (this is SL_AddStatusEffect addStatusHolder)
            {
                addStatusHolder.ApplyToTransform(t);
            }
            else if (this is SL_AddStatusEffectBuildUp addStatusBuildupHolder)
            {
                addStatusBuildupHolder.ApplyToTransform(t);
            }
            else if (this is SL_ImbueWeapon imbueWeaponHolder)
            {
                imbueWeaponHolder.ApplyToTransform(t);
            }
            else if (this is SL_RemoveStatusEffect removeStatusHolder)
            {
                removeStatusHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectStat affectStatHolder)
            {
                affectStatHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectBurntHealth affectBurntHealthHolder)
            {
                affectBurntHealthHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectBurntMana affectBurntManaHolder)
            {
                affectBurntManaHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectBurntStamina affectBurntStaminaHolder)
            {
                affectBurntStaminaHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectHealth affectHealthHolder)
            {
                affectHealthHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectHealthParentOwner affectHealthParentOwnerHolder) 
            {
                affectHealthParentOwnerHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectStamina affectStaminaHolder)
            {
                affectStaminaHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectMana affectManaHolder)
            {
                affectManaHolder.ApplyToTransform(t);
            }
            else if (this is SL_AffectStability affectStabilityHolder)
            {
                affectStabilityHolder.ApplyToTransform(t);
            }
        }

        // parse existing effect to template (should also be abstract method)

        public static SL_Effect ParseEffect(Effect effect)
        {
            var effectHolder = new SL_Effect();

            if (effect is PunctualDamage)
            {
                return SL_PunctualDamage.ParsePunctualDamage(effect as PunctualDamage, effectHolder);
            }
            else if (effect is AddStatusEffect)
            {
                return SL_AddStatusEffect.ParseAddStatusEffect(effect as AddStatusEffect, effectHolder);
            }
            else if (effect is AddStatusEffectBuildUp)
            {
                return SL_AddStatusEffectBuildUp.ParseAddStatusEffectBuildup(effect as AddStatusEffectBuildUp, effectHolder);
            }
            else if (effect is ImbueWeapon)
            {
                return SL_ImbueWeapon.ParseImbueWeapon(effect as ImbueWeapon, effectHolder);
            }
            else if (effect is RemoveStatusEffect)
            {
                return SL_RemoveStatusEffect.ParseRemoveStatusEffect(effect as RemoveStatusEffect, effectHolder);
            }
            else if (effect is AffectStat)
            {
                return SL_AffectStat.ParseAffectStat(effect as AffectStat, effectHolder);
            }
            else if (effect is AffectBurntHealth)
            {
                return SL_AffectBurntHealth.ParseAffectBurntHealth(effect as AffectBurntHealth, effectHolder);
            }
            else if (effect is AffectBurntMana)
            {
                return SL_AffectBurntMana.ParseAffectBurntMana(effect as AffectBurntMana, effectHolder);
            }
            else if (effect is AffectBurntStamina)
            {
                return SL_AffectBurntStamina.ParseAffectBurntStamina(effect as AffectBurntStamina, effectHolder);
            }
            else if (effect is AffectHealth)
            {
                return SL_AffectHealth.ParseAffectHealth(effect as AffectHealth, effectHolder);
            }
            else if (effect is AffectHealthParentOwner)
            {
                return SL_AffectHealthParentOwner.ParseAffectHealthParentOwner(effect as AffectHealthParentOwner, effectHolder);
            }
            else if (effect is AffectMana)
            {
                return SL_AffectMana.ParseAffectMana(effect as AffectMana, effectHolder);
            }
            else if (effect is AffectStability)
            {
                return SL_AffectStability.ParseAffectStability(effect as AffectStability, effectHolder);
            }
            else if (effect is AffectStamina)
            {
                return SL_AffectStamina.ParseAffectStamina(effect as AffectStamina, effectHolder);
            }
            //else if (effect is AffectNeed)
            //{
            //    return AffectNeedHolder.ParseAffectNeed(effect as AffectNeed, effectHolder);
            //}
            //else if (effect is ShootProjectile)
            //{
            //    return ShootProjectileHolder.ParseShootProjectile(effect as ShootProjectile, effectHolder);
            //}
            //else if (effect is ShootBlast)
            //{
            //    return ShootBlastHolder.ParseShootBlast(effect as ShootBlast, effectHolder);
            //}
            else
            {
                //if (effect.GetType() != typeof(PlaySoundEffect) 
                //    && effect.GetType() != typeof(PlayVFX) 
                //    && effect.GetType() != typeof(UseLoadoutAmunition)
                //    && effect.GetType() != typeof(UnloadWeapon))
                //{
                //    Debug.LogWarning("[ParseEffect] Unsupported effect of type: " + effect.GetType());
                //}

                return null;
            }
        }
    }
}
