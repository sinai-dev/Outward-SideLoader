using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    public class SL_WeaponDamageTargetHealth : SL_WeaponDamage
    {
        public Vector2 MultiplierHighLowHP;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as WeaponDamageTargetHealth).MultiplierHighLowHP = this.MultiplierHighLowHP;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            this.MultiplierHighLowHP = (effect as WeaponDamageTargetHealth).MultiplierHighLowHP;
        }
    }
}
