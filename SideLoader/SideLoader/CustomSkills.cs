using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader.CustomSkills
{
    public class SL_SkillTree
    {
        public string Name;
        public List<SL_SkillRow> SkillRows = new List<SL_SkillRow>();

        [XmlIgnore]
        private GameObject m_object;

        public SkillSchool CreateBaseSchool()
        {
            var template = (Resources.Load("_characters/CharacterProgression") as GameObject).transform.Find("Test");

            // instantiate a copy of the dev template
            var schoolObj = GameObject.Instantiate(template).gameObject;
            GameObject.DontDestroyOnLoad(schoolObj);
            var school = schoolObj.GetComponent<SkillSchool>();

            // set the name to the gameobject and the skill tree name/uid
            schoolObj.name = this.Name;
            At.SetValue(this.Name, typeof(SkillSchool), school, "m_defaultName");
            At.SetValue("", typeof(SkillSchool), school, "m_nameLocKey");
            At.SetValue(new UID(this.Name), typeof(SkillSchool), school, "m_uid");

            // add it to the game's skill tree holder.
            var list = (At.GetValue(typeof(SkillTreeHolder), SkillTreeHolder.Instance, "m_skillTrees") as SkillSchool[]).ToList();
            list.Add(school);
            At.SetValue(list.ToArray(), typeof(SkillTreeHolder), SkillTreeHolder.Instance, "m_skillTrees");

            this.m_object = school.gameObject;
            return school;
        }

        public void ApplyRows()
        {
            if (this.m_object == null)
            {
                SL.Log("Trying to apply SL_SkillSchool but it is not created yet! Call CreateBaseSchool first!", 1);
            }

            foreach (var row in this.SkillRows)
            {
                row.ApplyToSchoolTransform(m_object.transform);
            }

            this.m_object.SetActive(true);
        }
    }

    public class SL_SkillRow
    {
        public int RowIndex;

        public List<SL_BaseSkillSlot> Slots = new List<SL_BaseSkillSlot>();

        public void ApplyToSchoolTransform(Transform schoolTransform)
        {
            var row = schoolTransform.Find("Row" + RowIndex);
            if (!row)
            {
                row = new GameObject("Row" + this.RowIndex).transform;
                row.parent = schoolTransform;
                row.gameObject.AddComponent<SkillBranch>();
            }
            CustomItems.DestroyChildren(row);

            foreach (var slot in this.Slots)
            {
                if (slot is SL_SkillSlotFork)
                {
                    (slot as SL_SkillSlotFork).ApplyToRow(row);
                }
                else if (slot is SL_SkillSlot)
                {
                    (slot as SL_SkillSlot).ApplyToRow(row);
                }
            }
        }
    }

    public class SL_BaseSkillSlot
    {
        public int ColumnIndex;
        public Vector2 RequiredSkillSlot = Vector2.zero;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="RequiredSlot">X is the Row, Y is the Column</param>
        public void SetRequiredSlot(BaseSkillSlot comp)
        {
            bool success = false;
            var reqrow = RequiredSkillSlot.x;
            var reqcol = RequiredSkillSlot.y;

            if (comp.transform.root.Find("Row" + reqrow) is Transform reqrowTrans
                && reqrowTrans.Find("Col" + reqcol) is Transform reqcolTrans)
            {
                var reqslot = reqcolTrans.GetComponent<BaseSkillSlot>();
                if (reqslot)
                {
                    At.SetValue(reqslot, typeof(BaseSkillSlot), comp as BaseSkillSlot, "m_requiredSkillSlot");
                    success = true;
                }
            }

            if (!success)
            {
                SL.Log("Could not set required slot. Maybe it's not set yet?", 1);
            }
        }
    }

    public class SL_SkillSlotFork : SL_BaseSkillSlot
    {
        public SL_SkillSlot Choice1;
        public SL_SkillSlot Choice2;

        public void ApplyToRow(Transform row)
        {
            var col = new GameObject("Col" + this.ColumnIndex);
            col.transform.parent = row;

            var comp = col.AddComponent<SkillSlotFork>();
            At.SetValue(this.ColumnIndex, typeof(BaseSkillSlot), comp as BaseSkillSlot, "m_columnIndex");

            if (this.RequiredSkillSlot != Vector2.zero)
            {
                SetRequiredSlot(comp as BaseSkillSlot);
            }

            Choice1.ApplyToRow(col.transform);
            Choice2.ApplyToRow(col.transform);
        }
    }

    public class SL_SkillSlot : SL_BaseSkillSlot
    {
        public int SkillID;
        public int SilverCost;
        public bool Breakthrough;

        public SkillSlot ApplyToRow(Transform row)
        {
            var col = new GameObject("Col" + this.ColumnIndex);
            col.transform.parent = row;

            var slot = col.AddComponent<SkillSlot>();
            At.SetValue(ResourcesPrefabManager.Instance.GetItemPrefab(SkillID) as Skill, typeof(SkillSlot), slot, "m_skill");
            At.SetValue(SilverCost, typeof(SkillSlot), slot, "m_requiredMoney");
            At.SetValue(ColumnIndex, typeof(BaseSkillSlot), slot as BaseSkillSlot, "m_columnIndex");

            if (this.RequiredSkillSlot != Vector2.zero)
            {
                SetRequiredSlot(slot as BaseSkillSlot);
            }

            if (Breakthrough)
            {
                slot.IsBreakthrough = true;
            }

            return slot;
        }
    }
}
