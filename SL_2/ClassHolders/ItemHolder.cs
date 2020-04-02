using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace SideLoader_2
{
    public class ItemHolder
    {
        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom item template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore]
        public string SLPackName;
        /// <summary> [NOT SERIALIZED] The item subfolder path (if any). Used for custom items which replace textures from PNG.</summary>
        [XmlIgnore]
        public string SubfolderName;

        /// <summary>[Optional] The name of the AssetBundle where your custom ItemVisuals are loaded from</summary>
        public string AssetBundleName;
        /// <summary>[Optional] The name of the GameObject for your standard Item Visuals, in the AssetBundle defined as VisualsBundleName</summary>
        public string ItemVisualsPrefabName;
        /// <summary>[Optional] The name of the GameObject for your SPECIAL Visuals (commonly for Armor visuals), in the AssetBundle defined as VisualsBundleName</summary>
        public string SpecialVisualPrefabName;
        /// <summary>[Optional] The name of the GameObject for your SPECIAL FEMALE Visuals (commonly for Armor visuals), in the AssetBundle defined as VisualsBundleName</summary>
        public string SpecialFemaleVisualPrefabName;

        /// <summary>The Item ID of the Item you are cloning FROM</summary>
        public int Target_ItemID;
        /// <summary>The NEW Item ID for your custom Item (can be the same as target, will overwrite)</summary>
        public int New_ItemID;

        /// <summary>Default = true. Will destroy all child GameObjects on the Item prefab (ie, destroys all existing Effects)</summary>
        public bool DestroyExistingEffects = true;

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

        public bool CastLocomotionEnabled;
        public float MobileCastMovementMult;
        public bool MobileCastPersistToCastDone;
        public Character.SpellCastModifier CastModifier;
        public int CastSheatheRequired;
        public Item.CastTakeTypes CastTakeType;

        public List<string> Tags = new List<string>();

        public ItemStatsHolder StatsHolder;

        public List<EffectTransformHolder> EffectTransforms = new List<EffectTransformHolder>();

        public void ApplyTemplateToItem()
        {
            SL.Log("ItemHolder ApplyTemplateToItem. Target: " + Target_ItemID + ", New: " + New_ItemID);

            if (ResourcesPrefabManager.Instance.GetItemPrefab(New_ItemID) is Item item)
            {
                //SL.Log("Got cloned item, performing changes...");

                SLPack pack = null;
                if (!string.IsNullOrEmpty(SLPackName) && SL.Packs.ContainsKey(SLPackName))
                {
                    pack = SL.Packs[SLPackName];
                }

                CustomItems.SetNameAndDescription(item, Name, Description);

                // TODO set other item values, item stats, etc...


                //************************  This will need to change after DLC.  ************************//

                // TODO ITEM VISUALS
                if (pack != null && !string.IsNullOrEmpty(AssetBundleName))
                {
                    SL.Log("Custom item visuals from assetbundle are not yet supported.");
                }
                
                // clone the visual prefab so any modifications made to it do not affect the original item.
                if (item.UsesVisuals)
                {
                    // SL.Log("No custom visual prefab defined for this item. Cloning the original(s)");

                    if (item.VisualPrefab != null)
                    {
                        CustomItems.CloneVisualPrefab(item, CustomItems.VisualPrefabType.VisualPrefab);
                    }

                    if (item.SpecialVisualPrefabDefault != null)
                    {
                        CustomItems.CloneVisualPrefab(item, CustomItems.VisualPrefabType.SpecialVisualPrefabDefault);
                    }

                    if (item.SpecialVisualPrefabFemale != null)
                    {
                        CustomItems.CloneVisualPrefab(item, CustomItems.VisualPrefabType.SpecialVisualPrefabFemale);
                    }
                }

                // Texture Replacements
                if (pack != null && !string.IsNullOrEmpty(SubfolderName))
                {
                    CustomItems.CheckCustomTextures(this, item);
                }

                // ********************************************************************************************* //
            }
            else
            {
                SL.Log("Could not find new ID in dictionary! Maybe you are trying to apply before calling CustomItems.CreateCustomItem?", 1);
            }
        }



        


        // ***********************  FOR SERIALIZING AN ITEM INTO A TEMPLATE  *********************** //

        public static ItemHolder ParseItemToTemplate(Item item)
        {
            SL.Log("Parsing item to template: " + item.Name);

            var itemHolder = new ItemHolder
            {
                Name                        = item.Name,
                Description                 = item.Description,
                Target_ItemID               = item.ItemID,
                LegacyItemID                = item.LegacyItemID,
                CastLocomotionEnabled       = item.CastLocomotionEnabled,
                CastModifier                = item.CastModifier,
                CastSheatheRequired         = item.CastSheathRequired,
                CastTakeType                = item.CastTakeType,
                GroupItemInDisplay          = item.GroupItemInDisplay,
                HasPhysicsWhenWorld         = item.HasPhysicsWhenWorld,
                IsPickable                  = item.IsPickable,
                IsUsable                    = item.IsUsable,
                MobileCastMovementMult      = item.MobileCastMovementMult,
                MobileCastPersistToCastDone = item.MobileCastPersistToCastDone,
                QtyRemovedOnUse             = item.QtyRemovedOnUse,
                RepairedInRest              = item.RepairedInRest
            };

            if (item.Stats != null)
            {
                itemHolder.StatsHolder = ItemStatsHolder.ParseItemStats(item.Stats);
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
                var effectsChild = EffectTransformHolder.ParseTransform(child);

                if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                {
                    itemHolder.EffectTransforms.Add(effectsChild);
                }
            }            

            // Sub-Templates //
            if (item is Equipment)
            {
                return EquipmentHolder.ParseEquipment(item as Equipment, itemHolder);
            }
            //else if (item is Skill)
            //{
            //    return SkillHolder.ParseSkill(item as Skill, itemHolder);
            //}
            else
            {
                return itemHolder;
            }
        }
    }
}
