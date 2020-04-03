using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class CustomSkills
    {
        /// <summary>
        /// Use this to generate a custom SkillSchool, and register it to the game's Skill Tree holder.
        /// </summary>
        public static SkillSchool CreateSkillSchool(string name)
        {
            if ((Resources.Load("_characters/CharacterProgression") as GameObject).transform.Find("Test") is Transform template)
            {
                // instantiate a copy of the dev template
                var customObj = GameObject.Instantiate(template).gameObject;
                GameObject.DontDestroyOnLoad(customObj);
                var CustomTree = customObj.GetComponent<SkillSchool>();

                // set the name to the gameobject and the skill tree name/uid
                customObj.name = name;
                At.SetValue(name, typeof(SkillSchool), CustomTree, "m_defaultName");
                At.SetValue("", typeof(SkillSchool), CustomTree, "m_nameLocKey");
                At.SetValue(new UID(name), typeof(SkillSchool), CustomTree, "m_uid");

                // add it to the game's skill tree holder.
                var list = (At.GetValue(typeof(SkillTreeHolder), SkillTreeHolder.Instance, "m_skillTrees") as SkillSchool[]).ToList();
                list.Add(CustomTree);
                At.SetValue(list.ToArray(), typeof(SkillTreeHolder), SkillTreeHolder.Instance, "m_skillTrees");

                return CustomTree;
            }

            return null;
        }

        /// <summary>
        /// Helper for setting up a Skill Slot on a custom Skill Tree
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <param name="refSkillID"></param>
        /// <param name="requiredMoney"></param>
        /// <param name="requiredSlot"></param>
        /// <param name="isBreakthrough"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static SkillSlot CreateSkillSlot(Transform row, string name, int refSkillID, int requiredMoney, BaseSkillSlot requiredSlot = null, bool isBreakthrough = false, int column = 1)
        {
            var slotObj = new GameObject(name);
            slotObj.transform.parent = row;

            var slot = slotObj.AddComponent<SkillSlot>();
            At.SetValue(ResourcesPrefabManager.Instance.GetItemPrefab(refSkillID) as Skill, typeof(SkillSlot), slot, "m_skill");
            At.SetValue(requiredMoney, typeof(SkillSlot), slot, "m_requiredMoney");
            At.SetValue(column, typeof(BaseSkillSlot), slot as BaseSkillSlot, "m_columnIndex");

            if (requiredSlot != null)
            {
                At.SetValue(requiredSlot, typeof(BaseSkillSlot), slot, "m_requiredSkillSlot");
            }

            if (isBreakthrough)
            {
                slot.IsBreakthrough = true;
            }

            return slot;
        }
    }
}
