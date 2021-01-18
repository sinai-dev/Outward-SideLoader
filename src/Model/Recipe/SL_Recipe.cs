using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using SideLoader.Helpers;
using SideLoader.Model;

namespace SideLoader
{
    public class SL_Recipe : IContentTemplate<string>
    {
        [XmlIgnore] public string DefaultTemplateName => "UntitledRecipe";
        [XmlIgnore] public bool IsCreatingNewID => true;
        [XmlIgnore] public bool DoesTargetExist => true;
        [XmlIgnore] public string TargetID => this.UID;
        [XmlIgnore] public string AppliedID => this.UID;
        [XmlIgnore] public SLPack.SubFolders SLPackSubfolder => SLPack.SubFolders.Recipes;
        [XmlIgnore] public bool TemplateAllowedInSubfolder => false;

        [XmlIgnore] public bool CanParseContent => true;
        public IContentTemplate ParseToTemplate(object content) => ParseRecipe(content as Recipe);
        public object GetContentFromID(object id)
        {
            References.ALL_RECIPES.TryGetValue((string)id, out Recipe ret);
            return ret;
        }

        [XmlIgnore] public string SerializedSLPackName 
        {
            get => SLPackName; 
            set => SLPackName = value;
        }
        [XmlIgnore] public string SerializedSubfolderName
        {
            get => null;
            set { }
        }
        [XmlIgnore] public string SerializedFilename 
        {
            get => m_serializedFilename; 
            set => m_serializedFilename = value;
        }
        public void CreateContent() => this.ApplyRecipe();

        internal string SLPackName;
        internal string m_serializedFilename;

        [XmlIgnore]
        private bool m_applied = false;

        public string UID = "";

        // public string Name = "";
        public Recipe.CraftingType StationType = Recipe.CraftingType.Survival;

        public List<Ingredient> Ingredients = new List<Ingredient>();
        public List<ItemQty> Results = new List<ItemQty>();

        public void ApplyRecipe()
        {
            try
            {
                SL.Log("Defining recipe UID: " + this.UID);

                if (string.IsNullOrEmpty(this.UID))
                {
                    SL.LogWarning("No UID was set! Please set a UID, for example 'myname.myrecipe'. Aborting.");
                    return;
                }

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
                    if (ingredient.Type == RecipeIngredient.ActionTypes.AddGenericIngredient)
                    {
                        var tag = CustomTags.GetTag(ingredient.Ingredient_Tag);
                        if (tag == Tag.None)
                        {
                            SL.LogWarning("Could not get a tag by the name of '" + ingredient.Ingredient_Tag);
                            return;
                        }

                        ingredients.Add(new RecipeIngredient
                        {
                            ActionType = ingredient.Type,
                            AddedIngredientType = new TagSourceSelector(tag)
                        });
                    }
                    else
                    {
                        if (ingredient.Ingredient_ItemID == 0)
                        {
                            SL.LogWarning("Picking an Ingredient based on Item ID, but no ID was set. Check your XML and make sure there are no logical errors. Aborting");
                            return;
                        }

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
                            //SL.Log($"Adding tag {tag.TagName} to " + ingredientItem.name);

                            if (!ingredientItem.GetComponent<TagSource>())
                                ingredientItem.gameObject.AddComponent<TagSource>();

                            ((List<TagSourceSelector>)At.GetField<TagListSelectorComponent>(ingredientItem.GetComponent<TagSource>(), "m_tagSelectors"))
                                .Add(new TagSourceSelector(tag));
                        }

                        ingredients.Add(new RecipeIngredient()
                        {
                            ActionType = RecipeIngredient.ActionTypes.AddSpecificIngredient,
                            AddedIngredient = ingredientItem,
                        });
                    }
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
                    At.SetField(recipe, "m_uid", new UID(this.UID));
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

                m_applied = true;
            }
            catch (Exception e)
            {
                SL.LogWarning("Error applying recipe!");
                SL.LogInnerException(e);
            }
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
