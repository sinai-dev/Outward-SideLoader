using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Skill : SL_Item
    {
        public float Cooldown;
        public float StaminaCost;
        public float ManaCost;
        public float DurabilityCost;
        public float DurabilityCostPercent;

        public List<SkillItemReq> RequiredItems = new List<SkillItemReq>();

        // for AttackSkill only
        public List<Weapon.WeaponType> AmmunitionTypes = new List<Weapon.WeaponType>();
        public List<Weapon.WeaponType> RequiredOffHandTypes = new List<Weapon.WeaponType>();
        public List<Weapon.WeaponType> RequiredWeaponTypes = new List<Weapon.WeaponType>();
        public List<string> RequiredWeaponTags = new List<string>();
        public bool RequireImbue;

        // public List<SkillItemReq> RequiredItems = new List<SkillItemReq>();

        public void ApplyToItem(Skill item)
        {
            item.Cooldown = this.Cooldown;
            item.ManaCost = this.ManaCost;
            item.StaminaCost = this.StaminaCost;
            item.DurabilityCost = this.DurabilityCost;
            item.DurabilityCostPercent = this.DurabilityCostPercent;

            if (this.RequiredItems == null)
            {
                item.RequiredItems = new Skill.ItemRequired[0];
            }
            else
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
                attackSkill.AmmunitionTypes = this.AmmunitionTypes;
                attackSkill.RequiredOffHandTypes = this.RequiredOffHandTypes;
                attackSkill.RequiredWeaponTypes = this.RequiredWeaponTypes;
                attackSkill.RequireImbue = this.RequireImbue;

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
                else
                {
                    attackSkill.RequiredTags = new TagSourceSelector[0];
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
