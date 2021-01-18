using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SideLoader.Inspectors.Reflection
{
    public class CacheEnumerated : CacheObjectBase
    {
        public override Type FallbackType => ParentEnumeration.m_baseEntryType;
        public override bool CanWrite => RefIList != null && ParentEnumeration.Owner.CanWrite;

        public int Index { get; set; }
        public IList RefIList { get; set; }
        public InteractiveEnumerable ParentEnumeration { get; set; }

        public CacheEnumerated(int index, InteractiveEnumerable parentEnumeration, IList refIList, GameObject parentContent)
        {
            this.ParentEnumeration = parentEnumeration;
            this.Index = index;
            this.RefIList = refIList;
            this.m_parentContent = parentContent;
        }

        public override void CreateIValue(object value, Type fallbackType)
        {
            IValue = InteractiveValue.Create(value, fallbackType);
            IValue.Owner = this;
            if (m_rowObj)
                IValue.m_mainContentParent = m_rowObj;
        }

        public override void SetValue()
        {
            RefIList[Index] = IValue.Value;
            ParentEnumeration.Value = RefIList;

            ParentEnumeration.Owner.SetValue();
        }

        internal GameObject m_rowObj;

        internal void BeginConfirmDestroy()
        {
            var wasActiveGOs = new List<GameObject>();
            foreach (Transform child in m_rowObj.transform)
            {
                if (!child.gameObject.activeSelf)
                    continue;

                wasActiveGOs.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }

            var cancelBtnObj = UIFactory.CreateButton(m_rowObj, new Color(0.1f, 0.4f, 0.1f));
            var cancelLayout = cancelBtnObj.AddComponent<LayoutElement>();
            cancelLayout.minWidth = 80;
            cancelLayout.minHeight = 25;
            cancelLayout.minWidth = 100;
            cancelLayout.flexibleWidth = 0;
            var cancelText = cancelBtnObj.GetComponentInChildren<Text>();
            cancelText.text = "< Cancel";

            var confirmBtnObj = UIFactory.CreateButton(m_rowObj, new Color(0.4f, 0.1f, 0.1f));
            var confirmLayout = confirmBtnObj.AddComponent<LayoutElement>();
            confirmLayout.minWidth = 80;
            confirmLayout.minHeight = 25;
            confirmLayout.minWidth = 100;
            confirmLayout.flexibleWidth = 0;
            var confirmText = confirmBtnObj.GetComponentInChildren<Text>();
            confirmText.text = "Delete";

            var cancelBtn = cancelBtnObj.GetComponent<Button>();
            cancelBtn.onClick.AddListener(() =>
            {
                Close(false);
            });

            var confirmBtn = confirmBtnObj.GetComponent<Button>();
            confirmBtn.onClick.AddListener(() =>
            {
                Close(true);
            });

            void Close(bool destroyed)
            {
                GameObject.Destroy(cancelBtnObj);
                GameObject.Destroy(confirmBtnObj);

                if (destroyed)
                    ParentEnumeration.RemoveEntry(this.Index);
                else
                {
                    foreach (var obj in wasActiveGOs)
                        obj.SetActive(true);
                }
            }
        }

        internal override void ConstructUI()
        {
            base.ConstructUI();

            m_rowObj = UIFactory.CreateHorizontalGroup(m_mainContent, new Color(1, 1, 1, 0));
            var rowGroup = m_rowObj.GetComponent<HorizontalLayoutGroup>();
            rowGroup.padding.left = 5;
            rowGroup.padding.right = 2;
            rowGroup.spacing = 4;
            rowGroup.childForceExpandWidth = false;

            var destroyBtnObj = UIFactory.CreateButton(m_rowObj, new Color(0.45f, 0.15f, 0.15f));
            var destroyLayout = destroyBtnObj.AddComponent<LayoutElement>();
            destroyLayout.minWidth = 25;
            destroyLayout.minHeight = 25;
            var destroyText = destroyBtnObj.GetComponentInChildren<Text>();
            destroyText.text = "X";
            var destroyBtn = destroyBtnObj.GetComponent<Button>();
            destroyBtn.onClick.AddListener(() => 
            {
                BeginConfirmDestroy();
            });

            var shuffleUpBtnObj = UIFactory.CreateButton(m_rowObj, new Color(0.15f, 0.15f, 0.15f));
            var shuffleUpLayout = shuffleUpBtnObj.AddComponent<LayoutElement>();
            shuffleUpLayout.minWidth = 25;
            shuffleUpLayout.minHeight = 25;
            var shuffleUpText = shuffleUpBtnObj.GetComponentInChildren<Text>();
            shuffleUpText.text = "^";
            var shuffleUpBtn = shuffleUpBtnObj.GetComponent<Button>();
            shuffleUpBtn.onClick.AddListener(() =>
            {
                ParentEnumeration.Shuffle(this.Index, -1);
            });

            var shuffleDownBtnObj = UIFactory.CreateButton(m_rowObj, new Color(0.15f, 0.15f, 0.15f));
            var shuffleDownLayout = shuffleDownBtnObj.AddComponent<LayoutElement>();
            shuffleDownLayout.minWidth = 25;
            shuffleDownLayout.minHeight = 25;
            var shuffleDownText = shuffleDownBtnObj.GetComponentInChildren<Text>();
            shuffleDownText.text = "v";
            var shuffleDownBtn = shuffleDownBtnObj.GetComponent<Button>();
            shuffleDownBtn.onClick.AddListener(() =>
            {
                ParentEnumeration.Shuffle(this.Index, 1);
            });

            // reverse order set custom buttons to front
            shuffleDownBtnObj.transform.SetAsFirstSibling();
            shuffleUpBtnObj.transform.SetAsFirstSibling();
            destroyBtnObj.transform.SetAsFirstSibling();

            var indexLabelObj = UIFactory.CreateLabel(m_rowObj, TextAnchor.MiddleLeft);
            var indexLayout = indexLabelObj.AddComponent<LayoutElement>();
            indexLayout.minWidth = 20;
            indexLayout.flexibleWidth = 30;
            indexLayout.minHeight = 25;
            var indexText = indexLabelObj.GetComponent<Text>();
            indexText.text = this.Index + ":";

            IValue.m_mainContentParent = m_rowObj;
        }
    }
}
