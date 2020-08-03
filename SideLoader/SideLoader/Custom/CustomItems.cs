using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Localizer;
using System.Reflection;
using HarmonyLib;

namespace SideLoader
{
    /// <summary>
    /// SideLoader's manager class for Custom Items. Contains useful methods for the creation, mangement and destruction  of SL_Items.
    /// </summary>
    public class CustomItems
    {
        /// <summary>Cached ORIGINAL Item Prefabs (not modified)</summary>
        private static readonly Dictionary<int, Item> OrigItemPrefabs = new Dictionary<int, Item>();

        /// <summary>For legacy support. Use the SideLoader.References class for these sorts of references.</summary>
        public static Dictionary<string, Item> RPM_ITEM_PREFABS => References.RPM_ITEM_PREFABS;
        /// <summary>For legacy support. Use the SideLoader.References class for these sorts of references.</summary>
        public static Dictionary<int, ItemLocalization> ITEM_LOCALIZATION => References.ITEM_LOCALIZATION;

        // ================================================================================ //
        /*                                  Public Helpers                                  */
        // ================================================================================ //

        /// <summary> Will return the true original prefab for this Item ID. </summary>
        public static Item GetOriginalItemPrefab(int ItemID)
        {
            if (OrigItemPrefabs.ContainsKey(ItemID))
            {
                return OrigItemPrefabs[ItemID];
            }
            else
            {
                return ResourcesPrefabManager.Instance.GetItemPrefab(ItemID);
            }
        }

        /// <summary>
        /// Simple method to apply a SL_Item template. 
        /// If defining a custom item after SL.OnPacksLoaded it will be applied instantly, otherwise it uses a callback to be applied later.
        /// </summary>
        /// <param name="template"></param>
        /// <returns>Your new custom item (or the original item, if modifying an existing one)</returns>
        public static Item CreateCustomItem(SL_Item template)
        {
            return CreateCustomItem(template.Target_ItemID, template.New_ItemID, template.Name, template);
        }

        /// <summary>
        /// Clones an item prefab and returns the clone to you. Caches the original prefab for other mods or other custom items to reference.
        /// If you provide a SL_Item template, this will be applied as well (either immediately or with a callback later).
        /// </summary>
        /// <param name="cloneTargetID">The Item ID of the Item you want to clone from</param>
        /// <param name="newID">The new Item ID for your cloned item. Can be the same as the target, will overwrite.</param>
        /// <param name="name">Only used for the gameObject name, not the actual Item Name. This is the name thats used in Debug Menus.</param>
        /// <param name="template">[Optional] If you want to apply a template for this item manually, you can provide it here.</param>
        /// <returns>Your cloned Item prefab</returns>
        public static Item CreateCustomItem(int cloneTargetID, int newID, string name, SL_Item template = null)
        {
            Item original = GetOriginalItemPrefab(cloneTargetID);

            if (!original)
            {
                SL.Log("CustomItems::CreateCustomItem - Error! Could not find the clone target Item ID: " + cloneTargetID, 1);
                return null;
            }

            Item item; 

            // modifying an existing item
            if (newID == cloneTargetID)
            {
                // Modifying the original prefab for the first time. Cache it in case someone else wants the true original.
                if (!OrigItemPrefabs.ContainsKey(newID))
                {
                    var cached = GameObject.Instantiate(original.gameObject).GetComponent<Item>();
                    cached.gameObject.SetActive(false);
                    GameObject.DontDestroyOnLoad(cached.gameObject);
                    OrigItemPrefabs.Add(cached.ItemID, cached);
                }

                // apply to the original item prefab. this ensures direct prefab references to this item reflect the changes.
                item = original;
            }
            else // making a new item
            {
                item = GameObject.Instantiate(original.gameObject).GetComponent<Item>();
                item.gameObject.SetActive(false);
                item.gameObject.name = newID + "_" + name;

                // fix for name and description localization
                SetNameAndDescription(item, original.Name, original.Description);
            }

            if (template != null)
            {
                var gameType = Serializer.GetGameType(template.GetType());
                if (gameType != item.GetType())
                {
                    item = (Item)SL.FixComponentType(gameType, item);
                }
            }

            item.ItemID = newID;
            SetItemID(newID, item);

            // Do this so that any changes we make are not destroyed on scene changes.
            // This is needed whether this is a clone or a new item.
            GameObject.DontDestroyOnLoad(item.gameObject);

            if (template != null)
            {
                template.Apply();
            }

            return item;
        }

        /// <summary>
        /// Sets the ResourcesPrefabManager.ITEM_PREFABS dictionary for a custom Item ID. Will overwrite if the ID exists.
        /// This is called by CustomItems.CreateCustomItem
        /// </summary>
        /// <param name="_ID">The Item ID you want to set</param>
        /// <param name="item">The Item prefab</param>
        public static void SetItemID(int _ID, Item item)
        {
            //SL.Log("Setting a custom Item ID to the ResourcesPrefabManager dictionary. ID: " + ID + ", item name: " + item.Name);

            var id = _ID.ToString();
            if (References.RPM_ITEM_PREFABS.ContainsKey(id))
            {
                References.RPM_ITEM_PREFABS[id] = item;
            }
            else
            {
                References.RPM_ITEM_PREFABS.Add(id, item);
            }
        }

        /// <summary> Helper for setting an Item's name easily </summary>
        public static void SetName(Item item, string name)
        {
            SetNameAndDescription(item, name, item.Description);
        }

        /// <summary> Helper for setting an Item's description easily </summary>
        public static void SetDescription(Item item, string description)
        {
            SetNameAndDescription(item, item.Name, description);
        }

        /// <summary> Set both name and description. Used by SetName and SetDescription. </summary>
        public static void SetNameAndDescription(Item _item, string _name, string _description)
        {
            var name = _name ?? "";
            var desc = _description ?? "";

            At.SetValue(name, typeof(Item), _item, "m_name");
            At.SetValue(desc, typeof(Item), _item, "m_description");

            if (References.ITEM_LOCALIZATION.ContainsKey(_item.ItemID))
            {
                References.ITEM_LOCALIZATION[_item.ItemID].Name = name;
                References.ITEM_LOCALIZATION[_item.ItemID].Desc = desc;
            }
            else
            {
                ItemLocalization loc = new ItemLocalization(name, desc);
                References.ITEM_LOCALIZATION.Add(_item.ItemID, loc);
            }
        }

        // ================ TAGS ================ //

        /// <summary>
        /// Gets a tag from a string tag name. Note: This just calls CustomTags.GetTag(tagName, logging).
        /// </summary>
        public static Tag GetTag(string tagName, bool logging = true)
        {
            return CustomTags.GetTag(tagName, logging);
        }

        /// <summary>
        /// Creates a new custom tag. Note: This just calls CustomTags.CreateTag(tagName).
        /// </summary>
        public static void CreateTag(string tagName)
        {
            CustomTags.CreateTag(tagName);
        }

        /// <summary> Adds the range of tags to the Items' TagSource, and optionally destroys the existing tags.</summary>
        public static void SetItemTags(Item item, string[] tags, bool destroyExisting)
        {
            var tagsource = item.transform.GetOrAddComponent<TagSource>();

            if (destroyExisting)
            {
                At.SetValue(new List<Tag>(), typeof(TagListSelectorComponent), tagsource, "m_tags");
                At.SetValue(new List<TagSourceSelector>(), typeof(TagListSelectorComponent), tagsource, "m_tagSelectors");
            }

            var list = new List<TagSourceSelector>();
            foreach (var tag in tags)
            {
                if (GetTag(tag) is Tag _tag && _tag != Tag.None)
                {
                    list.Add(new TagSourceSelector(_tag));
                }
            }

            At.SetValue(list, typeof(TagListSelectorComponent), tagsource as TagListSelectorComponent, "m_tagSelectors");

            tagsource.RefreshTags();

            At.SetValue(tagsource, typeof(Item), item, "m_tagSource");
        }

        [Obsolete("Use SL.DestroyChildren instead! (Moved)")]
        public static void DestroyChildren(Transform t, bool destroyContent = false)
        {
            SL.DestroyChildren(t, destroyContent);
        }
    }
}
