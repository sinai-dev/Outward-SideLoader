using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class ShootProjectileHolder : EffectHolder
    {
        public string TargetingMode;
        public bool AutoTarget;

        public List<EffectTransformHolder> EffectTransforms = new List<EffectTransformHolder>();

        public static ShootProjectileHolder ParseShootProjectile(ShootProjectile shootProjectile, EffectHolder _effectHolder)
        {
            var shootProjectileHolder = new ShootProjectileHolder
            {
                TargetingMode = shootProjectile.TargetingMode.ToString(),
                AutoTarget = shootProjectile.AutoTarget
            };

            At.InheritBaseValues(shootProjectileHolder, _effectHolder);

            if (shootProjectile.BaseProjectile != null)
            {
                foreach (Transform child in shootProjectile.BaseProjectile.transform)
                {
                    var effectsTransform = EffectTransformHolder.ParseTransform(child);
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
