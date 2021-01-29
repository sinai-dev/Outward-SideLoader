using SideLoader.Model;
using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_TagManifest : ContentTemplate
    {
        #region IContentTemplate

        public override string DefaultTemplateName => $"Tags";
        public override bool TemplateAllowedInSubfolder => false;
        public override ITemplateCategory PackCategory => SLPackManager.GetCategoryInstance<TagCategory>();

        public override void ApplyActualTemplate() => this.Internal_Create();

        #endregion

        // Actual template

        public List<SL_TagDefinition> Tags = new List<SL_TagDefinition>();

        internal void Internal_Create()
        {
            if (this.Tags == null)
                return;

            foreach (var tag in this.Tags)
                tag.CreateTag();
        }
    }
}
