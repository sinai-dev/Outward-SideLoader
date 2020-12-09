using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_RecipeItem : SL_Item
    {
        public override bool ShouldApplyLate => true;

        public string RecipeUID;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var recipeItem = item as RecipeItem;

            if (this.RecipeUID != null && References.ALL_RECIPES.ContainsKey(this.RecipeUID))
            {
                var recipe = References.ALL_RECIPES[this.RecipeUID];

                recipeItem.Recipe = recipe;
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_RecipeItem;
            var recipeItem = item as RecipeItem;

            template.RecipeUID = recipeItem.Recipe.UID;
        }
    }
}
