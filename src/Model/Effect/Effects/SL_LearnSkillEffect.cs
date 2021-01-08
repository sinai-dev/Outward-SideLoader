using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_LearnSkillEffect : SL_Effect
    {
        public int SkillID;

        public override void ApplyToComponent<T>(T component)
        {
            if (ResourcesPrefabManager.Instance.GetItemPrefab(this.SkillID) is Skill skill)
            {
                (component as LearnSkillEffect).LearntSkill = skill;
            }
            else
            {
                SL.Log("SL_LearnSkillEffect - cannot find a skill with the ID " + this.SkillID);
            }
        }

        public override void SerializeEffect<T>(T effect)
        {
            SkillID = (effect as LearnSkillEffect).LearntSkill?.ItemID ?? -1;
        }
    }
}
