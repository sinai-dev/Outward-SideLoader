using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_PunctualDamageAoE : SL_PunctualDamage
    {
        public float Radius;
        public Shooter.TargetTypes TargetType;
        public bool IgnoreShooter;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as PunctualDamageAoE).Radius = this.Radius;
            (component as PunctualDamageAoE).TargetType = this.TargetType;
            (component as PunctualDamageAoE).IgnoreShooter = this.IgnoreShooter;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            var template = holder as SL_PunctualDamageAoE;
            var comp = effect as PunctualDamageAoE;

            template.Radius = comp.Radius;
            template.TargetType = comp.TargetType;
            template.IgnoreShooter = comp.IgnoreShooter;
        }
    }
}
