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

        // public List<SkillItemReq> RequiredItems = new List<SkillItemReq>();

        public void ApplyToItem(Skill item)
        {
            item.Cooldown = this.Cooldown;
            item.ManaCost = this.ManaCost;
            item.StaminaCost = this.StaminaCost;
            item.DurabilityCost = this.DurabilityCost;
            item.DurabilityCostPercent = this.DurabilityCostPercent;
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

            //try
            //{
            //    foreach (Skill.ItemRequired itemReq in skill.RequiredItems)
            //    {
            //        skillHolder.RequiredItems.Add(new SkillItemReq
            //        {
            //            ItemName = itemReq.Item.Name,
            //            ItemID = itemReq.Item.ItemID,
            //            Consume = itemReq.Consume,
            //            Quantity = itemReq.Quantity
            //        });
            //    }
            //}
            //catch { }

            At.InheritBaseValues(skillHolder, itemHolder);

            return skillHolder;
        }

        //public class SkillItemReq
        //{
        //    public string ItemName;
        //    public int ItemID;
        //    public int Quantity;
        //    public bool Consume;
        //}
    }
}
