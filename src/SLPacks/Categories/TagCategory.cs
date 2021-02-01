using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class TagCategory : SLPackTemplateCategory<SL_TagManifest>
    {
        public override string FolderName => "Tags";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.Tags;

        //public override bool ShouldApplyLate(IContentTemplate template) => false;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var manifest = template as SL_TagManifest;
            manifest.ApplyActualTemplate();
        }
    }
}
