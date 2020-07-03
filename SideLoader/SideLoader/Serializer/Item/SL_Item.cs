﻿using System;
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

        public List<string> Tags;

        public SL_ItemStats StatsHolder;

        /// <summary><list type="bullet">
        /// <item>NONE: Your effects are added on top of the existing ones.</item>
        /// <item>DestroyEffects: Destroys all child GameObjects on your item, except for "Content" (used for Bags)</item>
        /// <item>OverrideEffects (default): Only destroys child GameObjects if you have defined one of the same name.</item></list>
        /// </summary>
        public EffectBehaviours EffectBehaviour = EffectBehaviours.OverrideEffects;

        public List<SL_EffectTransform> EffectTransforms = new List<SL_EffectTransform>();

        /*       Visual Prefab stuff       */

        public SL_ItemVisual ItemVisuals;
        public SL_ItemVisual SpecialItemVisuals;
        public SL_ItemVisual SpecialFemaleItemVisuals;

        public void ApplyTemplateToItem()
        {
            var item = ResourcesPrefabManager.Instance.GetItemPrefab(New_ItemID);
            if (!item)
            {
                SL.Log($"Could not find an item with the ID {New_ItemID}! Maybe you are trying to apply before calling CustomItems.CreateCustomItem?", 1);
                return;
            }

            // re-set this, just to be safe. The component might have been replaced by FixComponentTypeIfNeeded.
            CustomItems.SetItemID(New_ItemID, item);

            SL.Log("Applying Item Template. ID: " + New_ItemID + ", Name: " + (Name ?? item.Name));

            ApplyToItem(item);
        }

        public virtual void ApplyToItem(Item item)
        {
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
                var desiredType = Serializer.GetGameType(this.StatsHolder.GetType());

                var stats = item.GetComponent<ItemStats>();
                if (!stats)
                {
                    stats = (ItemStats)item.gameObject.AddComponent(desiredType);
                }
                else
                {
                    stats = (ItemStats)Serializer.FixComponentTypeIfNeeded(item.transform, desiredType, stats);
                }

                StatsHolder.ApplyToItem(stats);
            }

            SL_EffectTransform.ApplyTransformList(item.transform, this.EffectTransforms, this.EffectBehaviour);

            ApplyItemVisuals(item);
        }

        private void ApplyItemVisuals(Item item)
        {
            // Visual Prefabs
            if (this.ItemVisuals != null)
            {
                ItemVisuals.Type = VisualPrefabType.VisualPrefab;
                this.ItemVisuals.ApplyToItem(item);
            }
            else
            {
                CustomItemVisuals.CloneVisualPrefab(item, VisualPrefabType.VisualPrefab);
            }

            if (this.SpecialItemVisuals != null)
            {
                this.SpecialItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabDefault;
                this.SpecialItemVisuals.ApplyToItem(item);
            }
            else
            {
                CustomItemVisuals.CloneVisualPrefab(item, VisualPrefabType.SpecialVisualPrefabDefault);
            }

            if (this.SpecialFemaleItemVisuals != null)
            {
                this.SpecialFemaleItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabFemale;
                this.SpecialFemaleItemVisuals.ApplyToItem(item);
            }
            else
            {
                CustomItemVisuals.CloneVisualPrefab(item, VisualPrefabType.SpecialVisualPrefabFemale);
            }

            // Texture Replacements
            if (!string.IsNullOrEmpty(SLPackName) && SL.Packs.ContainsKey(SLPackName) && !string.IsNullOrEmpty(this.SubfolderName))
            {
                CustomItemVisuals.TryApplyCustomTextures(this, item);
            }
        }

        // ***********************  FOR SERIALIZING AN ITEM INTO A TEMPLATE  *********************** //

        public static SL_Item ParseItemToTemplate(Item item)
        {
            SL.Log("Parsing item to template: " + item.Name);

            var type = Serializer.GetBestSLType(item.GetType());

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
                holder.Tags = new List<string>();

                foreach (Tag tag in item.Tags)
                {
                    holder.Tags.Add(tag.TagName);
                }
            }

            if (item.transform.childCount > 0)
            {
                foreach (Transform child in item.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                    {
                        holder.EffectTransforms.Add(effectsChild);
                    }
                }
            }

            if (item.HasVisualPrefab && ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.VisualPrefabPath).GetComponent<ItemVisual>() is ItemVisual visual)
            {
                holder.ItemVisuals = SL_ItemVisual.ParseVisualToTemplate(visual);
                holder.ItemVisuals.Type = VisualPrefabType.VisualPrefab;
            }
            if (item.HasSpecialVisualDefaultPrefab)
            {
                holder.SpecialItemVisuals = SL_ItemVisual.ParseVisualToTemplate(ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabDefaultPath).GetComponent<ItemVisual>());
                holder.SpecialItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabDefault;
            }
            if (item.HasSpecialVisualFemalePrefab)
            {
                holder.SpecialFemaleItemVisuals = SL_ItemVisual.ParseVisualToTemplate(ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabFemalePath).GetComponent<ItemVisual>());
                holder.SpecialFemaleItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabFemale;
            }
        }
    }
}