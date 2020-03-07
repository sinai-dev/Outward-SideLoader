using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace Dataminer
{
    public class RecipeHolder
    {
        public string StationType;
        public string Name;
        public int RecipeID;

        public List<string> Ingredients = new List<string>();
        public List<ItemQuantityHolder> Results = new List<ItemQuantityHolder>();

        public static RecipeHolder ParseRecipe(Recipe recipe)
        {
            var recipeHolder = new RecipeHolder
            {
                Name = recipe.Name,
                RecipeID = recipe.RecipeID,
                StationType = recipe.CraftingStationType.ToString()
            };

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient)
                {
                    recipeHolder.Ingredients.Add(ingredient.AddedIngredient.Name);
                }
                else
                {
                    recipeHolder.Ingredients.Add(ingredient.AddedIngredientType.Tag.TagName);
                }
            }

            foreach (ItemQuantity item in recipe.Results)
            {
                recipeHolder.Results.Add(new ItemQuantityHolder
                {
                    ItemName = item.Item.Name,
                    Quantity = item.Quantity
                });
            }

            return recipeHolder;
        }

        public static void ParseAllRecipes()
        {
            if (At.GetValue(typeof(RecipeManager), RecipeManager.Instance, "m_recipes") is Dictionary<string, Recipe> recipes)
            {
                foreach (Recipe recipe in recipes.Values)
                {
                    var recipeHolder = ParseRecipe(recipe);

                    string dir = Folders.Prefabs + "/Recipes";
                    string saveName = recipeHolder.Name + " (" + recipeHolder.RecipeID + ")";

                    ListManager.Recipes.Add(recipeHolder.RecipeID.ToString(), recipeHolder);
                    Dataminer.SerializeXML(dir, saveName, recipeHolder, typeof(RecipeHolder), new Type[] { typeof(ItemQuantityHolder) });
                }
            }
        }

        public class ItemQuantityHolder
        {
            public string ItemName;
            public int Quantity;
        }
    }
}
