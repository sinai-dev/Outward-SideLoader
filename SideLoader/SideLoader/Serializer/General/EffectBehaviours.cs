using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    /// <summary>Determines how SL_EffectTransforms are applied in various templates.</summary>
    public enum EffectBehaviours
    {
        /// <summary>Everything will be applied on-top of the existing effects, nothing will be removed.</summary>
        NONE,
        /// <summary>Destroys all existing effects and conditions.</summary>
        DestroyEffects,
        /// <summary>Only destroys Transforms if you have defined one with the same TransformName.</summary>
        OverrideEffects
    }
}
