using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_MeleeSkill : SL_AttackSkill
    {
        public bool? Blockable;
        public float? Damage;
        public float? Impact;
        public int? LinecastCount;
        public float? Radius;
        public bool? Unblockable;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var skill = item as MeleeSkill;

            if (this.Blockable != null)
            {
                skill.Blockable = (bool)this.Blockable;
            }

            var detector = skill.MeleeHitDetector;
            if (detector)
            {
                if (this.Damage != null)
                {
                    detector.Damage = (float)this.Damage;
                }
                if (this.Impact != null)
                {
                    detector.Impact = (float)this.Impact;
                }
                if (this.LinecastCount != null)
                {
                    detector.LinecastCount = (int)this.LinecastCount;
                }
                if (this.Radius != null)
                {
                    detector.Radius = (float)this.Radius;
                }
                if (this.Unblockable != null)
                {
                    detector.Unblockable = (bool)this.Unblockable;
                }
            }
            
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var skill = item as MeleeSkill;
            var template = holder as SL_MeleeSkill;

            template.Blockable = skill.Blockable;

            if (skill.MeleeHitDetector is MeleeHitDetector detector)
            {
                template.Damage = detector.Damage;
                template.Impact = detector.Impact;
                template.LinecastCount = detector.LinecastCount;
                template.Radius = detector.Radius;
                template.Unblockable = detector.Unblockable;
            }
        }
    }
}
