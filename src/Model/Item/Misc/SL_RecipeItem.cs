using SideLoader.SLPacks;

namespace SideLoader
{
    public class SL_RecipeItem : SL_Item
    {
        public string RecipeUID;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            SLPackManager.AddLateApplyListener(OnLateApply, item);
        }

        private void OnLateApply(object[] obj)
        {
            var recipeItem = obj[0] as RecipeItem;

            if (!recipeItem)
                return;

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
