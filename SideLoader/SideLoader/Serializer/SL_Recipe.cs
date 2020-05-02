using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace SideLoader
{
    public class SL_Recipe
    {
        [XmlIgnore]
        private bool m_applied = false;

        public string UID = "";

        // public string Name = "";
        public Recipe.CraftingType StationType = Recipe.CraftingType.Survival;

        public List<Ingredient> Ingredients = new List<Ingredient>();
        public List<ItemQty> Results = new List<ItemQty>();

        public void ApplyRecipe()
        {
            if (m_applied)
            {
                SL.Log("Trying to apply an SL_Recipe that is already applied! This is not allowed.", 0);
                return;
            }

            var results = new List<ItemQuantity>();
            foreach (var result in this.Results)
            {
                var resultItem = ResourcesPrefabManager.Instance.GetItemPrefab(result.ItemID);
                if (!resultItem)
                {
                    SL.Log("Error: Could not get recipe result id : " + result.ItemID);
                    return;
                }
                results.Add(new ItemQuantity(resultItem, result.Quantity));
            }

            var ingredients = new List<RecipeIngredient>();
            foreach (var ingredient in this.Ingredients)
            {
                var ingredientItem = ResourcesPrefabManager.Instance.GetItemPrefab(ingredient.Ingredient_ItemID);
                if (!ingredientItem)
                {
                    SL.Log("Error: Could not get ingredient id : " + ingredient.Ingredient_ItemID);
                    return;
                }

                ingredients.Add(new RecipeIngredient()
                {
                    ActionType = ingredient.Type,
                    AddedIngredient = ingredientItem,
                    AddedIngredientType = ingredient.Ingredient_Tag == null ? null : new TagSourceSelector(CustomItems.GetTag(ingredient.Ingredient_Tag))
                });
            }

            var recipe = ScriptableObject.CreateInstance("Recipe") as Recipe;

            recipe.SetCraftingType(this.StationType);

            At.SetValue(results.ToArray(), typeof(Recipe), recipe, "m_results");
            recipe.SetRecipeIngredients(ingredients.ToArray());            

            // set or generate UID
            if (string.IsNullOrEmpty(this.UID))
            {
                var uid = $"{recipe.Results[0].Item.ItemID}{recipe.Results[0].Quantity}";
                foreach (var ing in recipe.Ingredients)
                {
                    if (ing.AddedIngredient != null)
                    {
                        uid += $"{ing.AddedIngredient.ItemID}";
                    }
                    else if (ing.AddedIngredientType != null)
                    {
                        uid += $"{ing.AddedIngredientType.Tag.TagName}";
                    }
                }
                At.SetValue(new UID(uid), typeof(Recipe), recipe, "m_uid");
            }
            else
            {
                At.SetValue(new UID(this.UID), typeof(Recipe), recipe, "m_uid");
            }

            recipe.Init();

            // fix Recipe Manager dictionaries to contain our recipe
            var dict = At.GetValue(typeof(RecipeManager), RecipeManager.Instance, "m_recipes") as Dictionary<string, Recipe>;
            var dict2 = At.GetValue(typeof(RecipeManager), RecipeManager.Instance, "m_recipeUIDsPerUstensils") as Dictionary<Recipe.CraftingType, List<UID>>;

            dict.Add(recipe.UID, recipe);
            if (!dict2.ContainsKey(recipe.CraftingStationType))
            {
                dict2.Add(recipe.CraftingStationType, new List<UID>());
            }
            dict2[recipe.CraftingStationType].Add(recipe.UID);

            SL.Log("Defined recipe " + recipe.Name + "_" + recipe.UID);
            m_applied = true;
        }

        public static SL_Recipe ParseRecipe(Recipe recipe)
        {
            var recipeHolder = new SL_Recipe
            {
                //Name = recipe.Name,
                //RecipeID = recipe.RecipeID,
                StationType = recipe.CraftingStationType,
            };

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (ingredient.ActionType == RecipeIngredient.ActionTypes.AddSpecificIngredient)
                {
                    recipeHolder.Ingredients.Add(new Ingredient() 
                    {
                        Type = ingredient.ActionType,
                        Ingredient_ItemID = ingredient.AddedIngredient.ItemID
                    });
                }
                else
                {
                    recipeHolder.Ingredients.Add(new Ingredient() 
                    {
                        Type = ingredient.ActionType,
                        Ingredient_Tag = ingredient.AddedIngredientType.Tag.TagName
                    });
                }
            }

            foreach (ItemQuantity item in recipe.Results)
            {
                recipeHolder.Results.Add(new ItemQty
                {
                    ItemID = item.Item.ItemID,
                    Quantity = item.Quantity
                });
            }

            return recipeHolder;
        }

        public class Ingredient
        {
            public RecipeIngredient.ActionTypes Type;

            public int Ingredient_ItemID;
            public string Ingredient_Tag;
        }

        public class ItemQty
        {
            public int ItemID;
            public int Quantity;
        }
    }
}
