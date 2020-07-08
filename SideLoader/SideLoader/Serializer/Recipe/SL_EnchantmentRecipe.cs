using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_EnchantmentRecipe
    {
        public int EnchantmentID;
        public string Name;
        public string Description;

        // core requirements
        public int IncenseItemID;
        public PillarData[] PillarDatas;
        public EquipmentData CompatibleEquipment;

        // other conditions
        public Vector2[] TimeOfDay;
        public AreaManager.AreaEnum[] Areas;
        public WeatherCondition[] WeatherConditions;
        public TemperatureSteps[] Temperature;
        public bool WindAltarActivated;

        // actual enchantment data
        public float EnchantTime;
        public List<SL_EffectTransform> Effects;
        public List<AdditionalDamage> AddedDamages;
        public List<StatModification> StatModifications;
        public List<SL_Damage> FlatDamageAdded;
        public List<SL_Damage> DamageModifierBonus;
        public List<SL_Damage> DamageResistanceBonus;
        public float HealthAbsorbRatio;
        public float ManaAbsorbRatio;
        public float StaminaAbsorbRatio;
        public bool Indestructible;
        public float TrackDamageRatio;

        public void Apply()
        {
            if (SL.PacksLoaded)
            {
                ApplyTemplate();
            }
            else
            {
                SL.INTERNAL_ApplyRecipes += ApplyTemplate;
            }
        }

        private void ApplyTemplate()
        {
            SL.Log($"Applying Enchantment Recipe, ID: {this.EnchantmentID}, Name: {this.Name}");

            var recipe = ScriptableObject.CreateInstance("EnchantmentRecipe") as EnchantmentRecipe;

            recipe.RecipeID = this.EnchantmentID;
            recipe.ResultID = this.EnchantmentID;

            if (ResourcesPrefabManager.Instance.GetItemPrefab(this.IncenseItemID) is Item incense)
            {
                var list = new List<EnchantmentRecipe.PillarData>();
                foreach (var pillar in this.PillarDatas)
                {
                    list.Add(new EnchantmentRecipe.PillarData
                    {
                        Direction = (UICardinalPoint_v2.CardinalPoint)pillar.Direction,
                        IsFar = pillar.IsFar,
                        CompatibleIngredients = new EnchantmentRecipe.IngredientData[]
                        {
                            new EnchantmentRecipe.IngredientData
                            {
                                Type = EnchantmentRecipe.IngredientData.IngredientType.Specific,
                                SpecificIngredient = incense
                            }
                        }
                    });
                }
                recipe.PillarDatas = list.ToArray();
            }

            recipe.CompatibleEquipments = new EnchantmentRecipe.EquipmentData
            {
                EquipmentTag = new TagSourceSelector(CustomTags.GetTag(this.CompatibleEquipment.RequiredTag)),                
            };
            if (this.CompatibleEquipment.Equipments != null)
            {
                var equipList = new List<EnchantmentRecipe.IngredientData>();
                foreach (var ingData in this.CompatibleEquipment.Equipments)
                {
                    var data = new EnchantmentRecipe.IngredientData();
                    if (ingData.SelectorType == IngredientTypes.Tag)
                    {
                        data.Type = EnchantmentRecipe.IngredientData.IngredientType.Generic;
                        data.IngredientTag = new TagSourceSelector(CustomTags.GetTag(ingData.SelectorValue));
                    }
                    else if (ingData.SelectorType == IngredientTypes.SpecificItem)
                    {
                        data.Type = EnchantmentRecipe.IngredientData.IngredientType.Specific;
                        data.SpecificIngredient = ResourcesPrefabManager.Instance.GetItemPrefab(ingData.SelectorValue);
                    }
                    equipList.Add(data);
                }
                recipe.CompatibleEquipments.CompatibleEquipments = equipList.ToArray();
            }

            recipe.TimeOfDay = this.TimeOfDay;
            recipe.Region = this.Areas;
            recipe.WindAltarActivated = this.WindAltarActivated;
            recipe.Temperature = this.Temperature;

            if (this.WeatherConditions != null)
            {
                var list = new List<EnchantmentRecipe.WeaterCondition>();
                foreach (var condition in this.WeatherConditions)
                {
                    list.Add(new EnchantmentRecipe.WeaterCondition
                    {
                        Invert = condition.Invert,
                        Weather = (EnchantmentRecipe.WeaterType)condition.WeatherType
                    });
                }
                recipe.Weather = list.ToArray();
            }

            // ========== Create actual Enchantment effects prefab ==========

            var enchantmentObject = new GameObject(this.EnchantmentID + "_" + this.Name);
            GameObject.DontDestroyOnLoad(enchantmentObject);
            var enchantment = enchantmentObject.AddComponent<Enchantment>();

            SetLocalization(this, out enchantment.CustomDescLocKey);

            enchantment.EnchantTime = this.EnchantTime;

            if (this.Effects != null)
            {
                SL_EffectTransform.ApplyTransformList(enchantment.transform, this.Effects, EffectBehaviours.NONE);
            }

            if (this.AddedDamages != null)
            {
                var list = new List<Enchantment.AdditionalDamage>();
                foreach (var dmg in this.AddedDamages)
                {
                    list.Add(new Enchantment.AdditionalDamage
                    {
                        BonusDamageType = dmg.AddedDamageType,
                        ConversionRatio = dmg.ConversionRatio,
                        SourceDamageType = dmg.SourceDamageType
                    });
                }
                enchantment.AdditionalDamages = list.ToArray();
            }
            if (this.StatModifications != null)
            {
                var list = new Enchantment.StatModificationList();
                foreach (var mod in this.StatModifications)
                {
                    list.Add(new Enchantment.StatModification
                    {
                        Name = mod.Stat,
                        Type = mod.Type,
                        Value = mod.Value
                    });
                }
                enchantment.StatModifications = list;
            }
            if (this.FlatDamageAdded != null)
            {
                enchantment.DamageBonus = SL_Damage.GetDamageList(this.FlatDamageAdded);
            }
            if (this.DamageModifierBonus != null)
            {
                enchantment.DamageModifier = SL_Damage.GetDamageList(this.DamageModifierBonus);
            }
            if (this.DamageResistanceBonus != null)
            {
                enchantment.ElementalResistances = SL_Damage.GetDamageList(this.DamageResistanceBonus);
            }

            enchantment.HealthAbsorbRatio = this.HealthAbsorbRatio;
            enchantment.StaminaAbsorbRatio = this.StaminaAbsorbRatio;
            enchantment.ManaAbsorbRatio = this.ManaAbsorbRatio;

            enchantment.Indestructible = this.Indestructible;
            enchantment.TrackDamageRatio = this.TrackDamageRatio;

            // =========== SET DICTIONARY REFS ============
      
            // Recipe dict
            if (CustomItems.ENCHANTMENT_RECIPES.ContainsKey(this.EnchantmentID))
            {
                CustomItems.ENCHANTMENT_RECIPES[this.EnchantmentID] = recipe;
            }
            else
            {
                CustomItems.ENCHANTMENT_RECIPES.Add(this.EnchantmentID, recipe);
            }

            // Enchantment dict
            if (CustomItems.ENCHANTMENT_PREFABS.ContainsKey(this.EnchantmentID))
            {
                CustomItems.ENCHANTMENT_PREFABS[this.EnchantmentID] = enchantment;
            }
            else
            {
                CustomItems.ENCHANTMENT_PREFABS.Add(this.EnchantmentID, enchantment);
            }
        }

        public static void SetLocalization(SL_EnchantmentRecipe recipe, out string descKey)
        {
            var dict = CustomStatusEffects.GENERAL_LOCALIZATION;

            var nameKey = $"Enchantment_{recipe.EnchantmentID}";
            descKey = $"DESC_{nameKey}";

            if (dict.ContainsKey(nameKey))
            {
                dict[nameKey] = recipe.Name;
            }
            else
            {
                dict.Add(nameKey, recipe.Name);
            }

            if (dict.ContainsKey(descKey))
            {
                dict[descKey] = recipe.Description;
            }
            else
            {
                dict.Add(descKey, recipe.Description);
            }
        }

        // ======== Serializing Enchantment into a Template =========

        public static SL_EnchantmentRecipe SerializeEnchantment(EnchantmentRecipe recipe, Enchantment enchantment)
        {
            var template = new SL_EnchantmentRecipe
            {
                Name = enchantment.Name,
                Description = enchantment.Description,
                EnchantmentID = recipe.RecipeID,

                IncenseItemID = recipe.PillarDatas?[0]?.CompatibleIngredients?[0].SpecificIngredient?.ItemID ?? -1,                
                TimeOfDay = recipe.TimeOfDay,
                Areas = recipe.Region,
                Temperature = recipe.Temperature,
                WindAltarActivated = recipe.WindAltarActivated,

                EnchantTime = enchantment.EnchantTime,
                HealthAbsorbRatio = enchantment.HealthAbsorbRatio,
                StaminaAbsorbRatio = enchantment.StaminaAbsorbRatio,
                ManaAbsorbRatio = enchantment.ManaAbsorbRatio,
                Indestructible = enchantment.Indestructible,
                TrackDamageRatio = enchantment.TrackDamageRatio
            };

            if (recipe.PillarDatas != null)
            {
                var pillarList = new List<PillarData>();
                foreach (var pillarData in recipe.PillarDatas)
                {
                    var data = new PillarData
                    {
                        Direction = (Directions)pillarData.Direction,
                        IsFar = pillarData.IsFar,
                    };
                    pillarList.Add(data);
                }
                template.PillarDatas = pillarList.ToArray();
            }

            var compatibleEquipment = new EquipmentData
            {
                RequiredTag = recipe.CompatibleEquipments.EquipmentTag.Tag.TagName
            };
            if (recipe.CompatibleEquipments.CompatibleEquipments != null)
            {
                var equipList = new List<IngredientData>();
                foreach (var equipData in recipe.CompatibleEquipments.CompatibleEquipments)
                {
                    var data = new IngredientData
                    {
                        SelectorType = (IngredientTypes)equipData.Type
                    };
                    if (data.SelectorType == IngredientTypes.SpecificItem)
                    {
                        data.SelectorValue = equipData.SpecificIngredient?.ItemID.ToString();
                    }
                    else
                    {
                        data.SelectorValue = equipData.IngredientTag.Tag.TagName;
                    }
                    equipList.Add(data);
                }
                compatibleEquipment.Equipments = equipList.ToArray();
            }
            template.CompatibleEquipment = compatibleEquipment;

            // Parse the actual Enchantment effects

            if (enchantment.transform.childCount > 0)
            {
                template.Effects = new List<SL_EffectTransform>();
                foreach (Transform child in enchantment.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                    {
                        template.Effects.Add(effectsChild);
                    }
                }
            }

            if (enchantment.AdditionalDamages != null)
            {
                var list = new List<AdditionalDamage>();
                foreach (var addedDmg in enchantment.AdditionalDamages)
                {
                    list.Add(new AdditionalDamage
                    {
                        AddedDamageType = addedDmg.BonusDamageType,
                        ConversionRatio = addedDmg.ConversionRatio,
                        SourceDamageType = addedDmg.SourceDamageType
                    });
                }
                template.AddedDamages = list;
            }
            if (enchantment.StatModifications != null)
            {
                var list = new List<StatModification>();
                foreach (var statMod in enchantment.StatModifications)
                {
                    list.Add(new StatModification
                    {
                        Stat = statMod.Name,
                        Type = statMod.Type,
                        Value = statMod.Value
                    });
                }
                template.StatModifications = list;
            }

            if (enchantment.DamageBonus != null)
            {
                template.FlatDamageAdded = SL_Damage.ParseDamageList(enchantment.DamageBonus);
            }
            if (enchantment.DamageModifier != null)
            {
                template.DamageModifierBonus = SL_Damage.ParseDamageList(enchantment.DamageModifier);
            }
            if (enchantment.ElementalResistances != null)
            {
                template.DamageResistanceBonus = SL_Damage.ParseDamageList(enchantment.ElementalResistances);
            }

            return template;
        }

        // ====== EnchantmentRecipe sub-classes ======
        public enum IngredientTypes
        {
            Tag,
            SpecificItem,
        }

        public struct IngredientData
        {
            public IngredientTypes SelectorType;
            public string SelectorValue;
        }

        public struct EquipmentData
        {
            public IngredientData[] Equipments;
            public string RequiredTag;
        }

        public enum Directions
        {
            North,
            South,
            East,
            West
        }

        public struct PillarData
        {
            public Directions Direction;
            public bool IsFar;
        }

        public enum WeatherTypes
        {
            Clear,
            Rain,
            Snow,
            SeasonEffect,
        }

        public struct WeatherCondition
        {
            public WeatherTypes WeatherType;
            public bool Invert;
        }

        // ====== Enchantment sub-classes ======
        public struct AdditionalDamage
        {
            public DamageType.Types AddedDamageType;
            public DamageType.Types SourceDamageType;
            public float ConversionRatio;
        }

        public struct StatModification
        {
            public Enchantment.Stat Stat;
            public Enchantment.StatModification.BonusType Type;
            public float Value;
        }
    }
}
