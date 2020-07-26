using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_WeaponDamageFlurry : SL_WeaponDamage
    {
        public float HitDelay;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as WeaponDamageFlurry).HitDelay = this.HitDelay;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            (holder as SL_WeaponDamageFlurry).HitDelay = (effect as WeaponDamageFlurry).HitDelay;
        }
    }
}
