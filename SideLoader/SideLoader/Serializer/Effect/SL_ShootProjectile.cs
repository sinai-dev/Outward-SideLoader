using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ShootProjectile : SL_Effect
    {
        public string TargetingMode;
        public bool AutoTarget;

        public List<SL_EffectTransform> EffectTransforms = new List<SL_EffectTransform>();

        public static SL_ShootProjectile ParseShootProjectile(ShootProjectile shootProjectile, SL_Effect _effectHolder)
        {
            var shootProjectileHolder = new SL_ShootProjectile
            {
                TargetingMode = shootProjectile.TargetingMode.ToString(),
                AutoTarget = shootProjectile.AutoTarget
            };

            At.InheritBaseValues(shootProjectileHolder, _effectHolder);

            if (shootProjectile.BaseProjectile != null)
            {
                foreach (Transform child in shootProjectile.BaseProjectile.transform)
                {
                    var effectsTransform = SL_EffectTransform.ParseTransform(child);
                    if (effectsTransform != null && (effectsTransform.Effects.Count > 0 || effectsTransform.ChildEffects.Count > 0))
                    {
                        shootProjectileHolder.EffectTransforms.Add(effectsTransform);
                    }
                }
            }

            return shootProjectileHolder;
        }
    }
}
