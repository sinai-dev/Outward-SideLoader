using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ShootProjectilePistol : SL_ShootProjectile
    {
        public bool UseShot;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as ShootProjectilePistol).UseShot = this.UseShot;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            (holder as SL_ShootProjectilePistol).UseShot = (effect as ShootProjectilePistol).UseShot;
        }
    }
}
