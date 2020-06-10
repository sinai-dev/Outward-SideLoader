using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_AttackSkill : SL_Skill
    {
        public List<Weapon.WeaponType> AmmunitionTypes = new List<Weapon.WeaponType>();
        public List<Weapon.WeaponType> RequiredOffHandTypes = new List<Weapon.WeaponType>();
        public List<Weapon.WeaponType> RequiredWeaponTypes = new List<Weapon.WeaponType>();
        public List<string> RequiredWeaponTags = new List<string>();
        public bool? RequireImbue;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var attackSkill = item as AttackSkill;

            if (this.AmmunitionTypes != null)
            {
                attackSkill.AmmunitionTypes = this.AmmunitionTypes;
            }
            if (this.RequiredOffHandTypes != null)
            {
                attackSkill.RequiredOffHandTypes = this.RequiredOffHandTypes;
            }
            if (this.RequiredWeaponTypes != null)
            {
                attackSkill.RequiredWeaponTypes = this.RequiredWeaponTypes;
            }
            if (this.RequireImbue != null)
            {
                attackSkill.RequireImbue = (bool)this.RequireImbue;
            }

            if (this.RequiredWeaponTags != null)
            {
                var list = new List<TagSourceSelector>();
                foreach (var tag in this.RequiredWeaponTags)
                {
                    if (CustomItems.GetTag(tag) is Tag _tag && _tag != Tag.None)
                    {
                        list.Add(new TagSourceSelector(_tag));
                    }
                }
                attackSkill.RequiredTags = list.ToArray();
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_AttackSkill;
            var attackSkill = item as AttackSkill;

            template.AmmunitionTypes = attackSkill.AmmunitionTypes;
            template.RequiredOffHandTypes = attackSkill.RequiredOffHandTypes;
            template.RequiredWeaponTypes = attackSkill.RequiredWeaponTypes;
            template.RequireImbue = attackSkill.RequireImbue;

            foreach (var tag in attackSkill.RequiredTags)
            {
                template.RequiredWeaponTags.Add(tag.Tag.TagName);
            }
        }
    }
}
