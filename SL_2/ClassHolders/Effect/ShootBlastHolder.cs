using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class ShootBlastHolder : EffectHolder
    {
        public float BlastLifespan;
        public float RefreshTime;
        public List<EffectTransformHolder> EffectTransforms = new List<EffectTransformHolder>();

        public static ShootBlastHolder ParseShootBlast(ShootBlast shootBlast, EffectHolder _effectHolder)
        {
            var shootBlastHolder = new ShootBlastHolder 
            {
                BlastLifespan = shootBlast.BlastLifespan
            };

            At.InheritBaseValues(shootBlastHolder, _effectHolder);

            if (shootBlast.BaseBlast != null)
            {
                if (shootBlast.BaseBlast is LingeringBlast)
                {
                    shootBlastHolder.RefreshTime = (shootBlast.BaseBlast as LingeringBlast).RefreshEffectsTime;
                }
                else
                {
                    shootBlastHolder.RefreshTime = shootBlast.BaseBlast.RefreshTime;
                }

                foreach (Transform child in shootBlast.BaseBlast.transform)
                {
                    var effectsTransform = EffectTransformHolder.ParseTransform(child);
                    if (effectsTransform != null && (effectsTransform.Effects.Count > 0 || effectsTransform.ChildEffects.Count > 0))
                    {
                        shootBlastHolder.EffectTransforms.Add(effectsTransform);
                    }
                }
            }

            return shootBlastHolder;
        }
    }
}
