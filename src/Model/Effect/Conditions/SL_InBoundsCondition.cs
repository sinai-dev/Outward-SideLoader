using UnityEngine;

namespace SideLoader
{
    public class SL_InBoundsCondition : SL_EffectCondition
    {
        public Bounds Bounds;

        public override void ApplyToComponent<T>(T component)
        {
            (component as InBoundsCondition).Bounds = this.Bounds;
        }

        public override void SerializeEffect<T>(T component)
        {
            this.Bounds = (component as InBoundsCondition).Bounds;
        }
    }
}
