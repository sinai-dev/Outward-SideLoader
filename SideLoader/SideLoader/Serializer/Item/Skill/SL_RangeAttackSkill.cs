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
            {
                skill.AutoLoad = (bool)this.AutoLoad;
            }
            if (this.FakeShoot != null)
            {
                skill.FakeShoot = (bool)this.FakeShoot;
            }
            if (this.OverrideAimOffset != null)
            {
                skill.OverrideAimOffset = (bool)this.OverrideAimOffset;
            }
            if (this.AimOffset != null)
            {
                skill.AimOffset = (Vector2)this.AimOffset;
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_RangeAttackSkill;
            var skill = item as RangeAttackSkill;

            template.AutoLoad = skill.AutoLoad;
            template.FakeShoot = skill.FakeShoot;
            template.OverrideAimOffset = skill.OverrideAimOffset;
            template.AimOffset = skill.AimOffset;
        }
    }
}
