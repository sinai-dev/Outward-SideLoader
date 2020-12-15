using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_PassiveSkillCondition : SL_EffectCondition
    {
        public int ReqSkillID;

        public override void ApplyToComponent<T>(T component)
        {
            var skill = ResourcesPrefabManager.Instance.GetItemPrefab(ReqSkillID) as PassiveSkill;

            if (!skill)
            {
                SL.Log("SL_PassiveSkillCondition: Could not find a Passive Skill with the ID " + this.ReqSkillID);
                return;
            }

            component.Invert = false;

            (component as PassiveSkillCondition).Inverse = this.Invert;
            (component as PassiveSkillCondition).PassiveSkill = skill;
        }

        public override void SerializeEffect<T>(T component)
        {
            Invert = (component as PassiveSkillCondition).Inverse;
            ReqSkillID = (component as PassiveSkillCondition).PassiveSkill.ItemID;
        }
    }
}
