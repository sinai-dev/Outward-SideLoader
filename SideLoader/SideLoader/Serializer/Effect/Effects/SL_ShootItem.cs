using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    [Obsolete("This class requires the use of ItemExtensions (WeaponLoadoutItem), so I haven't implemented it yet.")]
    public class SL_ShootItem : SL_ShootProjectile
    {


        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);
        }
    }
}
