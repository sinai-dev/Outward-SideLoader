using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ThrowItem : SL_ShootProjectile
    {
        public ProjectileItem.CollisionBehaviorTypes CollisionBehaviour;
        public ThrowItem.ProjectileBehaviorTypes ProjectileBehaviour;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as ThrowItem;

            comp.CollisionBehavior = this.CollisionBehaviour;
            comp.ProjectileBehavior = this.ProjectileBehaviour;
            comp.DefenseLength = this.DefenseLength;
            comp.DefenseRange = this.DefenseRange;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            var template = holder as SL_ThrowItem;
            var comp = effect as ThrowItem;

            template.CollisionBehaviour = comp.CollisionBehavior;
            template.ProjectileBehaviour = comp.ProjectileBehavior;
            template.DefenseLength = comp.DefenseLength;
            template.DefenseRange = comp.DefenseRange;
        }
    }
}
