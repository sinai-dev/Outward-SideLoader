using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using System.IO;
using SideLoader.Model;
using SideLoader.Model.Status;

namespace SideLoader
{
    public class SL_ImbueEffect : SL_StatusBase, IContentTemplate<int>
    {
        [XmlIgnore] public string DefaultTemplateName => $"{this.AppliedID}_{this.Name}";
        [XmlIgnore] public bool IsCreatingNewID => this.NewStatusID > 0 && this.NewStatusID != this.TargetStatusID;
        [XmlIgnore] public bool DoesTargetExist => ResourcesPrefabManager.Instance.GetEffectPreset(this.TargetStatusID);
        [XmlIgnore] public int TargetID => this.TargetStatusID;
        [XmlIgnore] public int AppliedID => this.NewStatusID;
        [XmlIgnore] public SLPack.SubFolders SLPackCategory => SLPack.SubFolders.StatusEffects;
        [XmlIgnore] public bool TemplateAllowedInSubfolder => true;

        [XmlIgnore] public bool CanParseContent => true;
        public IContentTemplate ParseToTemplate(object content) => ParseImbueEffect(content as ImbueEffectPreset);
        public object GetContentFromID(object id)
        {
            if (!int.TryParse(id.ToString(), out int parsed))
                return null;

            References.RPM_EFFECT_PRESETS.TryGetValue(parsed, out EffectPreset ret);
            return ret;
        }

        [XmlIgnore] public string SerializedSLPackName
        {
            get => SLPackName;
            set => SLPackName = value;
        }
        [XmlIgnore] public string SerializedSubfolderName 
        {
            get => SubfolderName; 
            set => SubfolderName = value;
        }
        [XmlIgnore] public string SerializedFilename 
        {
            get => m_serializedFilename; 
            set => m_serializedFilename = value;
        }

        public void CreateContent() => this.Internal_Create();

        /// <summary>Invoked when this template is applied during SideLoader's start or hot-reload.</summary>
        public event Action<ImbueEffectPreset> OnTemplateApplied;

        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom item template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore]
        public string SLPackName;
        /// <summary> [NOT SERIALIZED] The name of the folder this custom item is using for textures (MyPack/Items/[SubfolderName]/Textures/).</summary>
        [XmlIgnore]
        public string SubfolderName;

        /// <summary>This is the Preset ID of the Status Effect you want to base from.</summary>
        public int TargetStatusID;
        /// <summary>The new Preset ID for your Status Effect</summary>
        public int NewStatusID;

        public string Name;
        public string Description;

        public EditBehaviours EffectBehaviour = EditBehaviours.Override;
        public SL_EffectTransform[] Effects;

        /// <summary>
        /// Call this to apply your template at Awake or BeforePacksLoaded.
        /// </summary>
        public void Apply()
        {
            if (SL.PacksLoaded)
            {
                SL.LogWarning("Applying a template AFTER SL.OnPacksLoaded has been called. This is not recommended, use SL.BeforePacksLoaded instead.");
                Internal_Create();
            }
            else
                SL.PendingImbues.Add(this);
        }

        private void Internal_Create()
        {
            var imbue = ApplyTemplate();
            this.OnTemplateApplied?.Invoke(imbue);
        }

        internal ImbueEffectPreset ApplyTemplate()
        {
            if (this.NewStatusID <= 0)
                this.NewStatusID = this.TargetStatusID;

            var preset = CustomStatusEffects.CreateCustomImbue(this);

            CustomStatusEffects.SetImbueLocalization(preset, Name, Description);

            // check for custom icon
            if (!string.IsNullOrEmpty(SLPackName) && !string.IsNullOrEmpty(SubfolderName) && SL.Packs[SLPackName] is SLPack pack)
            {
                var path = $@"{pack.GetSubfolderPath(SLPack.SubFolders.StatusEffects)}\{SubfolderName}\icon.png";

                if (File.Exists(path))
                {
                    var tex = CustomTextures.LoadTexture(path, false, false);
                    var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.NONE);

                    preset.ImbueStatusIcon = sprite;
                }
            }

            SL_EffectTransform.ApplyTransformList(preset.transform, Effects, EffectBehaviour);

            return preset;
        }

        public static SL_ImbueEffect ParseImbueEffect(ImbueEffectPreset imbue)
        {
            var template = new SL_ImbueEffect
            {
                TargetStatusID = imbue.PresetID,
                Name = imbue.Name,
                Description = imbue.Description
            };

            //CustomStatusEffects.GetImbueLocalization(imbue, out template.Name, out template.Description);

            var list = new List<SL_EffectTransform>();
            foreach (Transform child in imbue.transform)
            {
                var effectsChild = SL_EffectTransform.ParseTransform(child);

                if (effectsChild.HasContent)
                {
                    list.Add(effectsChild);
                }
            }
            template.Effects = list.ToArray();

            return template;
        }

        public override void ExportIcons(Component comp, string folder)
        {
            base.ExportIcons(comp, folder);

            var imbue = comp as ImbueEffectPreset;

            if (imbue.ImbueStatusIcon)
                CustomTextures.SaveIconAsPNG(imbue.ImbueStatusIcon, folder);
        }
    }
}
