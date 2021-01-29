using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader.Model.Status
{
    [SL_Serialized]
    public abstract class SL_StatusBase : ContentTemplate
    {
        #region IContentTemplate

        public override ITemplateCategory PackCategory => SLPackManager.GetCategoryInstance<StatusCategory>();
        public override string DefaultTemplateName => Internal_DefaultTemplateName();
        public override bool TemplateAllowedInSubfolder => true;

        public override bool DoesTargetExist => Internal_DoesTargetExist();
        public override object TargetID => Internal_TargetID();
        public override object AppliedID => Internal_AppliedID();

        public override bool CanParseContent => true;

        public override void ApplyActualTemplate() => this.Internal_ActualCreate();

        public override object GetContentFromID(object id)
            => Internal_GetContent(id);

        public override ContentTemplate ParseToTemplate(object content)
            => Internal_ParseToTemplate(content);

        #endregion

        internal abstract string Internal_DefaultTemplateName();
        internal abstract bool Internal_IsCreatingNewID();
        internal abstract bool Internal_DoesTargetExist();
        internal abstract object Internal_TargetID();
        internal abstract object Internal_AppliedID();

        internal abstract object Internal_GetContent(object id);
        internal abstract ContentTemplate Internal_ParseToTemplate(object content);

        internal abstract void Internal_ActualCreate();

        public virtual void ExportIcons(Component comp, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }
    }
}
