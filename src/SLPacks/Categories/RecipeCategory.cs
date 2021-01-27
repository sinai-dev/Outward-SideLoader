using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class RecipeCategory : SLPackTemplateCategory<SL_Recipe>
    {
        public override string FolderName => "Recipes";

        public override int LoadOrder => 20;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var recipe = template as SL_Recipe;
            recipe.ApplyActualTemplate();
        }

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
