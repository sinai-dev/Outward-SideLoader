using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Skill : SL_Item
    {
        public float? Cooldown;
        public float? StaminaCost;
        public float? ManaCost;
        public float? DurabilityCost;
        public float? DurabilityCostPercent;

        public List<SkillItemReq> RequiredItems = new List<SkillItemReq>();

        // for AttackSkill only
        public List<Weapon.WeaponType> AmmunitionTypes = new List<Weapon.WeaponType>();
        public List<Weapon.WeaponType> RequiredOffHandTypes = new List<Weapon.WeaponType>();
        public List<Weapon.WeaponType> RequiredWeaponTypes = new List<Weapon.WeaponType>();
        public List<string> RequiredWeaponTags = new List<string>();
        public bool? RequireImbue;

        // public List<SkillItemReq> RequiredItems = new List<SkillItemReq>();

        public void ApplyToItem(Skill item)
        {
            if (this.Cooldown != null)
            {
                item.Cooldown = (float)this.Cooldown;
            }
            if (this.ManaCost != null)
            {
                item.ManaCost = (float)this.ManaCost;
            }
            if (this.StaminaCost != null)
            {
                item.StaminaCost = (float)this.StaminaCost;
            }
            if (this.DurabilityCost != null)
            {
                item.DurabilityCost = (float)this.DurabilityCost;
            }
            if (this.DurabilityCostPercent != null)
            {
                item.DurabilityCostPercent = (float)this.DurabilityCostPercent;
            }

            if (this.RequiredItems != null)
            {
                var list = new List<Skill.ItemRequired>();
                foreach (var req in this.RequiredItems)
                {
                    if (ResourcesPrefabManager.Instance.GetItemPrefab(req.ItemID) is Item reqItem)
                    {
                        list.Add(new Skill.ItemRequired()
                        {
                            Item = reqItem,
                            Consume = req.Consume,
                            Quantity = req.Quantity
                        });
                    }
                }
                item.RequiredItems = list.ToArray();
            }

            if (item is AttackSkill attackSkill)
            {
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
        }

        public static SL_Skill ParseSkill(Skill skill, SL_Item itemHolder)
        {
            var skillHolder = new SL_Skill
            {
                Cooldown = skill.Cooldown,
                StaminaCost = skill.StaminaCost,
                ManaCost = skill.ManaCost,
                DurabilityCost = skill.DurabilityCost,
                DurabilityCostPercent = skill.DurabilityCostPercent,
            };

            if (skill.RequiredItems != null)
            {
                foreach (Skill.ItemRequired itemReq in skill.RequiredItems)
                {
                    if (itemReq.Item != null)
                    {
                        skillHolder.RequiredItems.Add(new SkillItemReq
                        {
                            ItemID = itemReq.Item.ItemID,
                            Consume = itemReq.Consume,
                            Quantity = itemReq.Quantity
                        });
                    }
                }
            }

            if (skill is AttackSkill attackSkill)
            {
                skillHolder.AmmunitionTypes = attackSkill.AmmunitionTypes;
                skillHolder.RequiredOffHandTypes = attackSkill.RequiredOffHandTypes;
                skillHolder.RequiredWeaponTypes = attackSkill.RequiredWeaponTypes;
                skillHolder.RequireImbue = attackSkill.RequireImbue;
            }

            At.InheritBaseValues(skillHolder, itemHolder);

            return skillHolder;
        }

        public class SkillItemReq
        {
            public int ItemID;
            public int Quantity;
            public bool Consume;
        }
    }
}
