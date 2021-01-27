using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader.Model.Status
{
    [SL_Serialized]
    public abstract class SL_StatusBase : IContentTemplate
    {
        #region IContentTemplate

        public string DefaultTemplateName => $"Tags";
        public bool CanParseContent => true;
        public bool TemplateAllowedInSubfolder => true;
        public bool IsCreatingNewID => Internal_IsCreatingNewID();
        public bool DoesTargetExist => Internal_DoesTargetExist();
        public object TargetID => Internal_TargetID();
        public object AppliedID => Internal_AppliedID();

        public ITemplateCategory PackCategory => (ITemplateCategory)SLPackManager.GetCategoryInstance<StatusCategory>();

        [XmlIgnore] public string SerializedSLPackName { get; set; }
        [XmlIgnore] public string SerializedSubfolderName { get; set; }
        [XmlIgnore] public string SerializedFilename { get; set; }

        public void ApplyActualTemplate() => this.Internal_ActualCreate();

        public object GetContentFromID(object id)
            => Internal_GetContent(id);

        public IContentTemplate ParseToTemplate(object content)
            => Internal_ParseToTemplate(content);

        #endregion

        internal abstract string Internal_DefaultTemplateName();
        internal abstract bool Internal_IsCreatingNewID();
        internal abstract bool Internal_DoesTargetExist();
        internal abstract object Internal_TargetID();
        internal abstract object Internal_AppliedID();

        internal abstract object Internal_GetContent(object id);
        internal abstract IContentTemplate Internal_ParseToTemplate(object content);

        internal abstract void Internal_ActualCreate();

        public virtual void ExportIcons(Component comp, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }
    }
}
