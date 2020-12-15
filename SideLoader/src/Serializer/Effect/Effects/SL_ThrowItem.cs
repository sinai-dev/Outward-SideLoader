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

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as ThrowItem;

            CollisionBehaviour = comp.CollisionBehavior;
            ProjectileBehaviour = comp.ProjectileBehavior;
            DefenseLength = comp.DefenseLength;
            DefenseRange = comp.DefenseRange;
        }
    }
}
