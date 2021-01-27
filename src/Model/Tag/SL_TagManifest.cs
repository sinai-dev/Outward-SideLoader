using SideLoader.Model;
using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_TagManifest : IContentTemplate
    {
        #region IContentTemplate

        public string DefaultTemplateName => $"Tags";
        public bool IsCreatingNewID => true;
        public bool DoesTargetExist => true;
        public object TargetID => null;
        public object AppliedID => null;
        public bool CanParseContent => false;
        public bool TemplateAllowedInSubfolder => false;

        public ITemplateCategory PackCategory => SLPackManager.GetCategoryInstance<TagCategory>();

        [XmlIgnore] public string SerializedSLPackName { get; set; }
        [XmlIgnore] public string SerializedSubfolderName { get; set; }
        [XmlIgnore] public string SerializedFilename { get; set; }

        public void ApplyActualTemplate() => this.Internal_Create();

        public object GetContentFromID(object id) => null;
        public IContentTemplate ParseToTemplate(object content) => throw new NotImplementedException();

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
