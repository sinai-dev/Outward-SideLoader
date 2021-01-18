using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.Helpers;
using UnityEngine;
using SideLoader.Inspectors.Reflection;
using SideLoader.UI.Shared;
using System.Reflection;
using SideLoader.UI;
using UnityEngine.UI;
using System.Xml.Serialization;

namespace SideLoader.Inspectors
{
    public class ReflectionInspector : InspectorBase
    {
        #region STATIC

        public static ReflectionInspector ActiveInstance { get; private set; }

        static ReflectionInspector()
        {
            PanelDragger.OnFinishResize += OnContainerResized;
            SLPackListView.OnToggleShow += OnContainerResized;
        }

        private static void OnContainerResized()
        {
            if (ActiveInstance == null)
                return;

            ActiveInstance.m_widthUpdateWanted = true;
        }

        //private static readonly HashSet<string> bl_memberNameStartsWith = new HashSet<string>
        //{
        //    // these are redundant
        //    "get_",
        //    "set_",
        //};

        #endregion

        #region INSTANCE

        public override string TabLabel => m_targetTypeShortName;

        internal CacheMember ParentMember { get; set; }

        internal Type m_targetType;
        internal string m_targetTypeShortName;

        // all cached members of the target
        internal CacheMember[] m_allMembers;
        // filtered members based on current filters
        internal readonly List<CacheMember> m_membersFiltered = new List<CacheMember>();
        // actual shortlist of displayed members
        internal readonly CacheMember[] m_displayedMembers = new CacheMember[30];

        //internal bool m_autoUpdate;

        // UI members

        private GameObject m_content;
        public override GameObject Content
        {
            get => m_content;
            set => m_content = value;
        }

        internal Text m_nameFilterText;
        //internal MemberTypes m_memberFilter;
        //internal Button m_lastActiveMemButton;

        internal PageHandler m_pageHandler;
        internal SliderScrollbar m_sliderScroller;
        internal GameObject m_scrollContent;
        internal RectTransform m_scrollContentRect;

        internal bool m_widthUpdateWanted;
        internal bool m_widthUpdateWaiting;

        public ReflectionInspector(object target) : base(target)
        {
            m_targetType = target.GetType();
            m_targetTypeShortName = UISyntaxHighlight.ParseFullSyntax(m_targetType, false);
        }

        //internal virtual void ChangeTarget(object newTarget)
        //{
        //    if (newTarget == null)
        //    {
        //        SL.LogWarning("New target is null - not changing");
        //        return;
        //    }

        //    this.Target = newTarget;

        //    m_targetType = newTarget.GetType();
        //    m_targetTypeShortName = UISyntaxHighlight.ParseFullSyntax(m_targetType, false);

        //    GameObject.Destroy(this.Content);
        //    Init();

        //    SL.Log("Done init");
        //}

        public virtual void Init()
        {
            ConstructUI();
            CacheMembers(m_targetType);
            FilterMembers();
        }

        public override void SetActive()
        {
            base.SetActive();
            ActiveInstance = this;
        }

        public override void SetInactive()
        {
            base.SetInactive();
            ActiveInstance = null;
        }

        public override void Destroy()
        {
            base.Destroy();

            if (this.Content)
                GameObject.Destroy(this.Content);
        }

        private void OnPageTurned()
        {
            RefreshDisplay();
        }

        //internal bool IsBlacklisted(MethodInfo method) => bl_memberNameStartsWith.Any(it => method.Name.StartsWith(it));

        internal string GetSig(MemberInfo member) => $"{member.DeclaringType.Name}.{member.Name}";
        internal string AppendArgsToSig(ParameterInfo[] args)
        {
            string ret = " (";
            foreach (var param in args)
                ret += $"{param.ParameterType.Name} {param.Name}, ";
            ret += ")";
            return ret;
        }

        public void CacheMembers(Type type)
        {
            var list = new List<CacheMember>();
            var cachedSigs = new HashSet<string>();

            var types = new List<Type> { type };
            while (type.BaseType != null)
            {
                type = type.BaseType;
                types.Add(type);
            }

            var flags = BindingFlags.Public | BindingFlags.Instance;

            for (int i = types.Count - 1; i >= 0; i--)
            {
                var declaringType = types[i];
                IEnumerable<MemberInfo> infos = declaringType.GetFields(flags);

                foreach (FieldInfo member in infos)
                {
                    try
                    {
                        if (member.CustomAttributes.Any(it => it.AttributeType == typeof(XmlIgnoreAttribute)))
                            continue;

                        //SL.Log($"Trying to cache member {sig}...");
                        //SL.Log(member.DeclaringType.FullName + "." + member.Name);

                        var sig = GetSig(member);

                        if (cachedSigs.Contains(sig))
                            continue;

                        cachedSigs.Add(sig);

                        var cache = new CacheField(member, Target, m_scrollContent)
                        {
                            ParentInspector = this
                        };

                        list.Add(cache);
                    }
                    catch (Exception e)
                    {
                        SL.LogWarning($"Exception caching member {member.DeclaringType.FullName}.{member.Name}!");
                        SL.Log(e.ToString());
                    }
                }
            }

            m_allMembers = list.ToArray();
        }

        public override void Update()
        {
            base.Update();

            if (m_widthUpdateWanted)
            {
                if (!m_widthUpdateWaiting)
                    m_widthUpdateWaiting = true;
                else
                {
                    UpdateWidths();
                    m_widthUpdateWaiting = false;
                    m_widthUpdateWanted = false;
                }
            }
        }

        //private void OnMemberFilterClicked(MemberTypes type, Button button)
        //{
        //    if (m_lastActiveMemButton)
        //    {
        //        var lastColors = m_lastActiveMemButton.colors;
        //        lastColors.normalColor = new Color(0.2f, 0.2f, 0.2f);
        //        m_lastActiveMemButton.colors = lastColors;
        //    }

        //    m_memberFilter = type;
        //    m_lastActiveMemButton = button;

        //    var colors = m_lastActiveMemButton.colors;
        //    colors.normalColor = new Color(0.2f, 0.6f, 0.2f);
        //    m_lastActiveMemButton.colors = colors;

        //    FilterMembers(null, true);
        //    m_sliderScroller.m_slider.value = 1f;
        //}

        public void FilterMembers(string nameFilter = null, bool force = false)
        {
            int lastCount = m_membersFiltered.Count;
            m_membersFiltered.Clear();

            nameFilter = nameFilter?.ToLower() ?? m_nameFilterText.text.ToLower();

            foreach (var mem in m_allMembers)
            {
                //// membertype filter
                //if (m_memberFilter != MemberTypes.All && mem.MemInfo.MemberType != m_memberFilter)
                //    continue;

                //if (this is InstanceInspector ii && ii.m_scopeFilter != MemberScopes.All)
                //{
                //    if (mem.IsStatic && ii.m_scopeFilter != MemberScopes.Static)
                //        continue;
                //    else if (!mem.IsStatic && ii.m_scopeFilter != MemberScopes.Instance)
                //        continue;
                //}

                // name filter
                if (!string.IsNullOrEmpty(nameFilter) && !mem.NameForFiltering.Contains(nameFilter))
                    continue;

                m_membersFiltered.Add(mem);
            }

            if (force || lastCount != m_membersFiltered.Count)
                RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            var members = m_membersFiltered;
            m_pageHandler.ListCount = members.Count;

            // disable current members
            for (int i = 0; i < m_displayedMembers.Length; i++)
            {
                var mem = m_displayedMembers[i];
                if (mem != null)
                    mem.Disable();
                else
                    break;
            }

            if (members.Count < 1)
                return;

            foreach (var itemIndex in m_pageHandler)
            {
                if (itemIndex >= members.Count)
                    break;

                CacheMember member = members[itemIndex];
                m_displayedMembers[itemIndex - m_pageHandler.StartIndex] = member;
                member.Enable();
            }

            m_widthUpdateWanted = true;
        }

        internal void UpdateWidths()
        {
            float labelWidth = 125;

            foreach (var cache in m_displayedMembers)
            {
                if (cache == null)
                    break;

                var width = cache.GetMemberLabelWidth(m_scrollContentRect);

                if (width > labelWidth)
                    labelWidth = width;
            }

            float valueWidth = m_scrollContentRect.rect.width - labelWidth - 20;

            foreach (var cache in m_displayedMembers)
            {
                if (cache == null)
                    break;
                cache.SetWidths(labelWidth, valueWidth);
            }
        }

        #region UI CONSTRUCTION

        internal GameObject m_filterAreaObj;
        internal GameObject m_memberListObj;

        internal void ConstructUI()
        {
            var parent = InspectorManager.Instance.m_inspectorContent;
            this.Content = UIFactory.CreateVerticalGroup(parent, new Color(0.15f, 0.15f, 0.15f));
            var mainGroup = Content.GetComponent<VerticalLayoutGroup>();
            mainGroup.childForceExpandHeight = false;
            mainGroup.childForceExpandWidth = true;
            mainGroup.childControlHeight = true;
            mainGroup.childControlWidth = true;
            mainGroup.spacing = 5;
            mainGroup.padding.top = 4;
            mainGroup.padding.left = 4;
            mainGroup.padding.right = 4;
            mainGroup.padding.bottom = 4;

            ConstructTopArea();

            ConstructMemberList();
        }

        internal void ConstructTopArea()
        {
            var nameRowObj = UIFactory.CreateHorizontalGroup(Content, new Color(1, 1, 1, 0));
            var nameRow = nameRowObj.GetComponent<HorizontalLayoutGroup>();
            nameRow.childForceExpandWidth = true;
            nameRow.childForceExpandHeight = true;
            nameRow.childControlHeight = true;
            nameRow.childControlWidth = true;
            nameRow.padding.top = 2;
            var nameRowLayout = nameRowObj.AddComponent<LayoutElement>();
            nameRowLayout.minHeight = 25;
            nameRowLayout.flexibleHeight = 0;
            nameRowLayout.minWidth = 200;
            nameRowLayout.flexibleWidth = 5000;

            var typeLabel = UIFactory.CreateLabel(nameRowObj, TextAnchor.MiddleLeft);
            var typeLabelText = typeLabel.GetComponent<Text>();
            typeLabelText.text = "Type:";
            typeLabelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            var typeLabelTextLayout = typeLabel.AddComponent<LayoutElement>();
            typeLabelTextLayout.minWidth = 40;
            typeLabelTextLayout.flexibleWidth = 0;
            typeLabelTextLayout.minHeight = 25;

            var typeDisplayObj = UIFactory.CreateLabel(nameRowObj, TextAnchor.MiddleLeft);
            var typeDisplayText = typeDisplayObj.GetComponent<Text>();
            typeDisplayText.text = UISyntaxHighlight.ParseFullSyntax(m_targetType, true);
            var typeDisplayLayout = typeDisplayObj.AddComponent<LayoutElement>();
            typeDisplayLayout.minHeight = 25;
            typeDisplayLayout.flexibleWidth = 5000;

            if (this is TemplateInspector)
                (this as TemplateInspector).ConstructTemplateUI();

            ConstructFilterArea();

            //ConstructUpdateRow();
        }

        internal void ConstructFilterArea()
        {
            // Filters

            var filterAreaObj = UIFactory.CreateVerticalGroup(Content, new Color(0.1f, 0.1f, 0.1f));
            var filterLayout = filterAreaObj.AddComponent<LayoutElement>();
            filterLayout.minHeight = 25;
            var filterGroup = filterAreaObj.GetComponent<VerticalLayoutGroup>();
            filterGroup.childForceExpandWidth = true;
            filterGroup.childForceExpandHeight = true;
            filterGroup.childControlWidth = true;
            filterGroup.childControlHeight = true;
            filterGroup.spacing = 4;
            filterGroup.padding.left = 4;
            filterGroup.padding.right = 4;
            filterGroup.padding.top = 4;
            filterGroup.padding.bottom = 4;

            m_filterAreaObj = filterAreaObj;

            // name filter

            var nameFilterRowObj = UIFactory.CreateHorizontalGroup(filterAreaObj, new Color(1, 1, 1, 0));
            var nameFilterGroup = nameFilterRowObj.GetComponent<HorizontalLayoutGroup>();
            nameFilterGroup.childForceExpandHeight = false;
            nameFilterGroup.childForceExpandWidth = false;
            nameFilterGroup.childControlWidth = true;
            nameFilterGroup.childControlHeight = true;
            nameFilterGroup.spacing = 5;
            var nameFilterLayout = nameFilterRowObj.AddComponent<LayoutElement>();
            nameFilterLayout.minHeight = 25;
            nameFilterLayout.flexibleHeight = 0;
            nameFilterLayout.flexibleWidth = 5000;

            var nameLabelObj = UIFactory.CreateLabel(nameFilterRowObj, TextAnchor.MiddleLeft);
            var nameLabelLayout = nameLabelObj.AddComponent<LayoutElement>();
            nameLabelLayout.minWidth = 100;
            nameLabelLayout.minHeight = 25;
            nameLabelLayout.flexibleWidth = 0;
            var nameLabelText = nameLabelObj.GetComponent<Text>();
            nameLabelText.text = "Filter fields:";
            nameLabelText.color = Color.grey;

            var nameInputObj = UIFactory.CreateInputField(nameFilterRowObj, 14, (int)TextAnchor.MiddleLeft, (int)HorizontalWrapMode.Overflow);
            var nameInputLayout = nameInputObj.AddComponent<LayoutElement>();
            nameInputLayout.flexibleWidth = 5000;
            nameInputLayout.minWidth = 100;
            nameInputLayout.minHeight = 25;
            var nameInput = nameInputObj.GetComponent<InputField>();
            nameInput.onValueChanged.AddListener((string val) => { FilterMembers(val); });
            m_nameFilterText = nameInput.textComponent;

            // update button

            var updateButtonObj = UIFactory.CreateButton(nameFilterRowObj, new Color(0.2f, 0.2f, 0.2f));
            var updateBtnLayout = updateButtonObj.AddComponent<LayoutElement>();
            updateBtnLayout.minWidth = 110;
            updateBtnLayout.minHeight = 25;
            updateBtnLayout.flexibleWidth = 0;
            var updateText = updateButtonObj.GetComponentInChildren<Text>();
            updateText.text = "Update Values";
            var updateBtn = updateButtonObj.GetComponent<Button>();
            updateBtn.onClick.AddListener(() =>
            {
                foreach (var member in m_displayedMembers)
                {
                    if (member == null) break;
                    member.UpdateValue();
                }
                //bool orig = m_autoUpdate;
                //m_autoUpdate = true;
                //Update();
                //if (!orig) m_autoUpdate = orig;
            });

            //// membertype filter

            //var memberFilterRowObj = UIFactory.CreateHorizontalGroup(filterAreaObj, new Color(1, 1, 1, 0));
            //var memFilterGroup = memberFilterRowObj.GetComponent<HorizontalLayoutGroup>();
            //memFilterGroup.childForceExpandHeight = false;
            //memFilterGroup.childForceExpandWidth = false;
            //memFilterGroup.childControlWidth = true;
            //memFilterGroup.childControlHeight = true;
            //memFilterGroup.spacing = 5;
            //var memFilterLayout = memberFilterRowObj.AddComponent<LayoutElement>();
            //memFilterLayout.minHeight = 25;
            //memFilterLayout.flexibleHeight = 0;
            //memFilterLayout.flexibleWidth = 5000;

            //var memLabelObj = UIFactory.CreateLabel(memberFilterRowObj, TextAnchor.MiddleLeft);
            //var memLabelLayout = memLabelObj.AddComponent<LayoutElement>();
            //memLabelLayout.minWidth = 100;
            //memLabelLayout.minHeight = 25;
            //memLabelLayout.flexibleWidth = 0;
            //var memLabelText = memLabelObj.GetComponent<Text>();
            //memLabelText.text = "Filter members:";
            //memLabelText.color = Color.grey;

            //AddFilterButton(memberFilterRowObj, MemberTypes.All);
            //AddFilterButton(memberFilterRowObj, MemberTypes.Method);
            //AddFilterButton(memberFilterRowObj, MemberTypes.Property, true);
            //AddFilterButton(memberFilterRowObj, MemberTypes.Field);

            //// Instance filters

            //if (this is InstanceInspector)
            //{
            //    (this as InstanceInspector).ConstructInstanceFilters(filterAreaObj);
            //}
        }

        //private void AddFilterButton(GameObject parent, MemberTypes type, bool setEnabled = false)
        //{
        //    var btnObj = UIFactory.CreateButton(parent, new Color(0.2f, 0.2f, 0.2f));

        //    var btnLayout = btnObj.AddComponent<LayoutElement>();
        //    btnLayout.minHeight = 25;
        //    btnLayout.minWidth = 70;

        //    var text = btnObj.GetComponentInChildren<Text>();
        //    text.text = type.ToString();

        //    var btn = btnObj.GetComponent<Button>();

        //    btn.onClick.AddListener(() => { OnMemberFilterClicked(type, btn); });

        //    var colors = btn.colors;
        //    colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f);

        //    if (setEnabled)
        //    {
        //        colors.normalColor = new Color(0.2f, 0.6f, 0.2f);
        //        m_memberFilter = type;
        //        m_lastActiveMemButton = btn;
        //    }

        //    btn.colors = colors;
        //}

        //internal void ConstructUpdateRow()
        //{
        //    var optionsRowObj = UIFactory.CreateHorizontalGroup(Content, new Color(1, 1, 1, 0));
        //    var optionsLayout = optionsRowObj.AddComponent<LayoutElement>();
        //    optionsLayout.minHeight = 25;
        //    var optionsGroup = optionsRowObj.GetComponent<HorizontalLayoutGroup>();
        //    optionsGroup.childForceExpandHeight = true;
        //    optionsGroup.childForceExpandWidth = false;
        //    optionsGroup.childAlignment = TextAnchor.MiddleLeft;
        //    optionsGroup.spacing = 10;

        //    m_updateRowObj = optionsRowObj;

        //    // update button

        //    var updateButtonObj = UIFactory.CreateButton(optionsRowObj, new Color(0.2f, 0.2f, 0.2f));
        //    var updateBtnLayout = updateButtonObj.AddComponent<LayoutElement>();
        //    updateBtnLayout.minWidth = 110;
        //    updateBtnLayout.flexibleWidth = 0;
        //    var updateText = updateButtonObj.GetComponentInChildren<Text>();
        //    updateText.text = "Update Values";
        //    var updateBtn = updateButtonObj.GetComponent<Button>();
        //    updateBtn.onClick.AddListener(() =>
        //    {
        //        bool orig = m_autoUpdate;
        //        m_autoUpdate = true;
        //        Update();
        //        if (!orig) m_autoUpdate = orig;
        //    });

        //    // auto update

        //    var autoUpdateObj = UIFactory.CreateToggle(optionsRowObj, out Toggle autoUpdateToggle, out Text autoUpdateText);
        //    var autoUpdateLayout = autoUpdateObj.AddComponent<LayoutElement>();
        //    autoUpdateLayout.minWidth = 150;
        //    autoUpdateLayout.minHeight = 25;
        //    autoUpdateText.text = "Auto-update?";
        //    autoUpdateToggle.isOn = false;
        //    autoUpdateToggle.onValueChanged.AddListener((bool val) => { m_autoUpdate = val; });
        //}
        
        internal void ConstructMemberList()
        {
            var scrollobj = UIFactory.CreateScrollView(Content, out m_scrollContent, out m_sliderScroller, new Color(0.05f, 0.05f, 0.05f));

            m_memberListObj = scrollobj;
            m_scrollContentRect = m_scrollContent.GetComponent<RectTransform>();

            var scrollGroup = m_scrollContent.GetComponent<VerticalLayoutGroup>();
            scrollGroup.spacing = 3;
            scrollGroup.padding.left = 0;
            scrollGroup.padding.right = 0;
            scrollGroup.childForceExpandHeight = true;

            m_pageHandler = new PageHandler(m_sliderScroller);
            m_pageHandler.ConstructUI(Content);
            m_pageHandler.OnPageChanged += OnPageTurned;
        }

        #endregion // end UI

        #endregion // end instance
    }
}
