using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using System.IO;
using SideLoader.Model;
using SideLoader.Model.Status;
using SideLoader.SLPacks.Categories;

namespace SideLoader
{
    public class SL_ImbueEffect : SL_StatusBase, ICustomModel
    {
        public Type SLTemplateModel => typeof(SL_ImbueEffect);
        public Type GameModel => typeof(ImbueEffectPreset);

        #region IContentTemplate

        internal override bool Internal_IsCreatingNewID() => this.NewStatusID != -1 && NewStatusID != TargetStatusID;

        internal override bool Internal_DoesTargetExist() => ResourcesPrefabManager.Instance.GetEffectPreset(this.TargetStatusID);

        internal override string Internal_DefaultTemplateName() => $"{AppliedID}_{Name}";

        internal override object Internal_TargetID() => TargetStatusID;

        internal override object Internal_AppliedID() => NewStatusID == -1
                                                            ? TargetStatusID
                                                            : NewStatusID;

        internal override object Internal_GetContent(object id)
        {
            if (string.IsNullOrEmpty((string)id))
                return null;

            if (int.TryParse((string)id, out int result))
            {
                References.RPM_EFFECT_PRESETS.TryGetValue(result, out EffectPreset ret);
                return (ImbueEffectPreset)ret;
            }
            else
                return null;
        }

        internal override IContentTemplate Internal_ParseToTemplate(object content)
            => ParseImbueEffect((ImbueEffectPreset)content);

        internal override void Internal_ActualCreate() => Internal_Apply();

        #endregion

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
                Internal_Apply();
            else
                PackCategory.CSharpTemplates.Add(this);
        }

        private void Internal_Apply()
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
                var path = $@"{pack.GetPathForCategory<StatusCategory>()}\{SubfolderName}\icon.png";

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
