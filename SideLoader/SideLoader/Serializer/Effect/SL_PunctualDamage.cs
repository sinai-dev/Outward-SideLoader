using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_PunctualDamage : SL_Effect
    {
        public List<SL_Damage> Damage = new List<SL_Damage>();
        public List<SL_Damage> Damages_AI = new List<SL_Damage>();
        public float Knockback;
        public bool HitInventory;

        public new void ApplyToTransform(Transform t)
        {
            PunctualDamage component;
            if (this is SL_WeaponDamage)
            {
                component = t.gameObject.AddComponent<WeaponDamage>();
            }
            else
            {
                component = t.gameObject.AddComponent<PunctualDamage>();
            }

            component.Knockback = this.Knockback;
            component.HitInventory = this.HitInventory;

            var damages = new List<DamageType>();
            foreach (var damage in this.Damage)
            {
                damages.Add(damage.GetDamageType());
            }
            component.Damages = damages.ToArray();

            if (this.Damages_AI != null)
            {
                var damagesAI = new List<DamageType>();
                foreach (var damage in this.Damages_AI)
                {
                    damagesAI.Add(damage.GetDamageType());
                }
                component.DamagesAI = damagesAI.ToArray();            
            }

            if (this is SL_WeaponDamage weaponDamageHolder)
            {
                weaponDamageHolder.ApplyToComponent(component as WeaponDamage);
            }
        }

        public static SL_PunctualDamage ParsePunctualDamage(PunctualDamage damage, SL_Effect effectHolder)
        {
            var punctualDamageHolder = new SL_PunctualDamage
            {
                Knockback = damage.Knockback,
                HitInventory = damage.HitInventory
            };

            At.InheritBaseValues(punctualDamageHolder, effectHolder);

            punctualDamageHolder.Damage = SL_Damage.ParseDamageArray(damage.Damages);
            punctualDamageHolder.Damages_AI = SL_Damage.ParseDamageArray(damage.DamagesAI);

            if (damage is WeaponDamage)
            {
                return SL_WeaponDamage.ParseWeaponDamage(damage as WeaponDamage, punctualDamageHolder);
            }

            return punctualDamageHolder;
        }
    }
}
