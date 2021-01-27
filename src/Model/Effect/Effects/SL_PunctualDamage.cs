using System.Collections.Generic;

namespace SideLoader
{
    public class SL_PunctualDamage : SL_Effect
    {
        public List<SL_Damage> Damage = new List<SL_Damage>();
        public List<SL_Damage> Damages_AI = new List<SL_Damage>();
        public float Knockback;
        public bool HitInventory;
        public bool IgnoreHalfResistances;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as PunctualDamage;

            comp.Knockback = this.Knockback;
            comp.HitInventory = this.HitInventory;
            comp.IgnoreHalfResistances = this.IgnoreHalfResistances;

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

        public override void SerializeEffect<T>(T effect)
        {
            var puncDamage = effect as PunctualDamage;

            Knockback = puncDamage.Knockback;
            HitInventory = puncDamage.HitInventory;
            this.IgnoreHalfResistances = puncDamage.IgnoreHalfResistances;

            Damage = SL_Damage.ParseDamageArray(puncDamage.Damages);
            Damages_AI = SL_Damage.ParseDamageArray(puncDamage.DamagesAI);
        }
    }
}
