using System.Collections.Generic;

namespace SideLoader
{
    public class SL_EnchantmentRecipeItem : SL_Item
    {
        public int[] Recipes;

        public override bool ShouldApplyLate => true;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var comp = item as EnchantmentRecipeItem;

            if (this.Recipes != null)
            {
                var list = new List<EnchantmentRecipe>();
                foreach (var id in this.Recipes)
                {
                    if (References.ENCHANTMENT_RECIPES.TryGetValue(id, out EnchantmentRecipe recipe))
                        list.Add(recipe);
                }

                comp.Recipes = list.ToArray();
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var comp = item as EnchantmentRecipeItem;

            this.Description = LocalizationManager.Instance.GetItemDesc(5800001);

            var list = new List<int>();
            foreach (var recipe in comp.Recipes)
                list.Add(recipe.RecipeID);
            this.Recipes = list.ToArray();
        }
    }
}
