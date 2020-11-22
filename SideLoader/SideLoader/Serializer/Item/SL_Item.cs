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

        [XmlIgnore]
        public virtual bool ShouldApplyLate => false;

        /// <summary>The Item ID of the Item you are cloning FROM</summary>
        public int Target_ItemID = -1;
        /// <summary>The NEW Item ID for your custom Item (can be the same as target, will overwrite)</summary>
        public int New_ItemID = -1;

        public string Name;
        public string Description;

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

        public float? OverrideSellModifier;

        /// <summary>Item Tags, represented as strings (uses CustomTags.GetTag(string tagName)).</summary>
        public string[] Tags;

        /// <summary>Holder for the ItemStats object</summary>
        public SL_ItemStats StatsHolder;

        /// <summary>Determines how the ItemExtensions are replaced and edited</summary>
        public EditBehaviours ExtensionsEditBehaviour = EditBehaviours.NONE;
        /// <summary>List of SL_ItemExtensions for this item. Can only have one per item.</summary>
        public SL_ItemExtension[] ItemExtensions;

        ///// <summary>Determines how the EffectTransforms are replaced and edited</summary>
        public EffectBehaviours EffectBehaviour = EffectBehaviours.OverrideEffects;
        /// <summary>Transform heirarchy containing the Effects and EffectConditions</summary>
        public SL_EffectTransform[] EffectTransforms;

        // Visual prefab stuff
        public SL_ItemVisual ItemVisuals;
        public SL_ItemVisual SpecialItemVisuals;
        public SL_ItemVisual SpecialFemaleItemVisuals;

        [Obsolete("Use SL_Item.Apply() instead (renamed).")]
        public void ApplyTemplateToItem() => Apply();

        /// <summary>
        /// The normal (and safest) way to apply the template. 
        /// If SL.PacksLoaded = true this will apply the template immediately.
        /// Otherwise it uses a callback to apply later.
        /// </summary>
        public void Apply()
        {
            if (SL.PacksLoaded)
            {
                ApplyToItem();
            }
            else
            {
                if (ShouldApplyLate)
                {
                    SL.INTERNAL_ApplyLateItems += ApplyToItem;
                }
                else
                {
                    SL.INTERNAL_ApplyItems += ApplyToItem;
                }
            }
        }

        /// <summary>
        /// Tries to apply the template immediately, with the template's New_ItemID.
        /// </summary>
        public void ApplyToItem()
        {
            var item = ResourcesPrefabManager.Instance.GetItemPrefab(New_ItemID);
            if (!item)
            {
                SL.Log($"Could not find an item with the ID {New_ItemID}! Maybe you are trying to apply before calling CustomItems.CreateCustomItem?", 1);
                return;
            }

            ApplyToItem(item);

            item.IsPrefab = true;
        }

        /// <summary>
        /// Applies the template immediately to the provided Item.
        /// </summary>
        public virtual void ApplyToItem(Item item)
        {
            SL.Log("Applying Item Template. ID: " + New_ItemID + ", Name: " + (Name ?? item.Name));

            var goName = Name ?? item.Name;
            goName = Serializer.ReplaceInvalidChars(goName);
            item.gameObject.name = $"{New_ItemID}_{goName}";

            // re-set this, just to be safe. The component might have been replaced by FixComponentTypeIfNeeded.
            CustomItems.SetItemID(New_ItemID, item);

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

            if (this.OverrideSellModifier != null)
                At.SetValue((float)this.OverrideSellModifier, typeof(Item), item, "m_overrideSellModifier");

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
                    stats = (ItemStats)SL.FixComponentType(desiredType, stats);
                }

                StatsHolder.ApplyToItem(stats);
            }

            if (this.ItemExtensions != null)
            {
                SL_ItemExtension.ApplyExtensionList(item, this.ItemExtensions, this.ExtensionsEditBehaviour);
            }

            SL_EffectTransform.ApplyTransformList(item.transform, this.EffectTransforms, this.EffectBehaviour);

            ApplyItemVisuals(item);
        }

        /// <summary>
        /// Applies the ItemVisuals, and checks for pngs and materials to apply in `SLPack\Items\SubfolderPath\Textures\`.
        /// </summary>
        public void ApplyItemVisuals(Item item)
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

            this.OverrideSellModifier = (float)At.GetValue(typeof(Item), item, "m_overrideSellModifier");

            if (item.GetComponent<ItemStats>() is ItemStats stats)
            {
                holder.StatsHolder = SL_ItemStats.ParseItemStats(stats);
            }

            var extensions = item.gameObject.GetComponentsInChildren<ItemExtension>();
            var extList = new List<SL_ItemExtension>();
            if (extensions != null)
            {
                foreach (var ext in extensions)
                {
                    var extHolder = SL_ItemExtension.SerializeExtension(ext);
                    extList.Add(extHolder);
                }
            }
            holder.ItemExtensions = extList.ToArray();

            var tags = new List<string>();
            if (item.Tags != null)
            {
                foreach (Tag tag in item.Tags)
                {
                    tags.Add(tag.TagName);
                }
            }
            holder.Tags = tags.ToArray();

            if (item.transform.childCount > 0)
            {
                var children = new List<SL_EffectTransform>();
                foreach (Transform child in item.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.HasContent)
                    {
                        children.Add(effectsChild);
                    }
                }
                holder.EffectTransforms = children.ToArray();
            }

            if (item.HasVisualPrefab && ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.VisualPrefabPath).GetComponent<ItemVisual>() is ItemVisual visual)
            {
                holder.ItemVisuals = SL_ItemVisual.ParseVisualToTemplate(item, VisualPrefabType.VisualPrefab, visual);
                holder.ItemVisuals.Type = VisualPrefabType.VisualPrefab;
            }
            if (item.HasSpecialVisualDefaultPrefab)
            {
                holder.SpecialItemVisuals = SL_ItemVisual.ParseVisualToTemplate(item, VisualPrefabType.SpecialVisualPrefabDefault, ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabDefaultPath).GetComponent<ItemVisual>());
                holder.SpecialItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabDefault;
            }
            if (item.HasSpecialVisualFemalePrefab)
            {
                holder.SpecialFemaleItemVisuals = SL_ItemVisual.ParseVisualToTemplate(item, VisualPrefabType.SpecialVisualPrefabFemale, ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabFemalePath).GetComponent<ItemVisual>());
                holder.SpecialFemaleItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabFemale;
            }
        }
    }
}
