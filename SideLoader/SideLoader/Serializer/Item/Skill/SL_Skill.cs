using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var skill = item as Skill;

            if (this.Cooldown != null)
            {
                skill.Cooldown = (float)this.Cooldown;
            }
            if (this.ManaCost != null)
            {
                skill.ManaCost = (float)this.ManaCost;
            }
            if (this.StaminaCost != null)
            {
                skill.StaminaCost = (float)this.StaminaCost;
            }
            if (this.DurabilityCost != null)
            {
                skill.DurabilityCost = (float)this.DurabilityCost;
            }
            if (this.DurabilityCostPercent != null)
            {
                skill.DurabilityCostPercent = (float)this.DurabilityCostPercent;
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
                skill.RequiredItems = list.ToArray();
            }

            if (skill.transform.childCount > 0)
            {
                var activationConditions = new List<Skill.ActivationCondition>();

                foreach (Transform child in skill.transform)
                {
                    if (child.name.Contains("Activation"))
                    {
                        foreach (var condition in child.GetComponentsInChildren<EffectCondition>())
                        {
                            var skillCondition = new Skill.ActivationCondition
                            {
                                Condition = condition
                            };
                            if (string.IsNullOrEmpty((string)At.GetValue(typeof(Skill.ActivationCondition), skillCondition, "m_messageLocKey")))
                            {
                                At.SetValue("Notification_Action_Invalid", typeof(Skill.ActivationCondition), skillCondition, "m_messageLocKey");
                            }
                            activationConditions.Add(skillCondition);
                        }
                    }
                }

                At.SetValue(activationConditions.ToArray(), typeof(Skill), skill, "m_additionalConditions");
            }

            if (skill.transform.Find("AdditionalActivationConditions") is Transform additionals)
            {
                
                
                
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var skillHolder = holder as SL_Skill;
            var skill = item as Skill;

            skillHolder.Cooldown = skill.Cooldown;
            skillHolder.StaminaCost = skill.StaminaCost;
            skillHolder.ManaCost = skill.ManaCost;
            skillHolder.DurabilityCost = skill.DurabilityCost;
            skillHolder.DurabilityCostPercent = skill.DurabilityCostPercent;

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
        }

        [SL_Serialized]
        public class SkillItemReq
        {
            public int ItemID;
            public int Quantity;
            public bool Consume;
        }
    }
}
