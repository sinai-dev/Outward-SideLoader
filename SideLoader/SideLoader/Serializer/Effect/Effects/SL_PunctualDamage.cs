﻿using System;
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

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as PunctualDamage;

            comp.Knockback = this.Knockback;
            comp.HitInventory = this.HitInventory;

            var damages = new List<DamageType>();
            foreach (var damage in this.Damage)
            {
                damages.Add(damage.GetDamageType());
            }
            comp.Damages = damages.ToArray();

            if (this.Damages_AI != null)
            {
                var damagesAI = new List<DamageType>();
                foreach (var damage in this.Damages_AI)
                {
                    damagesAI.Add(damage.GetDamageType());
                }
                comp.DamagesAI = damagesAI.ToArray();
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var puncDamage = effect as PunctualDamage;
            var puncHolder = holder as SL_PunctualDamage;

            puncHolder.Knockback = puncDamage.Knockback;
            puncHolder.HitInventory = puncDamage.HitInventory;
            puncHolder.Damage = SL_Damage.ParseDamageArray(puncDamage.Damages);
            puncHolder.Damages_AI = SL_Damage.ParseDamageArray(puncDamage.DamagesAI);
        }
    }
}