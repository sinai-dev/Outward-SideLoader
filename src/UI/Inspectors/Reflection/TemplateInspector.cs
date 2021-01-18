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
        public override string TabLabel => $" <color=cyan>[T]</color> {this.Template.SerializedFilename} ({base.TabLabel})";

        internal IContentTemplate Template;

        public SLPack RefPack;

        internal void UpdateFullPathText()
        {
            var path = $@"<b>XML Path:</b> ";

            if (RefPack.InMainSLFolder)
                path += $@"Mods\SideLoader\{RefPack.Name}\{Template.SLPackSubfolder}\";
            else
                path += $@"BepInEx\plugins\{RefPack.Name}\SideLoader\{Template.SLPackSubfolder}\";

            if (!string.IsNullOrEmpty(this.Template.SerializedSubfolderName))
                path += $@"{Template.SerializedSubfolderName}\";

            m_fullPathLabel.text = $@"{path}{Template.SerializedFilename}.xml";
        }

        public TemplateInspector(object target, SLPack pack) : base(target)
        {
            Template = target as IContentTemplate;
            RefPack = pack;
        }

        internal override void ChangeType(Type newType)
        {
            var origPack = RefPack.Name;
            var origSub = this.Template.SerializedFilename;
            var origFile = this.Template.SerializedSubfolderName;

            base.ChangeType(newType);

            Template.SerializedFilename = origFile;
            Template.SerializedSubfolderName = origSub;
            Template.SerializedSLPackName = origPack;

            UpdateFullPathText();
        }

        internal override void CopyValuesFrom(object data)
        {
            var origPack = RefPack.Name;
            var origSub = this.Template.SerializedFilename;
            var origFile = this.Template.SerializedSubfolderName;

            At.CopyFields(Target, data, null, true);

            Template.SerializedFilename = origFile;
            Template.SerializedSubfolderName = origSub;
            Template.SerializedSLPackName = origPack;
            //this.SubfolderName = origSub;
            //this.Filename = origFile;

            UpdateValues();
        }

        //internal InputField m_subfolderInput;
        //internal InputField m_filenameInput;
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

            //// ========= Subfolder/Filename row =========

            //var pathInputRowObj = UIFactory.CreateHorizontalGroup(vertGroupObj, new Color(1,1,1,0));
            //var pathGroup = pathInputRowObj.GetComponent<HorizontalLayoutGroup>();
            //pathGroup.childForceExpandWidth = true;
            //pathGroup.spacing = 5;

            //if (Template.TemplateAllowedInSubfolder)
            //{
            //    var subLabelObj = UIFactory.CreateLabel(pathInputRowObj, TextAnchor.MiddleRight);
            //    var subLabelLayout = subLabelObj.AddComponent<LayoutElement>();
            //    subLabelLayout.minHeight = 24;
            //    subLabelLayout.minWidth = 130;
            //    subLabelLayout.flexibleWidth = 0;
            //    var subText = subLabelObj.GetComponent<Text>();
            //    subText.text = "Subfolder Name:";

            //    var subInputObj = UIFactory.CreateInputField(pathInputRowObj);
            //    var subLayout = subInputObj.AddComponent<LayoutElement>();
            //    subLayout.minHeight = 24;
            //    subLayout.flexibleWidth = 9999;
            //    var subField = subInputObj.GetComponent<InputField>();
            //    subField.image.color = new Color(0.2f, 0.2f, 0.2f);
            //    (subField.placeholder as Text).text = @"Optional subfolder (used for textures/icons), eg. 'MySubfolderName'";
            //    m_subfolderInput = subField;
            //    subField.onValueChanged.AddListener((string val) => { UpdateFullPathText(); });
            //}

            //var nameLabelObj = UIFactory.CreateLabel(pathInputRowObj, TextAnchor.MiddleRight);
            //var nameLabelLayout = nameLabelObj.AddComponent<LayoutElement>();
            //nameLabelLayout.minHeight = 24;
            //nameLabelLayout.minWidth = 90;
            //nameLabelLayout.flexibleWidth = 0;
            //var nameText = nameLabelObj.GetComponent<Text>();
            //nameText.text = "File Name:";

            //var nameInputObj = UIFactory.CreateInputField(pathInputRowObj);
            //var nameLayout = nameInputObj.AddComponent<LayoutElement>();
            //nameLayout.minHeight = 24;
            //nameLayout.flexibleWidth = 9999;
            //var nameField = nameInputObj.GetComponent<InputField>();
            //nameField.image.color = new Color(0.2f, 0.2f, 0.2f);
            //(nameField.placeholder as Text).text = @"Template file name, eg. 'MyTemplate'";
            //m_filenameInput = nameField;
            //nameField.onValueChanged.AddListener((string val) => { UpdateFullPathText(); });

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

                if (Template.TemplateAllowedInSubfolder && !string.IsNullOrEmpty(this.Template.SerializedSubfolderName))
                {
                    directory += $@"\{this.Template.SerializedSubfolderName}";
                }

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Serializer.SaveToXml(directory, this.Template.SerializedFilename, Target);
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

                if (Template.TemplateAllowedInSubfolder && !string.IsNullOrEmpty(this.Template.SerializedSubfolderName))
                {
                    directory += $@"\{this.Template.SerializedSubfolderName}";
                }

                var path = directory + "\\" + Template.SerializedFilename + ".xml";
                if (!File.Exists(path))
                {
                    SL.LogWarning("No file exists at " + path);
                    return;
                }

                if (Serializer.LoadFromXml(path) is IContentTemplate loadedData)
                {
                    var loadedType = loadedData.GetType();
                    SL.Log("Loaded xml, replacing template with " + loadedType);

                    if (loadedType != m_targetType)
                        ChangeType(loadedType);

                    CopyValuesFrom(loadedData);
                }
            });
        }
    }
}
