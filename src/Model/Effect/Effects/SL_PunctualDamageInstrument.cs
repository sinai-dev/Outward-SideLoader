using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SL_PunctualDamageInstrument : SL_PunctualDamage
    {
        public float DamageCap;
        public float DamagePerCharge;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as PunctualDamageInstrument;

            comp.DamageCap = this.DamageCap;
            comp.DamagePerCharge = this.DamagePerCharge;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as PunctualDamageInstrument;

            this.DamageCap = comp.DamageCap;
            this.DamagePerCharge = comp.DamagePerCharge;
        }
    }
}
