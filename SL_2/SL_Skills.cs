using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader_2
{
    /// <summary>
    /// This class just contains a few helper functions for setting up skills. Custom Skills are still mostly defined from the CustomItems class.<br></br>
    /// At the moment, this class contains helpers for setting up Skill Trees
    /// </summary>
    public class SL_Skills : MonoBehaviour
    {
        //public static SL_Skills Instance;

        //internal void Awake()
        //{
        //    Instance = this;
        //}

        // set a skill tree icon for a skill
        public static void SetSkillSmallIcon(int id, string textureName)
        {
            if (SL_Textures.TextureReplacements.ContainsKey(textureName))
            {
                Skill skill = ResourcesPrefabManager.Instance.GetItemPrefab(id) as Skill;
                var tex = SL_Textures.TextureReplacements[textureName];
                tex.filterMode = FilterMode.Bilinear;
                skill.SkillTreeIcon = SL_Textures.CreateSprite(tex);
            }
            else
            {
                SideLoader.Log("Tried to set " + textureName + " as a small icon, but TextureData does not contain this key!", 0);
            }
        }

        /// <summary>
        /// Use this to generate a custom SkillSchool, and register it to the game's Skill Tree holder.
        /// </summary>
        public static SkillSchool CreateSkillSchool(string name)
        {
            if ((Resources.Load("_characters/CharacterProgression") as GameObject).transform.Find("Test") is Transform template)
            {
                // instantiate a copy of the dev template
                var customObj = Instantiate(template).gameObject;
                DontDestroyOnLoad(customObj);
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
        /// Helper for setting up a SkillSlot on a SkillSchool gameobject.
        /// </summary>
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

        /// <summary>
        /// Small helper for destroying all children on a given Transform 't'. Uses DestroyImmediate().
        /// </summary>
        public static void DestroyChildren(Transform t)
        {
            while (t.childCount > 0)
            {
                DestroyImmediate(t.GetChild(0).gameObject);
            }
        }
    }
}
