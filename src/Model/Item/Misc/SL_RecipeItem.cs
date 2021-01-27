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

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var recipeItem = item as RecipeItem;

            RecipeUID = recipeItem.Recipe.UID;
        }
    }
}
