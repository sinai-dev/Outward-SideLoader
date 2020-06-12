using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_Item
    {
        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom item template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore]
        public string SLPackName;
        /// <summary> [NOT SERIALIZED] The name of the folder this custom item is using for textures (MyPack/Items/[SubfolderName]/Textures/).</summary>
        [XmlIgnore]
        public string SubfolderName;

        /// <summary>The Item ID of the Item you are cloning FROM</summary>
        public int Target_ItemID = -1;
        /// <summary>The NEW Item ID for your custom Item (can be the same as target, will overwrite)</summary>
        public int New_ItemID = -1;

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

        /// <summary><list type="bullet">
        /// <item>NONE: Your effects are added on top of the existing ones.</item>
        /// <item>DestroyEffects: Destroys all child GameObjects on your item, except for "Content" (used for Bags)</item>
        /// <item>OverrideEffects (default): Only destroys child GameObjects if you have defined one of the same name.</item></list>
        /// </summary>
        public EffectBehaviours EffectBehaviour = EffectBehaviours.OverrideEffects;

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
                SL.Log($"Could not find an item with the ID {New_ItemID}! Maybe you are trying to apply before calling CustomItems.CreateCustomItem?", 1);
                return;
            }

            SL.Log("Applying Item Template. ID: " + New_ItemID + ", Name: " + (Name ?? item.Name));

            ApplyToItem(item);
        }

        public virtual void ApplyToItem(Item item)
        {
            SLPack pack = null;
            if (!string.IsNullOrEmpty(SLPackName) && SL.Packs.ContainsKey(SLPackName))
            {
                pack = SL.Packs[SLPackName];
            }

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
                var stats = item.GetComponent<ItemStats>();
                if (!stats)
                {
                    var gameType = Serializer.GetGameType(this.StatsHolder.GetType());
                    stats = (ItemStats)item.gameObject.AddComponent(gameType);
                }

                StatsHolder.ApplyToItem(stats);
            }

            SL_EffectTransform.ApplyTransformList(item.transform, this.EffectTransforms, this.EffectBehaviour);

            //************************  This will need to change after DLC.  ************************//

            ApplyVisuals(pack, item);

            // **************************************************************************************//
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

        private static Type GetBestSLType(Type type)
        {
            if (Serializer.GetSLType(type, false) is Type slType)
            {
                return slType;
            }
            else
            {
                return GetBestSLType(type.BaseType);
            }
        }

        public static SL_Item ParseItemToTemplate(Item item)
        {
            SL.Log("Parsing item to template: " + item.Name);

            var type = GetBestSLType(item.GetType());

            var holder = (SL_Item)Activator.CreateInstance(type);

            holder.SerializeItem(item, holder);

            return holder;
        }

        public virtual void SerializeItem(Item item, SL_Item holder)
        {
            holder.Name = item.Name;
            holder.Description = item.Description;
            holder.Target_ItemID = item.ItemID;
            holder.LegacyItemID = item.LegacyItemID;
            holder.CastLocomotionEnabled = item.CastLocomotionEnabled;
            holder.CastModifier = item.CastModifier;
            holder.CastSheatheRequired = item.CastSheathRequired;
            holder.GroupItemInDisplay = item.GroupItemInDisplay;
            holder.HasPhysicsWhenWorld = item.HasPhysicsWhenWorld;
            holder.IsPickable = item.IsPickable;
            holder.IsUsable = item.IsUsable;
            holder.QtyRemovedOnUse = item.QtyRemovedOnUse;
            holder.MobileCastMovementMult = item.MobileCastMovementMult;
            holder.RepairedInRest = item.RepairedInRest;
            holder.BehaviorOnNoDurability = item.BehaviorOnNoDurability;

            holder.CastType = (Character.SpellCastType)At.GetValue(typeof(Item), item, "m_activateEffectAnimType");

            if (item.GetComponent<ItemStats>() is ItemStats stats)
            {
                holder.StatsHolder = SL_ItemStats.ParseItemStats(stats);
            }

            if (item.Tags != null)
            {
                foreach (Tag tag in item.Tags)
                {
                    holder.Tags.Add(tag.TagName);
                }
            }

            foreach (Transform child in item.transform)
            {
                var effectsChild = SL_EffectTransform.ParseTransform(child);

                if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                {
                    holder.EffectTransforms.Add(effectsChild);
                }
            }
        }
    }
}
