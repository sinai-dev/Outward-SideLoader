using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace SideLoader_2
{
    public class RecipeHolder
    {
        public string StationType;
        public string Name;
        public int RecipeID;

        public List<Ingredient> Ingredients = new List<Ingredient>();
        public List<ItemQty> Results = new List<ItemQty>();

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
