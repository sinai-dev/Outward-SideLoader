using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_SkillTree
    {
        public string Name;
        public Sprite Sigil;
        public List<SL_SkillRow> SkillRows = new List<SL_SkillRow>();

        public string UID;

        [XmlIgnore]
        private GameObject m_object;

        public SkillSchool CreateBaseSchool()
        {
            SceneManager.sceneLoaded += FixOnMainMenu;

            return CreateSchool();
        }

        private SkillSchool CreateSchool(bool applyRowsInstantly = false)
        {
            var template = (Resources.Load("_characters/CharacterProgression") as GameObject).transform.Find("Test");

            // instantiate a copy of the dev template
            m_object = UnityEngine.Object.Instantiate(template).gameObject;

            var school = m_object.GetComponent<SkillSchool>();

            // set the name to the gameobject and the skill tree name/uid
            m_object.name = this.Name;
            At.SetValue(this.Name, typeof(SkillSchool), school, "m_defaultName");
            At.SetValue("", typeof(SkillSchool), school, "m_nameLocKey");

            if (string.IsNullOrEmpty(this.UID))
            {
                this.UID = this.Name;
            }
            At.SetValue(new UID(this.UID), typeof(SkillSchool), school, "m_uid");

            // fix the breakthrough int
            At.SetValue(-1, typeof(SkillSchool), school, "m_breakthroughSkillIndex");

            // set the sprite
            if (this.Sigil)
            {
                school.SchoolSigil = this.Sigil;
            }

            // add it to the game's skill tree holder.
            var list = (At.GetValue(typeof(SkillTreeHolder), SkillTreeHolder.Instance, "m_skillTrees") as SkillSchool[]).ToList();
            list.Add(school);
            At.SetValue(list.ToArray(), typeof(SkillTreeHolder), SkillTreeHolder.Instance, "m_skillTrees");

            if (applyRowsInstantly)
            {
                ApplyRows();
            }

            return school;
        }

        public void ApplyRows()
        {
            if (!this.m_object)
            {
                SL.Log("Trying to apply SL_SkillSchool but it is not created yet! Call CreateBaseSchool first!");
                return;
            }

            var school = m_object.GetComponent<SkillSchool>();

            At.SetValue(new List<SkillBranch>(), typeof(SkillSchool), school, "m_branches");
            At.SetValue(new List<BaseSkillSlot>(), typeof(SkillSchool), school, "m_skillSlots");

            for (int i = 0; i < 6; i++)
            {
                if (m_object.transform.Find("Row" + i) is Transform row)
                {
                    UnityEngine.Object.DestroyImmediate(row.gameObject);
                }
            }

            foreach (var row in this.SkillRows)
            {
                row.ApplyToSchoolTransform(m_object.transform);
            }

            m_object.transform.parent = SkillTreeHolder.Instance.transform;
            m_object.SetActive(true);
        }

        private void FixOnMainMenu(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.ToLower().Contains("mainmenu"))
            {
                SLPlugin.Instance.StartCoroutine(FixOnMenuCoroutine());
            }
        }

        private IEnumerator FixOnMenuCoroutine()
        {
            yield return new WaitForSeconds(1f);

            while (!SkillTreeHolder.Instance)
            {
                yield return null;
            }

            CreateSchool();
            ApplyRows();
        }
    }

    [SL_Serialized]
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

    [SL_Serialized]
    public class SL_BaseSkillSlot
    {
        public int ColumnIndex;
        public Vector2 RequiredSkillSlot = Vector2.zero;

        /// <summary>
        /// Internal use for setting a required slot.
        /// </summary>
        /// <param name="comp">The component that this SkillSlot is setting. Not the required slot.</param>
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
                SL.Log("Could not set required slot. Maybe it's not set yet?");
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

            var comp = col.AddComponent<SkillSlot>();
            comp.IsBreakthrough = Breakthrough;
            At.SetValue(ResourcesPrefabManager.Instance.GetItemPrefab(SkillID) as Skill, typeof(SkillSlot), comp, "m_skill");
            At.SetValue(SilverCost, typeof(SkillSlot), comp, "m_requiredMoney");
            At.SetValue(ColumnIndex, typeof(BaseSkillSlot), comp as BaseSkillSlot, "m_columnIndex");

            if (this.RequiredSkillSlot != Vector2.zero)
            {
                SetRequiredSlot(comp as BaseSkillSlot);
            }

            return comp;
        }
    }
}
