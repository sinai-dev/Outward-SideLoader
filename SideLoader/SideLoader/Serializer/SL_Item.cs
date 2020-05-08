using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace SideLoader
{
    public class SL_Item
    {
        /*************           Settings (not true Item values)          *************/

        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom item template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore]
        public string SLPackName;
        /// <summary> [NOT SERIALIZED] The name of the folder this custom item is using for textures (MyPack/Items/[SubfolderName]/Textures/).</summary>
        [XmlIgnore]
        public string SubfolderName;

        /// <summary><list type="bullet">
        /// <item>NONE (default): Your effects are added on top of the existing ones.</item>
        /// <item>DestroyEffects: Destroys all child GameObjects on your item, except for "Content" (used for Bags)</item>
        /// <item>OverrideEffects: Only destroys child GameObjects if you have defined one of the same name.</item></list>
        /// </summary>
        public TemplateBehaviour EffectBehaviour = TemplateBehaviour.NONE;

        /// <summary>The Item ID of the Item you are cloning FROM</summary>
        public int Target_ItemID = -1;
        /// <summary>The NEW Item ID for your custom Item (can be the same as target, will overwrite)</summary>
        public int New_ItemID = -1;

        /*************                   Actual Item values                  *************/
        public string Name = null;
        public string Description = null;

        /// <summary>The Item ID of the Legacy Item (the upgrade of this item when placed in a Legacy Chest)</summary>
        public int? LegacyItemID;

        /// <summary>Can the item be picked up?</summary>
        public bool? IsPickable;
        /// <summary>Can you "Use" the item? ("Use" option from menu)</summary>
        public bool? IsUsable;
        public int? QtyRemovedOnUse;
        public bool? GroupItemInDisplay;
        public bool? HasPhysicsWhenWorld;
        public bool? RepairedInRest;
        public Item.BehaviorOnNoDurabilityType? BehaviorOnNoDurability;

        public Character.SpellCastType? CastType;
        public Character.SpellCastModifier? CastModifier;
        public bool? CastLocomotionEnabled;
        public float? MobileCastMovementMult;
        public int? CastSheatheRequired;

        public List<string> Tags = new List<string>();

        public SL_ItemStats StatsHolder;

        public List<SL_EffectTransform> EffectTransforms = new List<SL_EffectTransform>();

        /*       Visual Prefab stuff       */

        /// <summary>The name of the AssetBundle where your custom ItemVisuals are loaded from</summary>
        public string AssetBundleName = "";
        /// <summary>The name of the GameObject for your standard Item Visuals, in the AssetBundle defined as AssetBundleName</summary>
        public string ItemVisuals_PrefabName = "";
        public Vector3 ItemVisuals_PositionOffset = Vector3.zero;
        public Vector3 ItemVisuals_RotationOffset = Vector3.zero;
        /// <summary>The name of the GameObject for your SPECIAL Visuals (commonly for Armor visuals), in the AssetBundle defined as AssetBundleName</summary>
        public string SpecialVisuals_PrefabName = "";
        public Vector3 SpecialVisuals_PositionOffset = Vector3.zero;
        public Vector3 SpecialVisuals_RotationOffset = Vector3.zero;
        /// <summary>The name of the GameObject for your SPECIAL FEMALE Visuals (commonly for Armor visuals), in the AssetBundle defined as AssetBundleName</summary>
        public string FemaleVisuals_PrefabName = "";
        public Vector3 FemaleVisuals_PositionOffset = Vector3.zero;
        public Vector3 FemaleVisuals_RotationOffset = Vector3.zero;

        public bool? VisualsHideFace;
        public bool? VisualsHideHair;

        public void ApplyTemplateToItem()
        {
            var item = ResourcesPrefabManager.Instance.GetItemPrefab(New_ItemID);
            if (!item)
            {
                SL.Log("Could not find an item with this New_ItemID! Maybe you are trying to apply before calling CustomItems.CreateCustomItem?", 1);
                return;
            }

            SL.Log("Applying Item Template. ID: " + New_ItemID + ", Name: " + (Name ?? item.Name));

            SLPack pack = null;
            if (!string.IsNullOrEmpty(SLPackName) && SL.Packs.ContainsKey(SLPackName))
            {
                pack = SL.Packs[SLPackName];
            }

            // obsolete checks
            #region Obsolete Checks
            if (this.Behaviour != TemplateBehaviour.NONE)
            {
                SL.Log("SL_Item.Behaviour is obsolete, use SL_Item.EffectBehaviour instead!");
                this.EffectBehaviour = this.Behaviour;
            }
            if (EffectBehaviour == TemplateBehaviour.OnlyChangeVisuals)
            {
                EffectBehaviour = TemplateBehaviour.NONE;
                SL.Log("TemplateBehaviour.OnlyChangeVisuals is deprecated, use \"NONE\" instead, and remove other fields from your template.");
            }
            if (EffectBehaviour == TemplateBehaviour.NONE)
            {
                if (OnlyChangeVisuals)
                {
                    SL.Log("SL_Item.OnlyChangeVisuals is deprecated - just exclude fields from the template if you don't want to change them.");
                    EffectBehaviour = TemplateBehaviour.NONE;
                }
                else if (ReplaceEffects)
                {
                    SL.Log("SL_Item.ReplaceEffects is deprecated - use SL_Item.EffectBehaviour instead!");
                    EffectBehaviour = TemplateBehaviour.DestroyEffects;
                }
            }
            #endregion

            CustomItems.SetNameAndDescription(item, this.Name ?? item.Name, this.Description ?? item.Description);

            if (this.LegacyItemID != null)
                item.LegacyItemID = (int)this.LegacyItemID;

            if (this.CastLocomotionEnabled != null)
                item.CastLocomotionEnabled = (bool)this.CastLocomotionEnabled;

            if (this.CastModifier != null)
                item.CastModifier = (Character.SpellCastModifier)this.CastModifier;

            if (this.CastSheatheRequired != null)
                item.CastSheathRequired = (int)this.CastSheatheRequired;

            if (this.GroupItemInDisplay != null)
                item.GroupItemInDisplay = (bool)this.GroupItemInDisplay;

            if (this.HasPhysicsWhenWorld != null)
                item.HasPhysicsWhenWorld = (bool)this.HasPhysicsWhenWorld;

            if (this.IsPickable != null)
                item.IsPickable = (bool)this.IsPickable;

            if (this.IsUsable != null)
                item.IsUsable = (bool)this.IsUsable;

            if (this.QtyRemovedOnUse != null)
                item.QtyRemovedOnUse = (int)this.QtyRemovedOnUse;

            if (this.MobileCastMovementMult != null)
                item.MobileCastMovementMult = (float)this.MobileCastMovementMult;

            if (this.RepairedInRest != null)
                item.RepairedInRest = (bool)this.RepairedInRest;

            if (this.BehaviorOnNoDurability != null)
                item.BehaviorOnNoDurability = (Item.BehaviorOnNoDurabilityType)this.BehaviorOnNoDurability;

            if (this.CastModifier != null)
                item.CastModifier = (Character.SpellCastModifier)this.CastModifier;

            if (this.CastType != null)
                At.SetValue((Character.SpellCastType)this.CastType, typeof(Item), item, "m_activateEffectAnimType");

            if (this.Tags != null)
            {
                CustomItems.SetItemTags(item, this.Tags, true);
            }

            if (this.StatsHolder != null)
            {
                StatsHolder.ApplyToItem(item.Stats ?? item.transform.GetOrAddComponent<ItemStats>());
            }

            if (EffectBehaviour == TemplateBehaviour.DestroyEffects)
            {
                Debug.Log("Replacing effects (destroying children)");
                CustomItems.DestroyChildren(item.transform);
            }

            if (this.EffectTransforms != null && this.EffectTransforms.Count > 0)
            {
                foreach (var effectsT in this.EffectTransforms)
                {
                    if (EffectBehaviour == TemplateBehaviour.OverrideEffects && item.transform.Find(effectsT.TransformName) is Transform existing)
                    {
                        Debug.Log("Overriding transform " + existing.name + " (destroying orig)");
                        UnityEngine.Object.DestroyImmediate(existing.gameObject);
                    }

                    effectsT.ApplyToItem(item);
                }
            }

            if (this is SL_Equipment equipmentHolder)
            {
                equipmentHolder.ApplyToItem(item as Equipment);
            }
            else if (this is SL_Skill skillHolder)
            {
                skillHolder.ApplyToItem(item as Skill);
            }
            else if (this is SL_RecipeItem recipeItem)
            {
                recipeItem.ApplyToItem(item as RecipeItem);
            }

            //************************  This will need to change after DLC.  ************************//

            ApplyVisuals(pack, item);

            // **************************************************************************************//

            if (item is Weapon weapon && weapon.GetComponent<WeaponStats>() is WeaponStats stats)
            {
                At.SetValue(stats.BaseDamage.Clone(), typeof(Weapon), weapon, "m_baseDamage");
                At.SetValue(stats.BaseDamage.Clone(), typeof(Weapon), weapon, "m_activeBaseDamage");
            }
        }

        private void ApplyVisuals(SLPack pack, Item item)
        {
            AssetBundle bundle = null;
            if (pack != null && !string.IsNullOrEmpty(AssetBundleName))
            {
                if (!pack.AssetBundles.ContainsKey(this.AssetBundleName))
                {
                    SL.Log("Error: The SLPack for this item does not contain an assetbundle by the name of " + this.AssetBundleName, 1);
                }
                else
                {
                    bundle = pack.AssetBundles[this.AssetBundleName];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                Transform orig = null;
                string prefabName = "";
                Vector3 pos = Vector3.zero;
                Vector3 rot = Vector3.zero;

                var type = (CustomItemVisuals.VisualPrefabType)i;
                switch (type)
                {
                    case CustomItemVisuals.VisualPrefabType.VisualPrefab:
                        prefabName = this.ItemVisuals_PrefabName;
                        orig = item.VisualPrefab;
                        pos = this.ItemVisuals_PositionOffset;
                        rot = this.ItemVisuals_RotationOffset;
                        break;
                    case CustomItemVisuals.VisualPrefabType.SpecialVisualPrefabDefault:
                        prefabName = this.SpecialVisuals_PrefabName;
                        orig = item.SpecialVisualPrefabDefault;
                        pos = this.SpecialVisuals_PositionOffset;
                        rot = this.SpecialVisuals_RotationOffset;
                        break;
                    case CustomItemVisuals.VisualPrefabType.SpecialVisualPrefabFemale:
                        prefabName = this.FemaleVisuals_PrefabName;
                        orig = item.SpecialVisualPrefabFemale;
                        pos = this.FemaleVisuals_PositionOffset;
                        rot = this.FemaleVisuals_RotationOffset;
                        break;
                }

                if (!orig)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(prefabName))
                {
                    var newVisuals = CustomItemVisuals.CloneVisualPrefab(item, type, pos, rot);

                    if (newVisuals.GetComponent<ArmorVisuals>() is ArmorVisuals armorVisuals)
                    {
                        if (this.VisualsHideFace != null)
                        {
                            armorVisuals.HideFace = (bool)this.VisualsHideFace;
                        }
                        if (this.VisualsHideHair != null)
                        {
                            armorVisuals.HideHair = (bool)this.VisualsHideHair;
                        }
                    }

                    continue;
                }
                else if (bundle != null)
                {
                    var prefab = bundle.LoadAsset<GameObject>(prefabName);

                    if (!prefab)
                    {
                        SL.Log("Error: Either we could not find a custom prefab by that name, or the original item does not use visuals for the given type", 1);
                        continue;
                    }

                    CustomItemVisuals.SetVisualPrefab(item, prefab.transform, type, pos, rot, this.VisualsHideFace, this.VisualsHideHair);
                }
            }

            // Texture Replacements
            if (pack != null && !string.IsNullOrEmpty(SubfolderName))
            {
                CustomItemVisuals.TryApplyCustomTextures(this, item);
            }
        }

        // ***********************  FOR SERIALIZING AN ITEM INTO A TEMPLATE  *********************** //

        public static SL_Item ParseItemToTemplate(Item item)
        {
            SL.Log("Parsing item to template: " + item.Name);

            var itemHolder = new SL_Item
            {
                Name                        = item.Name,
                Description                 = item.Description,
                Target_ItemID               = item.ItemID,
                LegacyItemID                = item.LegacyItemID,
                CastLocomotionEnabled       = item.CastLocomotionEnabled,
                CastModifier                = item.CastModifier,
                CastSheatheRequired         = item.CastSheathRequired,
                GroupItemInDisplay          = item.GroupItemInDisplay,
                HasPhysicsWhenWorld         = item.HasPhysicsWhenWorld,
                IsPickable                  = item.IsPickable,
                IsUsable                    = item.IsUsable,
                QtyRemovedOnUse             = item.QtyRemovedOnUse,
                MobileCastMovementMult      = item.MobileCastMovementMult,
                RepairedInRest              = item.RepairedInRest,
                BehaviorOnNoDurability      = item.BehaviorOnNoDurability                
            };

            itemHolder.CastType = (Character.SpellCastType)At.GetValue(typeof(Item), item, "m_activateEffectAnimType");

            if (item.GetComponent<ItemStats>() is ItemStats stats)
            {
                itemHolder.StatsHolder = SL_ItemStats.ParseItemStats(stats);
            }

            if (item.Tags != null)
            {
                foreach (Tag tag in item.Tags)
                {
                    itemHolder.Tags.Add(tag.TagName);
                }
            }

            foreach (Transform child in item.transform)
            {
                var effectsChild = SL_EffectTransform.ParseTransform(child);

                if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0) // || effectsChild.EffectConditions.Count > 0)
                {
                    itemHolder.EffectTransforms.Add(effectsChild);
                }
            }

            // Sub-Templates //
            if (item is Equipment)
            {
                return SL_Equipment.ParseEquipment(item as Equipment, itemHolder);
            }
            else if (item is Skill)
            {
                return SL_Skill.ParseSkill(item as Skill, itemHolder);
            }
            else if (item is RecipeItem)
            {
                return SL_RecipeItem.ParseRecipeItem(item as RecipeItem, itemHolder);
            }
            else
            {
                return itemHolder;
            }
        }

        public enum TemplateBehaviour
        {
            NONE,
            DestroyEffects,
            OverrideEffects,
            [Obsolete("Just exclude all other fields from the template if you don't want to change them.")] 
            OnlyChangeVisuals,
        }

        // ******************* Legacy support *******************

        [Obsolete("Use SL_Item.Behaviour instead.", false)]
        public bool OnlyChangeVisuals = false;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool ShouldSerializeOnlyChangeVisuals() { return false; }

        [Obsolete("Use SL_Item.Behaviour instead.", false)]
        public bool ReplaceEffects = false;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool ShouldSerializeReplaceEffects() { return false; }

        [Obsolete("Use Template.EffectBehaviour instead.")]
        public TemplateBehaviour Behaviour = TemplateBehaviour.NONE;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool ShouldSerializeBehaviour() { return false; }
    }
}
