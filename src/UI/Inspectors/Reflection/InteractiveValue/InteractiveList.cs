using SideLoader.UI.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SideLoader.UI.Inspectors.Reflection
{
    public class InteractiveList : InteractiveValue
    {
        public InteractiveList(object value, Type valueType) : base(value, valueType)
        {
            if (valueType.IsGenericType)
                m_baseEntryType = valueType.GetGenericArguments()[0];
            else if (valueType.IsArray)
                m_baseEntryType = valueType.GetElementType();
            else
                m_baseEntryType = typeof(object);

            m_typeToAdd = m_baseEntryType;
        }

        public override bool WantInspectBtn => false;
        public override bool HasSubContent => true;
        public override bool SubContentWanted => Value != null;

        //internal IEnumerable RefIEnumerable;
        internal IList RefIList;
        internal readonly Type m_baseEntryType;

        internal readonly List<CacheEnumerated> m_entries = new List<CacheEnumerated>();
        internal readonly CacheEnumerated[] m_displayedEntries = new CacheEnumerated[30];
        internal bool m_recacheWanted = true;

        //internal override void QuickSave()
        //{
        //    foreach (var entry in this.m_entries)
        //        entry.IValue.QuickSave();

        //    base.QuickSave();
        //}

        public override void OnValueUpdated()
        {
            //RefIEnumerable = Value as IEnumerable;
            RefIList = Value as IList;

            if (m_subContentParent && m_subContentParent.activeSelf)
            {
                GetCacheEntries();
                RefreshDisplay();
            }
            else
                m_recacheWanted = true;

            base.OnValueUpdated();
        }

        public override void OnException(CacheMember member)
        {
            base.OnException(member);
        }

        private void OnPageTurned()
        {
            RefreshDisplay();
        }

        internal void AddEntry()
        {
            if (m_typeToAdd == null)
            {
                SL.LogWarning("No type selected!");
                return;
            }

            if (RefIList == null)
            {
                SL.LogWarning("Cannot add to " + this.Value.GetType());
                return;
            }

            object newValue = At.TryCreateDefault(m_typeToAdd);

            if (RefIList.IsFixedSize)
            {
                if (this.FallbackType.IsArray)
                {
                    var array = Value as Array;
                    Resize(ref array, RefIList.Count + 1, -1);
                    Value = array;

                    RefIList = Value as IList;
                    RefIList[RefIList.Count - 1] = newValue;
                }
                else
                {
                    SL.LogWarning("Cannot add to this type: " + FallbackType.GetType());
                    return;
                }
            }
            else
            {
                RefIList.Add(newValue);
            }

            ApplyFromRefIList();
        }

        internal void RemoveEntry(int index)
        {
            if (RefIList.IsFixedSize)
            {
                if (this.FallbackType.IsArray)
                {
                    var array = Value as Array;
                    Resize(ref array, RefIList.Count - 1, index);
                    Value = array;

                    RefIList = Value as IList;
                }
                else
                {
                    SL.LogWarning("Cannot remove from this type: " + FallbackType.GetType());
                    return;
                }
            }
            else
            {
                RefIList.RemoveAt(index);
            }

            ApplyFromRefIList();
        }

        internal void ApplyFromRefIList()
        {
            this.Value = RefIList;
            //this.RefIEnumerable = Value as IEnumerable;
            Owner.SetValue();
            GetCacheEntries();
            RefreshUIForValue();
        }

        internal void Shuffle(int index, int change)
        {
            int newIndex = index + change;
            if (newIndex < 0 || newIndex >= this.m_entries.Count)
                return;

            if (RefIList == null && (RefIList = Value as IList) == null)
            {
                SL.LogWarning("Cannot shuffle type " + this.Value.GetType());
                return;
            }

            object toChange = RefIList[index];
            object toSwap = RefIList[newIndex];

            RefIList[newIndex] = toChange;
            RefIList[index] = toSwap;

            ApplyFromRefIList();
        }

        internal static void Resize(ref Array oldArray, int newSize, int skipIndex)
        {
            Type elementType = oldArray.GetType().GetElementType();

            Array newArray = Array.CreateInstance(elementType, newSize);

            if (skipIndex != -1)
            {
                int i = 0;
                foreach (var entry in oldArray)
                {
                    if (skipIndex != -1 && i != skipIndex)
                        newArray.SetValue(entry, (i > skipIndex && skipIndex != -1)
                                                    ? i - 1
                                                    : i);
                    i++;
                }
            }
            else
                Array.Copy(oldArray, newArray, Math.Min(oldArray.Length, newArray.Length));

            oldArray = newArray;
        }

        public override void RefreshUIForValue()
        {
            base.RefreshUIForValue();

            //GetDefaultLabel();

            if (Value != null)
            {
                string count = "?";
                if (m_recacheWanted && RefIList != null)
                    count = RefIList?.Count.ToString();
                else if (!m_recacheWanted)
                    count = m_entries.Count.ToString();

                m_baseLabel.text = $"[{count}] {m_richValueType}";
            }
            else
            {
                //m_baseLabel.text = DefaultLabel;
            }
        }

        public void GetCacheEntries()
        {
            if (m_entries.Any())
            {
                // maybe improve this, probably could be more efficient i guess

                foreach (var entry in m_entries)
                    entry.Destroy();

                m_entries.Clear();
            }

            if (RefIList != null)
            {
                int index = 0;
                foreach (var entry in RefIList)
                {
                    object entryToUse = entry;
                    if (entryToUse == null)
                        entryToUse = At.TryCreateDefault(this.m_baseEntryType);

                    var cache = new CacheEnumerated(index, this, RefIList, this.m_listContent);
                    cache.CreateIValue(entryToUse, m_baseEntryType);
                    m_entries.Add(cache);

                    cache.Disable();

                    index++;
                }
            }

            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            var entries = m_entries;
            m_pageHandler.ListCount = entries.Count;

            for (int i = 0; i < m_displayedEntries.Length; i++)
            {
                var entry = m_displayedEntries[i];
                if (entry != null)
                    entry.Disable();
                else
                    break;
            }

            if (entries.Count < 1)
                return;

            foreach (var itemIndex in m_pageHandler)
            {
                if (itemIndex >= entries.Count)
                    break;

                CacheEnumerated entry = entries[itemIndex];
                m_displayedEntries[itemIndex - m_pageHandler.StartIndex] = entry;
                entry.Enable();
            }

            //UpdateSubcontentHeight();
        }

        internal override void OnToggleSubcontent(bool active)
        {
            base.OnToggleSubcontent(active);

            if (active && m_recacheWanted)
            {
                m_recacheWanted = false;
                GetCacheEntries();
                RefreshUIForValue();
            }

            RefreshDisplay();
        }

        #region UI CONSTRUCTION

        internal GameObject m_listContent;
        internal LayoutElement m_listLayout;

        internal PageHandler m_pageHandler;

        internal TypeTreeDropdown m_typeDrop;
        internal Type m_typeToAdd;

        public override void ConstructUI(GameObject parent, GameObject subGroup)
        {
            base.ConstructUI(parent, subGroup);
        }

        public override void ConstructSubcontent()
        {
            base.ConstructSubcontent();

            m_pageHandler = new PageHandler(null);
            m_pageHandler.ConstructUI(m_subContentParent);
            m_pageHandler.OnPageChanged += OnPageTurned;

            var scrollObj = UIFactory.CreateVerticalGroup(this.m_subContentParent, new Color(0.08f, 0.08f, 0.08f));
            m_listContent = scrollObj;

            var scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            m_listLayout = Owner.m_mainContent.GetComponent<LayoutElement>();
            m_listLayout.minHeight = 25;
            m_listLayout.flexibleHeight = 0;
            Owner.m_mainRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);

            var scrollGroup = m_listContent.GetComponent<VerticalLayoutGroup>();
            scrollGroup.childForceExpandHeight = true;
            scrollGroup.childControlHeight = true;
            scrollGroup.spacing = 2;
            scrollGroup.padding.top = 5;
            scrollGroup.padding.left = 5;
            scrollGroup.padding.right = 5;
            scrollGroup.padding.bottom = 5;

            var contentFitter = scrollObj.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ConstructAddRow();

            RefreshElementsAfterUpdate();
        }

        internal void ConstructAddRow()
        {
            var addRowObj = UIFactory.CreateHorizontalGroup(m_subContentParent, new Color(1, 1, 1, 0));
            var rowGroup = addRowObj.GetComponent<HorizontalLayoutGroup>();
            rowGroup.childForceExpandWidth = false;
            rowGroup.spacing = 5;
            rowGroup.padding = new RectOffset(3, 3, 3, 3);
            var addRowLayout = addRowObj.AddComponent<LayoutElement>();
            addRowLayout.minHeight = 25;
            addRowLayout.flexibleHeight = 0;

            var inherited = At.GetImplementationsOf(this.m_baseEntryType);
            if (inherited.Count > 1 || m_baseEntryType.IsAbstract || m_baseEntryType.IsInterface)
            {
                m_typeDrop = new TypeTreeDropdown(m_baseEntryType, addRowObj, m_baseEntryType, (Type val) =>
                {
                    m_typeToAdd = val;
                });

                if (m_baseEntryType.IsAbstract || m_baseEntryType.IsInterface)
                {
                    m_typeDrop.m_dropdown.value = 0;
                }
            }

            var addBtnObj = UIFactory.CreateButton(addRowObj, new Color(0.15f, 0.45f, 0.15f));
            var addBtn = addBtnObj.GetComponent<Button>();
            var addbtnLayout = addBtnObj.AddComponent<LayoutElement>();
            addbtnLayout.minHeight = 25;
            addbtnLayout.minWidth = 120;
            addbtnLayout.flexibleWidth = 0;
            addBtn.onClick.AddListener(() =>
            {
                AddEntry();
            });
            addBtnObj.GetComponentInChildren<Text>().text = "Add...";
        }

        #endregion
    }
}
