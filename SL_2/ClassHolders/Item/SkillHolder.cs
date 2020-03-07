using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class SkillHolder : ItemHolder
    {
        public float Cooldown;
        public float StaminaCost;
        public float ManaCost;
        public float DurabilityCost;
        public float DurabilityCostPercent;
        public bool IsDLCSkill;

        public List<SkillItemReq> RequiredItems = new List<SkillItemReq>();

        public static SkillHolder ParseSkill(Skill skill, ItemHolder itemHolder)
        {
            var skillHolder = new SkillHolder
            {
                Cooldown = skill.Cooldown,
                StaminaCost = skill.StaminaCost,
                ManaCost = skill.ManaCost,
                DurabilityCost = skill.DurabilityCost,
                DurabilityCostPercent = skill.DurabilityCostPercent,
                IsDLCSkill = skill.IsDLCSkill
            };

            try
            {
                foreach (Skill.ItemRequired itemReq in skill.RequiredItems)
                {
                    skillHolder.RequiredItems.Add(new SkillItemReq
                    {
                        ItemName = itemReq.Item.Name,
                        ItemID = itemReq.Item.ItemID,
                        Consume = itemReq.Consume,
                        Quantity = itemReq.Quantity
                    });
                }
            }
            catch { }

            At.InheritBaseValues(skillHolder, itemHolder);

            return skillHolder;
        }

        public class SkillItemReq
        {
            public string ItemName;
            public int ItemID;
            public int Quantity;
            public bool Consume;
        }
    }
}
