using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class RecipeCategory : SLPackTemplateCategory<SL_Recipe>
    {
        public override string FolderName => "Recipes";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.Recipe;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var recipe = template as SL_Recipe;
            recipe.ApplyActualTemplate();
        }

        //public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
