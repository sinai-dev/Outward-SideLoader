using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_TagManifest : IContentTemplate<string>
    {
        #region IContentTemplate
        [XmlIgnore] public string DefaultTemplateName => $"Tags";
        [XmlIgnore] public bool IsCreatingNewID => true;
        [XmlIgnore] public bool DoesTargetExist => true;
        [XmlIgnore] public string TargetID => null;
        [XmlIgnore] public string AppliedID => null;
        [XmlIgnore] public SLPack.SubFolders SLPackCategory => SLPack.SubFolders.Tags;
        [XmlIgnore] public bool TemplateAllowedInSubfolder => false;

        [XmlIgnore] public bool CanParseContent => false;
        public IContentTemplate ParseToTemplate(object content) => throw new NotImplementedException();
        public object GetContentFromID(object id) => null;

        [XmlIgnore]
        public string SerializedSLPackName
        {
            get => SLPackName;
            set => SLPackName = value;
        }
        [XmlIgnore]
        public string SerializedSubfolderName
        {
            get => SubfolderName;
            set => SubfolderName = value;
        }
        [XmlIgnore]
        public string SerializedFilename
        {
            get => m_serializedFilename;
            set => m_serializedFilename = value;
        }
        public void CreateContent() => this.Internal_Create();
        #endregion

        // Actual template

        [XmlIgnore] internal string SLPackName;
        [XmlIgnore] internal string SubfolderName;
        [XmlIgnore] internal string m_serializedFilename;

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
