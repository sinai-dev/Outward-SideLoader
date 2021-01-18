using System;
using System.Collections.Generic;
using System.Linq;
using SideLoader.UI;
using SideLoader.UI.Modules;
using SideLoader.UI.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SideLoader.Helpers;
using System.IO;
using SideLoader.Model;
using UnityEngine.EventSystems;

namespace SideLoader.Inspectors
{
    public class SLPackListView
    {
        internal static List<Type> s_templateTypes = new List<Type>();

        public SLPackListView()
        {
            Instance = this;
            ConstructUI();
        }

        public static SLPackListView Instance;

        internal static Action OnToggleShow;

        internal SLPack m_currentPack;

        internal SLPack.SubFolders m_currentSubfolder = SLPack.SubFolders.Items;
        internal Type m_currentGeneratorType;

        private Dropdown m_slPackDropdown;
        private Text m_slPackDropdownLabel;
        private Text m_slPackLabel;

        private GameObject m_pageContent;

        private GameObject m_hotReloadRow;

        private InputField m_createInput;

        private GameObject m_scrollObj;

        private GameObject m_generateObj;
        private Dropdown m_genDropdown;
        internal InputField m_genenratorInput;
        //internal int m_lastInputCaretPos;

        internal ContentAutoCompleter AutoCompleter;

        internal void UpdateAutocompletes()
        {
            AutoCompleter.CheckAutocomplete();
            AutoCompleter.Update();
        }

        public void UseAutocomplete(string idToUse)
        {
            m_genenratorInput.text = idToUse;
            //AutoCompleter.ClearAutocompletes();
            AutoCompleter.m_mainObj.SetActive(false);
        }

        public void Init()
        {
            AutoCompleter = new ContentAutoCompleter();
            AutoCompleter.Init();

            foreach (var type in Serializer.SL_Assembly.GetExportedTypes())
            {
                if (!type.IsAbstract 
                    && !(type.BaseType is IContentTemplate)
                    && typeof(IContentTemplate).IsAssignableFrom(type))
                {
                    s_templateTypes.Add(type);
                }
            }

            s_templateTypes = s_templateTypes.OrderBy(it => it.Name).ToList();

            m_genDropdown.options = new List<Dropdown.OptionData>();
            foreach (var type in s_templateTypes)
                m_genDropdown.options.Add(new Dropdown.OptionData { text = type.Name });

            m_genDropdown.onValueChanged.Invoke(0);

            RefreshLoadedSLPacks();
        }

        public void Update()
        {
            //m_lastInputCaretPos = m_genInput?.caretPosition ?? -1;
        }

        internal void RefreshLoadedSLPacks()
        {
            m_currentPack = null;
            m_slPackDropdown.options.Clear();
            m_slPackDropdownLabel.text = "Choose an SLPack...";
            m_scrollObj.gameObject.SetActive(false);
            m_generateObj.gameObject.SetActive(false);

            m_slPackDropdown.options.Add(new Dropdown.OptionData
            {
                text = "No SLPack selected"
            });

            for (int i = 0; i < SL.Packs.Count; i++)
            {
                var pack = SL.Packs.ElementAt(i).Value;
                m_slPackDropdown.options.Add(new Dropdown.OptionData
                {
                    text = pack.Name
                });
            }
        }

        internal void ClearPackEntryButtons()
        {
            foreach (Transform child in m_pageContent.transform)
                GameObject.Destroy(child.gameObject);
        }

        internal void SetSLPackFromDowndown(int val)
        {
            if (val < 1 || val > SL.Packs.Count)
            {
                RefreshLoadedSLPacks();
                m_slPackLabel.text = $"No pack selected...";
                return;
            }

            m_currentPack = SL.Packs.ElementAt(val -1).Value;
            m_slPackLabel.text = $"<b>Inspecting:</b> {m_currentPack.Name}";

            m_scrollObj.gameObject.SetActive(true);
            m_generateObj.gameObject.SetActive(true);

            RefreshSLPackContentList();
        }

        internal void RefreshSLPackContentList()
        {
            if (m_currentPack == null)
                return;

            ClearPackEntryButtons();

            var dict = m_currentPack.GetContentForSubfolder(m_currentSubfolder);
            foreach (var entry in dict.Values)
                AddSLPackTemplateButton(entry as IContentTemplate);
        }

        private void CreatePack()
        {
            RefreshLoadedSLPacks();

            var name = m_createInput.text;

            var safename = Serializer.ReplaceInvalidChars(name);
            if (name != safename)
            {
                SL.LogWarning("SLPack name contains invalid path characters! Try '" + safename + "'");
                return;
            }

            if (SL.GetSLPack(name) != null)
            {
                SL.LogWarning("Cannot make an SLPack with this name as one already exists!");
                return;
            }

            var slPack = new SLPack
            {
                Name = name,
                InMainSLFolder = true,
            };

            Directory.CreateDirectory(SL.SL_FOLDER + $@"\{name}");
            SL.Packs.Add(name, slPack);

            RefreshLoadedSLPacks();

            for (int i = 0; i < SL.Packs.Count; i++)
            {
                var pack = SL.Packs.ElementAt(i).Value;
                if (pack.Name == name)
                {
                    SetSLPackFromDowndown(i);
                    break;
                }
            }
        }

        internal void BeginConfirmHotReload()
        {
            m_hotReloadRow.transform.GetChild(0).gameObject.SetActive(false);

            var reloadLabelObj = UIFactory.CreateLabel(m_hotReloadRow, TextAnchor.MiddleLeft);
            var reloadText = reloadLabelObj.GetComponent<Text>();
            reloadText.text = "Really reload? This will close all open editor windows!";

            var cancelBtnObj = UIFactory.CreateButton(m_hotReloadRow, new Color(0.2f, 0.2f, 0.2f));
            var cancelLayout = cancelBtnObj.AddComponent<LayoutElement>();
            cancelLayout.minWidth = 80;
            cancelLayout.minHeight = 25;
            cancelLayout.minWidth = 100;
            cancelLayout.flexibleWidth = 0;
            var cancelText = cancelBtnObj.GetComponentInChildren<Text>();
            cancelText.text = "< Cancel";

            var confirmBtnObj = UIFactory.CreateButton(m_hotReloadRow, new Color(1f, 0.3f, 0f));
            var confirmLayout = confirmBtnObj.AddComponent<LayoutElement>();
            confirmLayout.minWidth = 80;
            confirmLayout.minHeight = 25;
            confirmLayout.minWidth = 100;
            confirmLayout.flexibleWidth = 0;
            var confirmText = confirmBtnObj.GetComponentInChildren<Text>();
            confirmText.text = "Confirm";

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

            void Close(bool confirmed)
            {
                GameObject.Destroy(cancelBtnObj);
                GameObject.Destroy(confirmBtnObj);
                GameObject.Destroy(reloadLabelObj);

                if (confirmed)
                {
                    for (int i = 0; i < InspectorManager.Instance.m_currentInspectors.Count; i++)
                        InspectorManager.Instance.m_currentInspectors[i].Destroy();

                    SL.Setup(false);

                    RefreshLoadedSLPacks();
                }

                m_hotReloadRow.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        #region UI CONSTRUCTION

        public void ConstructUI()
        {
            GameObject leftPane = UIFactory.CreateVerticalGroup(HomePage.Instance.Content, new Color(72f / 255f, 72f / 255f, 72f / 255f));
            leftPane.name = "SLPack Pane";
            LayoutElement leftLayout = leftPane.AddComponent<LayoutElement>();
            leftLayout.minWidth = 350;
            leftLayout.flexibleWidth = 0;

            VerticalLayoutGroup leftGroup = leftPane.GetComponent<VerticalLayoutGroup>();
            leftGroup.padding.left = 4;
            leftGroup.padding.right = 4;
            leftGroup.padding.top = 8;
            leftGroup.padding.bottom = 4;
            leftGroup.spacing = 4;
            leftGroup.childControlWidth = true;
            leftGroup.childControlHeight = true;
            leftGroup.childForceExpandWidth = true;
            leftGroup.childForceExpandHeight = false;

            GameObject titleObj = UIFactory.CreateLabel(leftPane, TextAnchor.UpperLeft);
            Text titleLabel = titleObj.GetComponent<Text>();
            titleLabel.text = "Active SL Packs";
            titleLabel.fontSize = 20;
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.minHeight = 30;
            titleLayout.flexibleHeight = 0;

            m_hotReloadRow = UIFactory.CreateVerticalGroup(leftPane, new Color(1, 1, 1, 0));
            var hotReloadGroup = m_hotReloadRow.GetComponent<VerticalLayoutGroup>();
            hotReloadGroup.childForceExpandWidth = true;
            hotReloadGroup.spacing = 5;
            var reloadLayout = m_hotReloadRow.AddComponent<LayoutElement>();
            reloadLayout.minHeight = 25;
            reloadLayout.flexibleHeight = 0;
            var hotReloadBtnObj = UIFactory.CreateButton(m_hotReloadRow, new Color(1f, 0.3f, 0f));
            var refreshLayout = hotReloadBtnObj.AddComponent<LayoutElement>();
            refreshLayout.minHeight = 25;
            var refreshTxt = hotReloadBtnObj.GetComponentInChildren<Text>();
            refreshTxt.text = "Hot Reload";
            var refreshBtn = hotReloadBtnObj.GetComponent<Button>();
            refreshBtn.onClick.AddListener(() =>
            {
                BeginConfirmHotReload();
            });

            var createRow = UIFactory.CreateHorizontalGroup(leftPane, new Color(0.15f, 0.15f, 0.15f));
            var createGroup = createRow.GetComponent<HorizontalLayoutGroup>();
            createGroup.padding = new RectOffset(3, 3, 3, 3);
            createGroup.spacing = 4f;
            createGroup.childForceExpandHeight = true;
            createGroup.childForceExpandWidth = true;
            var createLayout = createRow.AddComponent<LayoutElement>();
            createLayout.minHeight = 30;
            createLayout.flexibleHeight = 0;
            createLayout.flexibleWidth = 9999;

            var createInputObj = UIFactory.CreateInputField(createRow);
            m_createInput = createInputObj.GetComponent<InputField>();
            (m_createInput.placeholder as Text).text = "SLPack name...";
            createInputObj.AddComponent<LayoutElement>().flexibleWidth = 9999;

            var createPackObj = UIFactory.CreateButton(createRow, new Color(0.15f, 0.4f, 0.15f));
            var createBtn = createPackObj.GetComponent<Button>();
            var createBtnLayout = createPackObj.AddComponent<LayoutElement>();
            createBtnLayout.minWidth = 90;
            createBtnLayout.flexibleWidth = 0;
            createBtn.onClick.AddListener(() =>
            {
                CreatePack();
            });
            var createTxt = createPackObj.GetComponentInChildren<Text>();
            createTxt.text = "Create Pack";

            GameObject slPackDropdownObj = UIFactory.CreateDropdown(leftPane, out m_slPackDropdown);
            LayoutElement dropdownLayout = slPackDropdownObj.AddComponent<LayoutElement>();
            dropdownLayout.minHeight = 40;
            dropdownLayout.flexibleHeight = 0;
            dropdownLayout.minWidth = 320;
            dropdownLayout.flexibleWidth = 2;

            m_slPackDropdownLabel = m_slPackDropdown.transform.Find("Label").GetComponent<Text>();
            m_slPackDropdownLabel.text = "Choose an SLPack...";

            m_slPackDropdown.onValueChanged.AddListener(SetSLPackFromDowndown);

            GameObject selectedSLPackTextObj = UIFactory.CreateLabel(leftPane, TextAnchor.MiddleLeft);
            m_slPackLabel = selectedSLPackTextObj.GetComponent<Text>();
            m_slPackLabel.text = "No pack selected...";
            m_slPackLabel.fontSize = 15;
            m_slPackLabel.horizontalOverflow = HorizontalWrapMode.Overflow;

            LayoutElement textLayout = selectedSLPackTextObj.gameObject.AddComponent<LayoutElement>();
            textLayout.minWidth = 210;
            textLayout.flexibleWidth = 120;
            textLayout.minHeight = 20;
            textLayout.flexibleHeight = 0;

            // ====== List view ======

            var subfolderDropObj = UIFactory.CreateDropdown(leftPane, out Dropdown subDrop);
            var subdropLayout = subfolderDropObj.AddComponent<LayoutElement>();
            subdropLayout.minHeight = 25;

            var list = new List<SLPack.SubFolders>();
            foreach (var name in Enum.GetValues(typeof(SLPack.SubFolders)))
            {
                var folder = (SLPack.SubFolders)Enum.Parse(typeof(SLPack.SubFolders), name.ToString());

                if (folder == SLPack.SubFolders.Texture2D
                    || folder == SLPack.SubFolders.AssetBundles
                    || folder == SLPack.SubFolders.AudioClip)
                    continue;

                list.Add(folder);
            }

            subDrop.options = new List<Dropdown.OptionData>();
            foreach (var entry in list)
                subDrop.options.Add(new Dropdown.OptionData { text = entry.ToString() });

            subDrop.value = list.IndexOf(SLPack.SubFolders.Items);

            subDrop.onValueChanged.AddListener((int val) =>
            {
                m_currentSubfolder = list[val];
                RefreshSLPackContentList();
            });

            m_scrollObj = UIFactory.CreateScrollView(leftPane, out m_pageContent, out SliderScrollbar scroller, new Color(0.1f, 0.1f, 0.1f));

            // ======= Generate template area =======

            m_generateObj = UIFactory.CreateVerticalGroup(leftPane, new Color(0.1f, 0.1f, 0.1f));

            var genLabelObj = UIFactory.CreateLabel(m_generateObj, TextAnchor.MiddleLeft);
            var genText = genLabelObj.GetComponent<Text>();
            genText.text = "Template Generator";
            genText.fontSize = 16;

            var genDropObj = UIFactory.CreateDropdown(m_generateObj, out m_genDropdown);
            var dropLayout = genDropObj.AddComponent<LayoutElement>();
            dropLayout.minHeight = 25;
            m_genDropdown.onValueChanged.AddListener((int val) =>
            {
                m_currentGeneratorType = s_templateTypes[val];
            });

            var genGroupLayout = m_generateObj.AddComponent<LayoutElement>();
            genGroupLayout.minHeight = 50;
            genGroupLayout.flexibleWidth = 9999;
            var genGroup = m_generateObj.GetComponent<VerticalLayoutGroup>();
            genGroup.childForceExpandHeight = true;
            genGroup.childForceExpandWidth = true;
            genGroup.padding = new RectOffset(3, 3, 3, 3);
            genGroup.spacing = 5;

            var generateInputObj = UIFactory.CreateInputField(m_generateObj, 14, 3, 0, typeof(AutoCompleteInputField));
            generateInputObj.name = "AutoCompleterInput";
            m_genenratorInput = generateInputObj.GetComponent<AutoCompleteInputField>();
            (m_genenratorInput.placeholder as Text).text = "Clone target ID (if valid)";
            var genLayout = m_genenratorInput.gameObject.AddComponent<LayoutElement>();
            genLayout.minHeight = 25;

            m_genenratorInput.onValueChanged.AddListener((string val) =>
            {
                UpdateAutocompletes();
            });

            var genBtnObj = UIFactory.CreateButton(m_generateObj, new Color(0.15f, 0.45f, 0.15f));
            var genBtnLayout = genBtnObj.AddComponent<LayoutElement>();
            genBtnLayout.minHeight = 25;
            var genBtn = genBtnObj.GetComponent<Button>();
            genBtn.onClick.AddListener(() =>
            {
                if (m_currentPack == null)
                {
                    SL.LogWarning("Cannot generate a template without an inspected SLPack!");
                    return;
                }

                var newTemplate = (IContentTemplate)Activator.CreateInstance(this.m_currentGeneratorType);
                if (newTemplate.CanParseContent)
                {
                    var content = newTemplate.GetContentFromID(m_genenratorInput.text);
                    if (content != null && newTemplate.ParseToTemplate(content) is IContentTemplate parsed)
                    {
                        // todo check if content is assignable from desired type.

                        newTemplate = parsed;
                    }
                    else
                    {
                        SL.LogWarning("Could not find any content from target ID '" + m_genenratorInput.text);
                        newTemplate = null;
                    }
                }

                if (newTemplate != null)
                {
                    newTemplate.SerializedSLPackName = m_currentPack.Name;
                    newTemplate.SerializedSubfolderName = newTemplate.DefaultTemplateName;
                    newTemplate.SerializedFilename = newTemplate.DefaultTemplateName;
                    InspectorManager.Instance.Inspect(newTemplate, m_currentPack);
                }
            });

            var genBtnText = genBtnObj.GetComponentInChildren<Text>();
            genBtnText.text = "Create Template";

            RefreshLoadedSLPacks();
        }

        internal void AddSLPackTemplateButton(IContentTemplate template)
        {
            var rowObj = UIFactory.CreateHorizontalGroup(m_pageContent, new Color(0.15f, 0.15f, 0.15f));
            var rowLayout = rowObj.AddComponent<LayoutElement>();
            rowLayout.minHeight = 25;
            var rowGroup = rowObj.GetComponent<HorizontalLayoutGroup>();
            rowGroup.childForceExpandWidth = false;
            rowGroup.childForceExpandHeight = true;
            rowGroup.spacing = 5;
            rowGroup.padding = new RectOffset(2, 2, 2, 2);

            var labelObj = UIFactory.CreateLabel(rowObj, TextAnchor.MiddleLeft);
            var labelText = labelObj.GetComponent<Text>();
            labelText.text = $"{template.SerializedFilename} ({UISyntaxHighlight.ParseFullSyntax(template.GetType(), false)})";
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 9999;

            var inspectBtnObj = UIFactory.CreateButton(rowObj, new Color(0.1f, 0.3f, 0.1f));
            var inspectLayout = inspectBtnObj.AddComponent<LayoutElement>();
            inspectLayout.minWidth = 60;
            inspectLayout.flexibleWidth = 0;
            var inspectText = inspectBtnObj.GetComponentInChildren<Text>();
            inspectText.text = "Edit";
            var inspectBtn = inspectBtnObj.GetComponent<Button>();

            var refTemplate = template;

            inspectBtn.onClick.AddListener(() => 
            {
                if (m_currentPack == null)
                {
                    SL.LogWarning("Trying to inspect an SL Pack template, but current pack is null!");
                    return;
                }

                InspectorManager.Instance.Inspect(refTemplate, m_currentPack);
            });
        }

        #endregion
    }
}
