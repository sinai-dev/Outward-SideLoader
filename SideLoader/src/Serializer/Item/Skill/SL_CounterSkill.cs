using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_CounterSkill : SL_MeleeSkill
    {
        public float? BlockMult;
        public float? DamageMult;
        public float? KnockbackMult;

        public DamageType.Types[] BlockDamageTypes;
        public DamageType.Types[] CounterDamageTypes;

        public float? MaxRange;
        public bool? TurnTowardsDealer;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var skill = item as CounterSkill;

            if (this.BlockMult != null)
                skill.BlockMult = (float)this.BlockMult;

            if (this.DamageMult != null)
                skill.DamageMult = (float)this.DamageMult;

            if (this.KnockbackMult != null)
                skill.KnockbackMult = (float)this.KnockbackMult;

            if (this.BlockDamageTypes != null)
            {
                skill.BlockTypes = new List<DamageType.Types>();
                foreach (var type in this.BlockDamageTypes)
                    skill.BlockTypes.Add(type);
            }

            if (this.CounterDamageTypes != null)
            {
                skill.CounterTypes = new List<DamageType.Types>();
                foreach (var type in this.CounterDamageTypes)
                    skill.CounterTypes.Add(type);
            }

            if (this.MaxRange != null)
                skill.MaxRange = (float)this.MaxRange;

            if (this.TurnTowardsDealer != null)
                skill.TurnTowardsDealer = (bool)this.TurnTowardsDealer;
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var skill = item as CounterSkill;

            BlockMult = skill.BlockMult;
            DamageMult = skill.DamageMult;
            KnockbackMult = skill.KnockbackMult;
            BlockDamageTypes = skill.BlockTypes.ToArray();
            CounterDamageTypes = skill.CounterTypes.ToArray();
            MaxRange = skill.MaxRange;
            TurnTowardsDealer = skill.TurnTowardsDealer;
        }
    }
}
