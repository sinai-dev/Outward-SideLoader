using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    /// <summary>
    /// Shell class, doesn't require any extra fields, but requires a WeaponLoadoutItem ItemExtension on the Item.
    /// </summary>
    public class SL_ShootItem : SL_ShootProjectile
    {
        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);
        }
    }
}
