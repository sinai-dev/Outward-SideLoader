using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class ItemCategory : SLPackTemplateCategory<SL_Item>
    {
        public override string FolderName => "Items";

        public override int LoadOrder => 15;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var item = template as SL_Item;

            item.ApplyActualTemplate();
        }

        protected internal override void OnHotReload()
        {
            base.OnHotReload();

            SL_Item.CurrentlyAppliedTemplates.Clear();
            SL_Skill.s_customSkills.Clear();
        }
    }
}
