using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader_2
{
    public class PunctualDamageHolder : EffectHolder
    {
        public List<DamageHolder> Damage = new List<DamageHolder>();
        public List<DamageHolder> Damages_AI = new List<DamageHolder>();
        public float Knockback;
        public bool HitInventory;

        public static PunctualDamageHolder ParsePunctualDamage(PunctualDamage damage, EffectHolder effectHolder)
        {
            var punctualDamageHolder = new PunctualDamageHolder
            {
                Knockback = damage.Knockback,
                HitInventory = damage.HitInventory
            };

            At.InheritBaseValues(punctualDamageHolder, effectHolder);

            punctualDamageHolder.Damage = DamageHolder.ParseDamageArray(damage.Damages);
            punctualDamageHolder.Damages_AI = DamageHolder.ParseDamageArray(damage.DamagesAI);

            if (damage is WeaponDamage)
            {
                return WeaponDamageHolder.ParseWeaponDamage(damage as WeaponDamage, punctualDamageHolder);
            }

            return punctualDamageHolder;
        }
    }
}
