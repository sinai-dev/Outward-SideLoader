using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_RangeAttackSkill : SL_AttackSkill
    {
        public bool? AutoLoad;
        public bool? FakeShoot;
        public bool? OverrideAimOffset;
        public Vector2? AimOffset;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var skill = item as RangeAttackSkill;

            if (this.AutoLoad != null)
                skill.AutoLoad = (bool)this.AutoLoad;

            if (this.FakeShoot != null)
                skill.FakeShoot = (bool)this.FakeShoot;

            if (this.OverrideAimOffset != null)
                skill.OverrideAimOffset = (bool)this.OverrideAimOffset;

            if (this.AimOffset != null)
                skill.AimOffset = (Vector2)this.AimOffset;
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var skill = item as RangeAttackSkill;

            AutoLoad = skill.AutoLoad;
            FakeShoot = skill.FakeShoot;
            OverrideAimOffset = skill.OverrideAimOffset;
            AimOffset = skill.AimOffset;
        }
    }
}
