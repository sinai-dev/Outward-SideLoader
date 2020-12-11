using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using SideLoader.Helpers;

namespace SideLoader
{
    [SL_Serialized]
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
                SL.Log("Trying to apply an SL_Recipe that is already applied! This is not allowed.");
                return;
            }

            var results = new List<ItemReferenceQuantity>();
            foreach (var result in this.Results)
            {
                var resultItem = ResourcesPrefabManager.Instance.GetItemPrefab(result.ItemID);
                if (!resultItem)
                {
                    SL.Log("Error: Could not get recipe result id : " + result.ItemID);
                    return;
                }
                results.Add(new ItemReferenceQuantity(resultItem, result.Quantity));
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

                // The item needs the station type tag in order to be used in a manual recipe on that station
                var tag = TagSourceManager.GetCraftingIngredient(StationType);
                if (!ingredientItem.HasTag(tag))
                {
                    //Debug.Log($"Adding tag {tag.TagName} to " + ingredientItem.name);

                    ((List<TagSourceSelector>)At.GetField<TagListSelectorComponent>(ingredientItem.GetComponent<TagSource>(), "m_tagSelectors"))
                        .Add(new TagSourceSelector(tag));
                }

                ingredients.Add(new RecipeIngredient()
                {
                    ActionType = ingredient.Type,
                    AddedIngredient = ingredientItem,
                    AddedIngredientType = ingredient.Ingredient_Tag == null ? null : new TagSourceSelector(CustomItems.GetTag(ingredient.Ingredient_Tag))
                });
            }

            var recipe = ScriptableObject.CreateInstance<Recipe>();

            recipe.SetCraftingType(this.StationType);

            At.SetField(recipe, "m_results", results.ToArray());
            recipe.SetRecipeIngredients(ingredients.ToArray());            

            // set or generate UID
            if (string.IsNullOrEmpty(this.UID))
            {
                var uid = $"{recipe.Results[0].ItemID}{recipe.Results[0].Quantity}";
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
                this.UID = uid;
                At.SetField(recipe, "m_uid", new UID(uid));
            }
            else
            {
                At.SetField(recipe, "m_uid",new UID(this.UID));
            }

            recipe.Init();

            // fix Recipe Manager dictionaries to contain our recipe
            var dict = References.ALL_RECIPES;
            var dict2 = References.RECIPES_PER_UTENSIL;

            if (dict.ContainsKey(recipe.UID))
            {
                dict[recipe.UID] = recipe;
            }
            else
            {
                dict.Add(recipe.UID, recipe);
            }

            if (!dict2.ContainsKey(recipe.CraftingStationType))
            {
                dict2.Add(recipe.CraftingStationType, new List<UID>());
            }
            
            if (!dict2[recipe.CraftingStationType].Contains(recipe.UID))
            {
                dict2[recipe.CraftingStationType].Add(recipe.UID);
            }

            SL.Log("Defined recipe '" + recipe.Name + "', UID: " + recipe.UID);
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

            foreach (ItemReferenceQuantity item in recipe.Results)
            {
                recipeHolder.Results.Add(new ItemQty
                {
                    ItemID = item.ItemID,
                    Quantity = item.Quantity
                });
            }

            return recipeHolder;
        }

        [SL_Serialized]
        public class Ingredient
        {
            public RecipeIngredient.ActionTypes Type;

            public int Ingredient_ItemID;
            public string Ingredient_Tag;
        }

        [SL_Serialized]
        public class ItemQty
        {
            public int ItemID;
            public int Quantity;
        }
    }
}
