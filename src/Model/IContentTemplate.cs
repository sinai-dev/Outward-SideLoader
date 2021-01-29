//using SideLoader.SLPacks;
//using System.Xml.Serialization;

//namespace SideLoader.Model
//{
//    public interface ContentTemplate
//    {
//        [XmlIgnore] string SerializedSLPackName { get; set; }
//        [XmlIgnore] string SerializedSubfolderName { get; set; }
//        [XmlIgnore] string SerializedFilename { get; set; }

//        bool IsCreatingNewID { get; }
//        bool DoesTargetExist { get; }

//        object TargetID { get; }
//        object AppliedID { get; }

//        void ApplyActualTemplate();

//        bool CanParseContent { get; }
//        ContentTemplate ParseToTemplate(object content);
//        object GetContentFromID(object id);

//        ITemplateCategory PackCategory { get; }
//        bool TemplateAllowedInSubfolder { get; }
//        string DefaultTemplateName { get; }
//    }
//}
