using System;
using System.Reflection;
using UnityEngine;
using SideLoader.Helpers;
using SideLoader.UI;
using UnityEngine.UI;
using System.IO;
using SideLoader.Model;
using System.Collections;

namespace SideLoader.Inspectors.Reflection
{
    public class TemplateInspector : ReflectionInspector
    {
        public override string TabLabel => $" <color=cyan>[T]</color> {this.Filename} ({base.TabLabel})";

        internal IContentTemplate Template;

        public SLPack RefPack;

        public string SubfolderName
        {
            get => m_subfolderInput?.text;
            set 
            {
                m_subfolderInput.text = value;
            }
        }
        public string Filename
        {
            get => m_filenameInput?.text;
            set
            {
                m_filenameInput.text = value;
            }
        }

        internal void UpdateFullPathText()
        {
            var path = $@"Saving/Loading at: <b><color=cyan>{RefPack.Name}</color></b>\{Template.SLPackSubfolder}\";

            string sub = "";
            if (!string.IsNullOrEmpty(this.SubfolderName))
                sub = SubfolderName + "\\";

            m_fullPathLabel.text = $@"{path}{sub}{Filename}.xml";
        }

        public TemplateInspector(object target, SLPack pack) : base(target)
        {
            Template = target as IContentTemplate;
            RefPack = pack;
        }

        //internal override void ChangeTarget(object newTarget)
        //{
        //    Template = newTarget as IContentTemplate;

        //    base.ChangeTarget(newTarget);
        //}

        internal InputField m_subfolderInput;
        internal InputField m_filenameInput;
        //internal InputField m_fullPathInput;
        internal Text m_fullPathLabel;

        public void ConstructTemplateUI()
        {
            if (Template == null)
            {
                SL.LogWarning("Template is null!");
                return;
            }

            var vertGroupObj = UIFactory.CreateVerticalGroup(this.Content, new Color(0.07f, 0.07f, 0.07f));
            var vertGroup = vertGroupObj.GetComponent<VerticalLayoutGroup>();
            vertGroup.spacing = 4f;
            vertGroup.padding = new RectOffset
            {
                bottom = 4,
                top = 4,
                right = 4,
                left = 4
            };

            // ========= Subfolder/Filename row =========

            var pathInputRowObj = UIFactory.CreateHorizontalGroup(vertGroupObj, new Color(1,1,1,0));
            var pathGroup = pathInputRowObj.GetComponent<HorizontalLayoutGroup>();
            pathGroup.childForceExpandWidth = true;
            pathGroup.spacing = 5;

            if (Template.TemplateAllowedInSubfolder)
            {
                var subLabelObj = UIFactory.CreateLabel(pathInputRowObj, TextAnchor.MiddleRight);
                var subLabelLayout = subLabelObj.AddComponent<LayoutElement>();
                subLabelLayout.minHeight = 24;
                subLabelLayout.minWidth = 130;
                subLabelLayout.flexibleWidth = 0;
                var subText = subLabelObj.GetComponent<Text>();
                subText.text = "Subfolder Name:";

                var subInputObj = UIFactory.CreateInputField(pathInputRowObj);
                var subLayout = subInputObj.AddComponent<LayoutElement>();
                subLayout.minHeight = 24;
                subLayout.flexibleWidth = 9999;
                var subField = subInputObj.GetComponent<InputField>();
                subField.image.color = new Color(0.2f, 0.2f, 0.2f);
                (subField.placeholder as Text).text = @"Optional subfolder (used for textures/icons), eg. 'MySubfolderName'";
                m_subfolderInput = subField;
                subField.onValueChanged.AddListener((string val) => { UpdateFullPathText(); });
            }

            var nameLabelObj = UIFactory.CreateLabel(pathInputRowObj, TextAnchor.MiddleRight);
            var nameLabelLayout = nameLabelObj.AddComponent<LayoutElement>();
            nameLabelLayout.minHeight = 24;
            nameLabelLayout.minWidth = 90;
            nameLabelLayout.flexibleWidth = 0;
            var nameText = nameLabelObj.GetComponent<Text>();
            nameText.text = "File Name:";

            var nameInputObj = UIFactory.CreateInputField(pathInputRowObj);
            var nameLayout = nameInputObj.AddComponent<LayoutElement>();
            nameLayout.minHeight = 24;
            nameLayout.flexibleWidth = 9999;
            var nameField = nameInputObj.GetComponent<InputField>();
            nameField.image.color = new Color(0.2f, 0.2f, 0.2f);
            (nameField.placeholder as Text).text = @"Template file name, eg. 'MyTemplate'";
            m_filenameInput = nameField;
            nameField.onValueChanged.AddListener((string val) => { UpdateFullPathText(); });

            // ========= Full path Row =========

            var fullPathRowObj = UIFactory.CreateHorizontalGroup(vertGroupObj, new Color(1, 1, 1, 0));
            var fullPathGroup = fullPathRowObj.GetComponent<HorizontalLayoutGroup>();
            fullPathGroup.childForceExpandWidth = true;
            fullPathGroup.spacing = 5;

            var fullLabel = UIFactory.CreateLabel(fullPathRowObj, TextAnchor.MiddleLeft);
            var fullText = fullLabel.GetComponent<Text>();
            fullText.text = "Saving/Loading at:";
            var fulllayout = fullLabel.AddComponent<LayoutElement>();
            fulllayout.minWidth = 130;
            fulllayout.flexibleWidth = 0;

            m_fullPathLabel = fullText;

            //var fullInputObj = UIFactory.CreateInputField(fullPathRowObj);
            //var fullInputLayout = fullInputObj.AddComponent<LayoutElement>();
            //fullInputLayout.minHeight = 25;
            //fullInputLayout.flexibleWidth = 9999;
            //var fullInput = fullInputObj.GetComponent<InputField>();
            //fullInput.readOnly = true;
            //m_fullPathInput = fullInput;

            // ========= Save/Load Row =========

            var saveLoadRowObj = UIFactory.CreateHorizontalGroup(vertGroupObj, new Color(1, 1, 1, 0));
            var saveLoadGroup = saveLoadRowObj.GetComponent<HorizontalLayoutGroup>();
            saveLoadGroup.spacing = 5;
            saveLoadGroup.padding = new RectOffset(3, 3, 3, 3);
            saveLoadGroup.childForceExpandWidth = true;
            saveLoadGroup.childForceExpandHeight = true;

            var saveBtnObj = UIFactory.CreateButton(saveLoadRowObj, new Color(0.2f, 0.5f, 0.2f));
            var btnLayout = saveBtnObj.AddComponent<LayoutElement>();
            btnLayout.minHeight = 25;
            var saveBtnText = saveBtnObj.GetComponentInChildren<Text>();
            saveBtnText.text = "Save";
            var saveBtn = saveBtnObj.GetComponent<Button>();
            saveBtn.onClick.AddListener(() =>
            {
                if (RefPack == null)
                {
                    SL.LogWarning("Error - RefPack is null on TemplateInspector.Save!");
                    return;
                }

                var directory = RefPack.GetSubfolderPath(Template.SLPackSubfolder);

                if (Template.TemplateAllowedInSubfolder && !string.IsNullOrEmpty(this.SubfolderName))
                {
                    SubfolderName = Serializer.ReplaceInvalidChars(SubfolderName);
                    directory += $@"\{this.SubfolderName}";
                }

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Filename = Serializer.ReplaceInvalidChars(Filename);

                Serializer.SaveToXml(directory, this.Filename, Target);
            });

            var loadBtnObj = UIFactory.CreateButton(saveLoadRowObj, new Color(0.5f, 0.3f, 0.2f));
            var loadLayout = loadBtnObj.AddComponent<LayoutElement>();
            loadLayout.minHeight = 25;
            var loadBtnText = loadBtnObj.GetComponentInChildren<Text>();
            loadBtnText.text = "Load";
            var loadBtn = loadBtnObj.GetComponent<Button>();
            loadBtn.onClick.AddListener(() =>
            {
                if (RefPack == null)
                {
                    SL.LogWarning("Error - RefPack is null on TemplateInspector.Save!");
                    return;
                }

                var directory = RefPack.GetSubfolderPath(Template.SLPackSubfolder);

                if (Template.TemplateAllowedInSubfolder && !string.IsNullOrEmpty(this.SubfolderName))
                {
                    SubfolderName = Serializer.ReplaceInvalidChars(SubfolderName);
                    directory += $@"\{this.SubfolderName}";
                }

                Filename = Serializer.ReplaceInvalidChars(Filename);

                var path = directory + "\\" + Filename + ".xml";
                if (!File.Exists(path))
                {
                    SL.LogWarning("No file exists at " + path);
                    return;
                }

                if (Serializer.LoadFromXml(path) is IContentTemplate loadedData)
                {
                    SL.Log("Loaded xml, replacing template with " + loadedData.GetType());

                    var origPack = RefPack.Name;
                    var origSub = this.SubfolderName;
                    var origFile = this.Filename;

                    At.CopyFields(Template, loadedData, null, true);

                    Template.SerializedFilename = origFile;
                    Template.SerializedSubfolderName = origSub;
                    Template.SerializedSLPackName = origPack;

                    m_targetType = Template.GetType();
                    m_targetTypeShortName = UISyntaxHighlight.ParseFullSyntax(m_targetType, false);

                    GameObject.Destroy(this.Content);
                    Init();
                    SetActive();
                    RefreshDisplay();

                    this.SubfolderName = origSub;
                    this.Filename = origFile;

                    // this.ChangeTarget(template);
                }
            });
        }
    }
}
