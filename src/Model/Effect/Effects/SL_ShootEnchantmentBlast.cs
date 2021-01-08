using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ShootEnchantmentBlast : SL_ShootBlast
    {
        public float DamageMultiplier;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as ShootEnchantmentBlast).DamageMultiplier = this.DamageMultiplier;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            DamageMultiplier = (effect as ShootEnchantmentBlast).DamageMultiplier;
        }
    }
}
