using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_RecipeItem : SL_Item
    {
        public string RecipeUID;

        public void ApplyToItem(RecipeItem item)
        {
            if (this.RecipeUID != null && CustomItems.ALL_RECIPES.ContainsKey(this.RecipeUID))
            {
                var recipe = CustomItems.ALL_RECIPES[this.RecipeUID];

                item.Recipe = recipe;
            }
        }

        public static SL_RecipeItem ParseRecipeItem(RecipeItem item, SL_Item itemHolder)
        {
            var recipeHolder = new SL_RecipeItem()
            {
                RecipeUID = item.Recipe.UID,
            };

            At.InheritBaseValues(recipeHolder as SL_Item, itemHolder);

            return recipeHolder;
        }
    }
}
