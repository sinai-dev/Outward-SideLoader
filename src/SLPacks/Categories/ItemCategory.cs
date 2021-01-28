using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class ItemCategory : SLPackTemplateCategory<SL_Item>
    {
        public override string FolderName => "Items";

        public override int LoadOrder => 15;

        public override void ApplyTemplate(IContentTemplate template)
        {
            var item = template as SL_Item;

            item.ApplyActualTemplate();
        }

        //public override bool ShouldApplyLate(IContentTemplate template) => (template as SL_Item).ShouldApplyLate;
    }
}
