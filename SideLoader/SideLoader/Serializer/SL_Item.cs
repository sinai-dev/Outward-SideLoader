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

        /// <summary> Default = false. 
        /// If you only want to change the visuals (and not even change the item ID), you can use this so you dont have to set every other field. </summary>
        public bool OnlyChangeVisuals = false;

        /// <summary>Default = true. 
        /// Will destroy all child GameObjects on the Item prefab (ie, destroys all existing Effects)</summary>
        public bool ReplaceEffects = true;

        /// <summary>The Item ID of the Item you are cloning FROM</summary>
        public int Target_ItemID;
        /// <summary>The NEW Item ID for your custom Item (can be the same as target, will overwrite)</summary>
        public int New_ItemID;

        /*************                   Actual Item values                  *************/

        public string Name;
        public string Description;

        /// <summary>The Item ID of the Legacy Item (the upgrade of this item when placed in a Legacy Chest)</summary>
        public int LegacyItemID = -1;

        /// <summary>Can the item be picked up?</summary>
        public bool IsPickable = true;
        /// <summary>Can you "Use" the item? ("Use" option from menu)</summary>
        public bool IsUsable = false;
        public int QtyRemovedOnUse = 1;
        public bool GroupItemInDisplay = false;
        public bool HasPhysicsWhenWorld = false;
        public bool RepairedInRest = true;
        public Item.BehaviorOnNoDurabilityType BehaviorOnNoDurability = Item.BehaviorOnNoDurabilityType.NotSet;

        public Character.SpellCastType CastType;
        public Character.SpellCastModifier CastModifier;
        public bool CastLocomotionEnabled;
        public float MobileCastMovementMult = -1f;
        public int CastSheatheRequired;

        public List<string> Tags = new List<string>();

        public SL_ItemStats StatsHolder;

        public List<SL_EffectTransform> EffectTransforms = new List<SL_EffectTransform>();

        /// <summary>[Optional] The name of the AssetBundle where your custom ItemVisuals are loaded from</summary>
        public string AssetBundleName = "";
        /// <summary>[Optional] The name of the GameObject for your standard Item Visuals, in the AssetBundle defined as VisualsBundleName</summary>
        public string ItemVisuals_PrefabName = "";
        public Vector3 ItemVisuals_PositionOffset = Vector3.zero;
        public Vector3 ItemVisuals_RotationOffset = Vector3.zero;
        /// <summary>[Optional] The name of the GameObject for your SPECIAL Visuals (commonly for Armor visuals), in the AssetBundle defined as VisualsBundleName</summary>
        public string SpecialVisuals_PrefabName = "";
        public Vector3 SpecialVisuals_PositionOffset = Vector3.zero;
        public Vector3 SpecialVisuals_RotationOffset = Vector3.zero;
        /// <summary>[Optional] The name of the GameObject for your SPECIAL FEMALE Visuals (commonly for Armor visuals), in the AssetBundle defined as VisualsBundleName</summary>
        public string FemaleVisuals_PrefabName = "";
        public Vector3 FemaleVisuals_PositionOffset = Vector3.zero;
        public Vector3 FemaleVisuals_RotationOffset = Vector3.zero;

        public bool VisualsHideFace = false;
        public bool VisualsHideHair = false;

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

            // if "Only Change Visuals" is NOT true, then apply all other changes
            if (!OnlyChangeVisuals)
            {
                CustomItems.SetNameAndDescription(item, Name, Description);

                item.LegacyItemID = this.LegacyItemID;
                item.CastLocomotionEnabled = this.CastLocomotionEnabled;
                item.CastModifier = this.CastModifier;
                item.CastSheathRequired = this.CastSheatheRequired;
                item.GroupItemInDisplay = this.GroupItemInDisplay;
                item.HasPhysicsWhenWorld = this.HasPhysicsWhenWorld;
                item.IsPickable = this.IsPickable;
                item.IsUsable = this.IsUsable;
                item.QtyRemovedOnUse = this.QtyRemovedOnUse;
                item.MobileCastMovementMult = this.MobileCastMovementMult;
                item.RepairedInRest = this.RepairedInRest;
                item.BehaviorOnNoDurability = this.BehaviorOnNoDurability;
                item.CastModifier = this.CastModifier;
                At.SetValue(this.CastType, typeof(Item), item, "m_activateEffectAnimType");

                if (this.Tags != null)
                {
                    CustomItems.SetItemTags(item, this.Tags, true);
                }

                if (this.StatsHolder != null)
                {
                    StatsHolder.ApplyToItem(item.Stats ?? item.GetComponent<ItemStats>());
                }

                if (this.EffectTransforms != null && this.EffectTransforms.Count > 0)
                {
                    foreach (var transform in this.EffectTransforms)
                    {                        
                        transform.ApplyToItem(item, true);
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

            } // End "OnlyChangeVisuals == false" section


            //************************  This will need to change after DLC.  ************************//

            ApplyVisuals(pack, item);

            // **************************************************************************************//


            SL.Log("Finished applying template");
        }

        private void ApplyVisuals(SLPack pack, Item item)
        {
            bool customVisualPrefab = false;
            bool customSpecialPrefab = false;
            bool customFemalePrefab = false;

            if (pack != null && !string.IsNullOrEmpty(AssetBundleName))
            {
                if (!pack.AssetBundles.ContainsKey(this.AssetBundleName))
                {
                    SL.Log("Error: The SLPack for this item does not contain an assetbundle by the name of " + this.AssetBundleName, 1);
                }
                else
                {
                    var bundle = pack.AssetBundles[this.AssetBundleName];

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

                        if (string.IsNullOrEmpty(prefabName))
                        {
                            continue;
                        }

                        var prefab = bundle.LoadAsset<GameObject>(prefabName);

                        if (!prefab || !orig)
                        {
                            SL.Log("Error: Either we could not find a custom prefab by that name, or the original item does not use visuals for the given type", 1);
                            return;
                        }

                        CustomItemVisuals.SetVisualPrefab(item, orig, prefab.transform, type, pos, rot, this.VisualsHideFace, this.VisualsHideHair);
                    }
                }
            }

            if (item.VisualPrefab != null && !customVisualPrefab)
            {
                CustomItemVisuals.CloneVisualPrefab(item, CustomItemVisuals.VisualPrefabType.VisualPrefab);
            }

            if (item.SpecialVisualPrefabDefault != null)
            {
                if (!customSpecialPrefab)
                    CustomItemVisuals.CloneVisualPrefab(item, CustomItemVisuals.VisualPrefabType.SpecialVisualPrefabDefault);

                if (item.SpecialVisualPrefabDefault.GetComponent<ArmorVisuals>() is ArmorVisuals armorVisuals)
                {
                    armorVisuals.HideFace = this.VisualsHideFace;
                    armorVisuals.HideHair = this.VisualsHideHair;
                }
            }

            if (item.SpecialVisualPrefabFemale != null)
            {
                if (!customFemalePrefab)
                    CustomItemVisuals.CloneVisualPrefab(item, CustomItemVisuals.VisualPrefabType.SpecialVisualPrefabFemale);

                if (item.SpecialVisualPrefabFemale.GetComponent<ArmorVisuals>() is ArmorVisuals armorVisuals)
                {
                    armorVisuals.HideFace = this.VisualsHideFace;
                    armorVisuals.HideHair = this.VisualsHideHair;
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
                BehaviorOnNoDurability      = item.BehaviorOnNoDurability,
                CastType                    = item.ActivateEffectAnimType                
            };

            if (item.Stats != null)
            {
                itemHolder.StatsHolder = SL_ItemStats.ParseItemStats(item.Stats);
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
            else
            {
                return itemHolder;
            }
        }
    }
}
