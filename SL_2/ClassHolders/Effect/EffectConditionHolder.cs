using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class EffectConditionHolder
    {
        public List<ConditionHolder> Conditions = new List<ConditionHolder>();

        public static EffectConditionHolder ParseEffectCondition(EffectCondition condition)
        {
            var effectConditionHolder = new EffectConditionHolder();

            if (condition is ProximityCondition)
            {
                foreach (Skill.ItemRequired itemreq in (condition as ProximityCondition).ProximityItemReq)
                {
                    effectConditionHolder.Conditions.Add(new ConditionHolder
                    {
                        Condition_Type = "Item Proximity",
                        Requirement = itemreq.Item.Name,
                        Invert = condition.Invert,
                    });
                }
            }
            else if (condition is StatusEffectCondition)
            {
                effectConditionHolder.Conditions.Add(new ConditionHolder
                {
                    Condition_Type = "Status Effect",
                    Requirement = (condition as StatusEffectCondition).StatusEffectPrefab.IdentifierName,
                    Invert = (condition as StatusEffectCondition).Inverse
                });
            }
            else if (condition is PassiveSkillCondition)
            {
                effectConditionHolder.Conditions.Add(new ConditionHolder
                {
                    Condition_Type = "Passive Skill",
                    Requirement = (condition as PassiveSkillCondition).PassiveSkill.Name,
                    Invert = (condition as PassiveSkillCondition).Inverse
                });
            }
            else if (condition is HasStatusEffectEffectCondition)
            {
                var conditionHolder = new ConditionHolder
                {
                    Condition_Type = condition.Invert ? "Target does not have status" : "Target has status",
                    Invert = condition.Invert
                };

                var selector = (condition as HasStatusEffectEffectCondition).StatusEffect;
                if (selector.StatusEffect != null)
                {
                    conditionHolder.Requirement = selector.StatusEffect.IdentifierName;
                }
                else if (selector.StatusFamily != null && StatusEffectFamilyLibrary.Instance.GetStatusEffect(selector.StatusFamily) != null)
                {
                    conditionHolder.Requirement = StatusEffectFamilyLibrary.Instance.GetStatusEffect(selector.StatusFamily).Name;
                }
                else if (selector.StatusType != null)
                {
                    conditionHolder.Requirement = selector.StatusType.Tag.TagName;
                }

                effectConditionHolder.Conditions.Add(conditionHolder);
            }
            else
            {
                Debug.Log("Unsupported effect condition: " + condition.GetType().ToString());
                effectConditionHolder.Conditions.Add(new ConditionHolder
                {
                    Condition_Type = condition.GetType().ToString(),
                    Invert = condition.Invert,
                });
            }

            return effectConditionHolder;
        }

        public class ConditionHolder
        {
            public string Requirement;
            public string Condition_Type;
            public bool Invert;
        }
    }
}
