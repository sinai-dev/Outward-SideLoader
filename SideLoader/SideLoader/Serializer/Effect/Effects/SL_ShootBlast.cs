//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;

//namespace SideLoader
//{
//    public class SL_ShootBlast : SL_Effect
//    {
//        public float BlastLifespan;
//        public float RefreshTime;
//        public List<SL_EffectTransform> EffectTransforms = new List<SL_EffectTransform>();

//        public static SL_ShootBlast ParseShootBlast(ShootBlast shootBlast, SL_Effect _effectHolder)
//        {
//            var shootBlastHolder = new SL_ShootBlast 
//            {
//                BlastLifespan = shootBlast.BlastLifespan
//            };

//            At.InheritBaseValues(shootBlastHolder, _effectHolder);

//            if (shootBlast.BaseBlast != null)
//            {
//                if (shootBlast.BaseBlast is LingeringBlast)
//                {
//                    shootBlastHolder.RefreshTime = (shootBlast.BaseBlast as LingeringBlast).RefreshEffectsTime;
//                }
//                else
//                {
//                    shootBlastHolder.RefreshTime = shootBlast.BaseBlast.RefreshTime;
//                }

//                foreach (Transform child in shootBlast.BaseBlast.transform)
//                {
//                    var effectsTransform = SL_EffectTransform.ParseTransform(child);
//                    if (effectsTransform != null && (effectsTransform.Effects.Count > 0 || effectsTransform.ChildEffects.Count > 0))
//                    {
//                        shootBlastHolder.EffectTransforms.Add(effectsTransform);
//                    }
//                }
//            }

//            return shootBlastHolder;
//        }
//    }
//}
