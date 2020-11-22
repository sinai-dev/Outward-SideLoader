using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AttackSkill : SL_Skill
    {
        public Weapon.WeaponType[] AmmunitionTypes;
        public Weapon.WeaponType[] RequiredOffHandTypes;
        public Weapon.WeaponType[] RequiredWeaponTypes;
        public string[] RequiredWeaponTags;
        public bool? RequireImbue;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var attackSkill = item as AttackSkill;

            if (this.AmmunitionTypes != null)
            {
                attackSkill.AmmunitionTypes = this.AmmunitionTypes.ToList();
            }
            if (this.RequiredOffHandTypes != null)
            {
                attackSkill.RequiredOffHandTypes = this.RequiredOffHandTypes.ToList();
            }
            if (this.RequiredWeaponTypes != null)
            {
                attackSkill.RequiredWeaponTypes = this.RequiredWeaponTypes.ToList();
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

            if (item is PistolSkill pistolSkill && pistolSkill.transform.Find("NormalReload") is Transform reload)
            {
                At.SetValue(reload.gameObject, typeof(PistolSkill), pistolSkill, "m_alternateAnimConditionsHolder");

                foreach (var icon in pistolSkill.m_alternateIcons)
                    At.SetValue(reload.gameObject, typeof(PistolSkill.AlternateIcon), icon, "m_conditionHolder");
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_AttackSkill;
            var attackSkill = item as AttackSkill;

            if (attackSkill.AmmunitionTypes != null)
            {
                template.AmmunitionTypes = attackSkill.AmmunitionTypes.ToArray();
            }
            if (attackSkill.RequiredOffHandTypes != null)
            {
                template.RequiredOffHandTypes = attackSkill.RequiredOffHandTypes.ToArray();
            }
            if (attackSkill.RequiredWeaponTypes != null)
            {
                template.RequiredWeaponTypes = attackSkill.RequiredWeaponTypes.ToArray();
            }
            
            template.RequireImbue = attackSkill.RequireImbue;

            if (attackSkill.RequiredTags != null)
            {
                var tagList = new List<string>();
                foreach (var tag in attackSkill.RequiredTags)
                {
                    tagList.Add(tag.Tag.TagName);
                }
                template.RequiredWeaponTags = tagList.ToArray();
            }
        }
    }
}
