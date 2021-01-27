using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class TagCategory : SLPackTemplateCategory<SL_TagManifest>
    {
        public override string FolderName => "Tags";

        public override int LoadOrder => 5;

        public override bool ShouldApplyLate(IContentTemplate template) => false;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var manifest = template as SL_TagManifest;
            manifest.ApplyActualTemplate();
        }
    }
}
