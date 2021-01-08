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
    [SL_Serialized]
    public class SL_Item : IPrefabTemplate<int>
    {
        // IPrefabTemplate implementation

        [XmlIgnore] public bool IsCreatingNewID => this.New_ItemID > 0 && this.New_ItemID != this.Target_ItemID;
        [XmlIgnore] public bool DoesTargetExist => ResourcesPrefabManager.Instance.GetItemPrefab(this.Target_ItemID);

        [XmlIgnore] public int TargetID => this.Target_ItemID;
        [XmlIgnore] public int AppliedID => IsCreatingNewID ? this.New_ItemID : this.Target_ItemID;

        public void CreatePrefab() => ApplyToItem();

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly Dictionary<int, List<Action<Item>>> s_initCallbacks = new Dictionary<int, List<Action<Item>>>();

        /// <summary>
        /// The OnInstanceStart event is called when an Item with this template's applied ID is created or loaded during gameplay.
        /// </summary>
        /// <param name="listener">Your callback. The Item argument is the Item instance.</param>
        public void AddOnInstanceStartListener(Action<Item> listener)
        {
            if (s_initCallbacks.ContainsKey(this.AppliedID))
                s_initCallbacks[this.AppliedID].Add(listener);
            else
                s_initCallbacks.Add(this.AppliedID, new List<Action<Item>> { listener });
        }

        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom item template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore] public string SLPackName;
        /// <summary> [NOT SERIALIZED] The name of the folder this custom item is using for textures (MyPack/Items/[SubfolderName]/Textures/).</summary>
        [XmlIgnore] public string SubfolderName;

        [XmlIgnore] public virtual bool ShouldApplyLate => false;

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
        public EditBehaviours EffectBehaviour = EditBehaviours.Override;
        /// <summary>Transform heirarchy containing the Effects and EffectConditions</summary>
        public SL_EffectTransform[] EffectTransforms;

        // Visual prefab stuff
        public SL_ItemVisual ItemVisuals;
        public SL_ItemVisual SpecialItemVisuals;
        public SL_ItemVisual SpecialFemaleItemVisuals;

        [Obsolete("Use SL_Item.Apply() instead (renamed).")]
        public void ApplyTemplateToItem() => Apply();

        /// <summary>
        /// The normal (and safest) way to apply the template. Call this some time before or at SL.BeforePacksLoaded.
        /// </summary>
        public void Apply()
        {
            if (SL.PacksLoaded)
            {
                SL.LogWarning("Applying an Item Template AFTER SL.OnPacksLoaded has been called. This is not recommended, use SL.BeforePacksLoaded at the latest instead.");
                ApplyToItem();
            }
            else
            {
                if (ShouldApplyLate)
                    SL.PendingLateItems.Add(this);
                else
                    SL.PendingItems.Add(this);
            }
        }

        /// <summary>
        /// Tries to apply the template immediately, with the template's New_ItemID (or Target_ItemID if none set)
        /// </summary>
        public void ApplyToItem()
        {
            if (this.New_ItemID <= 0)
                this.New_ItemID = this.Target_ItemID;

            var item = CustomItems.CreateCustomItem(this);

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
                At.SetField(item, "m_activateEffectAnimType", (Character.SpellCastType)this.CastType);

            if (this.OverrideSellModifier != null)
                At.SetField(item, "m_overrideSellModifier", (float)this.OverrideSellModifier);

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
                    stats = (ItemStats)UnityHelpers.FixComponentType(desiredType, stats);
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

            if (type == null)
            {
                SL.LogWarning("Could not get SL_Type for " + item.GetType().FullName + "!");
                return null;
            }

            var holder = (SL_Item)Activator.CreateInstance(type);

            holder.SerializeItem(item);

            return holder;
        }

        public virtual void SerializeItem(Item item)
        {
            Name = item.Name;
            Description = item.Description;
            Target_ItemID = item.ItemID;
            LegacyItemID = item.LegacyItemID;
            CastLocomotionEnabled = item.CastLocomotionEnabled;
            CastModifier = item.CastModifier;
            CastSheatheRequired = item.CastSheathRequired;
            GroupItemInDisplay = item.GroupItemInDisplay;
            HasPhysicsWhenWorld = item.HasPhysicsWhenWorld;
            IsPickable = item.IsPickable;
            IsUsable = item.IsUsable;
            QtyRemovedOnUse = item.QtyRemovedOnUse;
            MobileCastMovementMult = item.MobileCastMovementMult;
            RepairedInRest = item.RepairedInRest;
            BehaviorOnNoDurability = item.BehaviorOnNoDurability;

            CastType = (Character.SpellCastType)At.GetField(item, "m_activateEffectAnimType");

            this.OverrideSellModifier = (float)At.GetField(item, "m_overrideSellModifier");

            if (item.GetComponent<ItemStats>() is ItemStats stats)
            {
                StatsHolder = SL_ItemStats.ParseItemStats(stats);
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
            ItemExtensions = extList.ToArray();

            var tags = new List<string>();
            if (item.Tags != null)
            {
                foreach (Tag tag in item.Tags)
                {
                    tags.Add(tag.TagName);
                }
            }
            Tags = tags.ToArray();

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
                EffectTransforms = children.ToArray();
            }

            if (item.HasVisualPrefab && ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.VisualPrefabPath).GetComponent<ItemVisual>() is ItemVisual visual)
            {
                ItemVisuals = SL_ItemVisual.ParseVisualToTemplate(item, VisualPrefabType.VisualPrefab, visual);
                ItemVisuals.Type = VisualPrefabType.VisualPrefab;
            }
            if (item.HasSpecialVisualDefaultPrefab)
            {
                SpecialItemVisuals = SL_ItemVisual.ParseVisualToTemplate(item, VisualPrefabType.SpecialVisualPrefabDefault, ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabDefaultPath).GetComponent<ItemVisual>());
                SpecialItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabDefault;
            }
            if (item.HasSpecialVisualFemalePrefab)
            {
                SpecialFemaleItemVisuals = SL_ItemVisual.ParseVisualToTemplate(item, VisualPrefabType.SpecialVisualPrefabFemale, ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabFemalePath).GetComponent<ItemVisual>());
                SpecialFemaleItemVisuals.Type = VisualPrefabType.SpecialVisualPrefabFemale;
            }
        }

        // Legacy

        [Obsolete("Use 'AddOnInstanceStartListener' instead (renamed)")]
        public void OnInstanceStart(Action<Item> callback) => AddOnInstanceStartListener(callback);
    }
}
