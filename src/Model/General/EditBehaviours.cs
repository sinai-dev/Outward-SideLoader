using System;

namespace SideLoader
{
    /// <summary>Determines how SideLoader applies your template to the original object.</summary>
    public enum EditBehaviours
    {
        /// <summary>Will leave the existing objects untouched, and add yours on-top of them (if any).</summary>
        NONE,
        [Obsolete("Use 'Override'")]
        OverrideEffects = 1,
        /// <summary>Will override the existing objects if you have defined an equivalent (for SL_EffectTransform, this means the SL_EffectTransform itself)</summary>
        Override = 1,
        /// <summary>Destroys all existing objects before adding yours (if any).</summary>
        Destroy,
        [Obsolete("Use 'Destroy'")]
        DestroyEffects = 2,
    }
}
