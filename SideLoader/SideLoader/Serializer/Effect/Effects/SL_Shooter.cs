using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace SideLoader
{
    /// <summary>
    /// Patch to re-enable the disabled blast/projectile prefabs before they are used. 
    /// We need to disable them when we clone them, but here they need to be activated again.
    /// </summary>
    [HarmonyPatch(typeof(Shooter), "Setup", new Type[] { typeof(TargetingSystem), typeof(Transform) })]
    public class ShootProjectile_Setup
    {
        [HarmonyPrefix]
        public static void Prefix(Shooter __instance)
        {
            if (__instance is ShootProjectile shootProjectile && shootProjectile.BaseProjectile is Projectile projectile && !projectile.gameObject.activeSelf)
            {
                projectile.gameObject.SetActive(true);
                EnableEffects(projectile.gameObject);
                
            }
            else if (__instance is ShootBlast shootBlast && shootBlast.BaseBlast is Blast blast && !blast.gameObject.activeSelf)
            {
                blast.gameObject.SetActive(true);
                EnableEffects(blast.gameObject);
            }
        }

        private static void EnableEffects(GameObject obj)
        {
            foreach (var effect in obj.GetComponentsInChildren<Effect>(true))
            {
                if (!effect.enabled)
                {
                    effect.enabled = true;
                }
            }
            foreach (var condition in obj.GetComponentsInChildren<EffectCondition>(true))
            {
                if (!condition.enabled)
                {
                    condition.enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Abstract base class for SL_ShootBlast and SL_ShootProjectile
    /// </summary>
    public abstract class SL_Shooter : SL_Effect
    {
        public Shooter.CastPositionType CastPosition;
        public Vector3 LocalPositionAdd;
        public bool NoAim;
        public Shooter.TargetTypes TargetType;
        public string TransformName;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Shooter;

            comp.CastPosition = this.CastPosition;
            comp.LocalCastPositionAdd = this.LocalPositionAdd;
            comp.NoAim = this.NoAim;
            comp.TargetType = this.TargetType;
            comp.TransformName = this.TransformName;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var comp = effect as Shooter;
            var template = holder as SL_Shooter;

            template.CastPosition = comp.CastPosition;
            template.NoAim = comp.NoAim;
            template.LocalPositionAdd = comp.LocalCastPositionAdd;
            template.TargetType = comp.TargetType;
            template.TransformName = comp.TransformName;
        }
    }
}
