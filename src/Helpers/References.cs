using Localizer;
using SideLoader.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SideLoader
{
    /// <summary>Helpers to access useful dictionaries, lists and instances maintained by the game.</summary>
    public class References
    {
        // =================== LOCALIZATION ===================

        /// <summary>Cached LocalizationManager.m_generalLocalization reference.</summary>
        public static Dictionary<string, string> GENERAL_LOCALIZATION
        {
            get
            {
                if (s_generalLocalization == null)
                {
                    try
                    {
                        s_generalLocalization = (Dictionary<string, string>)At.GetField(LocalizationManager.Instance, "m_generalLocalization");
                    }
                    catch { }
                }
                return s_generalLocalization;
            }
        }
        private static Dictionary<string, string> s_generalLocalization;

        /// <summary>Cached LocalizationManager.m_itemLocalization reference</summary>
        public static Dictionary<int, ItemLocalization> ITEM_LOCALIZATION
        {
            get
            {
                if (s_itemLocalization == null)
                {
                    try
                    {
                        s_itemLocalization = At.GetField(LocalizationManager.Instance, "m_itemLocalization") as Dictionary<int, ItemLocalization>;
                    }
                    catch { }
                }
                return s_itemLocalization;
            }
        }
        private static Dictionary<int, ItemLocalization> s_itemLocalization;

        public static Dictionary<int, int> LOCALIZATION_DUPLICATE_NAME
        {
            get
            {
                if (s_duplicateName == null)
                    s_duplicateName = typeof(ItemLocalization).GetField<Dictionary<int, int>>("DUPLICATE_NAME");

                return s_duplicateName;
            }
        }
        private static Dictionary<int, int> s_duplicateName;

        public static Dictionary<int, int> LOCALIZATION_DUPLICATE_DESC
        {
            get
            {
                if (s_duplicateDesc == null)
                    s_duplicateDesc = typeof(ItemLocalization).GetField<Dictionary<int, int>>("DUPLICATE_DESCRIPTION");

                return s_duplicateDesc;
            }
        }
        private static Dictionary<int, int> s_duplicateDesc;

        // ============= RESOURCES PREFAB MANAGER =============

        /// <summary>Cached ResourcesPrefabManager.ITEM_PREFABS Dictionary</summary>
        public static Dictionary<string, Item> RPM_ITEM_PREFABS
        {
            get
            {
                if (s_itemPrefabs == null)
                    s_itemPrefabs = At.GetField<ResourcesPrefabManager>("ITEM_PREFABS") as Dictionary<string, Item>;

                return s_itemPrefabs;
            }
        }
        private static Dictionary<string, Item> s_itemPrefabs;

        /// <summary>Cached ResourcesPrefabManager.EFFECTPRESET_PREFABS reference.</summary>
        public static Dictionary<int, EffectPreset> RPM_EFFECT_PRESETS
        {
            get
            {
                if (s_effectPresets == null)
                    s_effectPresets = (Dictionary<int, EffectPreset>)At.GetField<ResourcesPrefabManager>("EFFECTPRESET_PREFABS");

                return s_effectPresets;
            }
        }
        private static Dictionary<int, EffectPreset> s_effectPresets;

        /// <summary>Cached ResourcesPrefabManager.STATUSEFFECT_PREFABS reference.</summary>
        public static Dictionary<string, StatusEffect> RPM_STATUS_EFFECTS
        {
            get
            {
                if (m_statusEffects == null)
                    m_statusEffects = (Dictionary<string, StatusEffect>)At.GetField<ResourcesPrefabManager>("STATUSEFFECT_PREFABS");

                return m_statusEffects;
            }
        }
        private static Dictionary<string, StatusEffect> m_statusEffects;

        /// <summary>Cached ResourcesPrefabManager.ENCHANTMENT_PREFABS reference.</summary>
        public static Dictionary<int, Enchantment> ENCHANTMENT_PREFABS
        {
            get
            {
                if (m_enchantmentPrefabs == null)
                    m_enchantmentPrefabs = At.GetField<ResourcesPrefabManager>("ENCHANTMENT_PREFABS") as Dictionary<int, Enchantment>;

                return m_enchantmentPrefabs;
            }
        }
        private static Dictionary<int, Enchantment> m_enchantmentPrefabs;

        // =================== RECIPE MANAGER ===================

        /// <summary>Cached RecipeManager.m_recipes Dictionary</summary>
        public static Dictionary<string, Recipe> ALL_RECIPES
        {
            get
            {
                if (m_recipes == null)
                    m_recipes = At.GetField(RecipeManager.Instance, "m_recipes") as Dictionary<string, Recipe>;

                return m_recipes;
            }
        }
        private static Dictionary<string, Recipe> m_recipes;

        /// <summary>Cached RecipeManager.m_recipeUIDsPerUstensils Dictionary</summary>
        public static Dictionary<Recipe.CraftingType, List<UID>> RECIPES_PER_UTENSIL
        {
            get
            {
                if (m_recipesPerUtensil == null)
                    m_recipesPerUtensil = At.GetField(RecipeManager.Instance, "m_recipeUIDsPerUstensils") as Dictionary<Recipe.CraftingType, List<UID>>;

                return m_recipesPerUtensil;
            }
        }
        private static Dictionary<Recipe.CraftingType, List<UID>> m_recipesPerUtensil;

        /// <summary>Cached RecipeManager.m_enchantmentRecipes reference.</summary>
        public static Dictionary<int, EnchantmentRecipe> ENCHANTMENT_RECIPES
        {
            get
            {
                if (m_enchantmentRecipes == null)
                    m_enchantmentRecipes = At.GetField(RecipeManager.Instance, "m_enchantmentRecipes") as Dictionary<int, EnchantmentRecipe>;

                return m_enchantmentRecipes;
            }
        }
        private static Dictionary<int, EnchantmentRecipe> m_enchantmentRecipes;

        // ============= OTHER =========== 

        public static GlobalAudioManager GLOBALAUDIOMANAGER
        {
            get
            {
                if (!m_GlobalAudioManager)
                {
                    var list = Resources.FindObjectsOfTypeAll<GlobalAudioManager>();
                    if (list != null && list.Length > 0 && list[0])
                        m_GlobalAudioManager = list[0];
                    else
                        SL.LogWarning("Cannot find GlobalAudioManager Instance!");
                }
                return m_GlobalAudioManager;
            }
        }

        private static GlobalAudioManager m_GlobalAudioManager;
    }
}
