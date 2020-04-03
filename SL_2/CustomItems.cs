using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Localizer;
using System.Text.RegularExpressions;
using System.Reflection;

namespace SideLoader_2
{
    public class CustomItems : MonoBehaviour
    {
        public static CustomItems Instance;

        /// <summary> Cached ORIGINAL Item Prefabs (not modified) </summary>
        private static readonly Dictionary<int, Item> OrigItemPrefabs = new Dictionary<int, Item>();

        /// <summary> Custom Item Visual prefabs (including retexture-only) </summary>
        private static readonly Dictionary<int, ItemVisualsLink> CustomItemVisuals = new Dictionary<int, ItemVisualsLink>();

        // Used to get tags more easily (by string instead of UID)
        private static readonly Dictionary<string, Tag> AllTags = new Dictionary<string, Tag>();

        /// <summary> cached ResourcesPrefabManager.ITEM_PREFABS Dictionary (reference to actual object) </summary>
        private static Dictionary<string, Item> RPM_ITEM_PREFABS;

        /// <summary> cached LocalizationManager.Instance.ItemLocalization </summary>
        private static Dictionary<int, ItemLocalization> ITEM_LOCALIZATION;

        internal void Awake()
        {
            Instance = this;

            // Cache useful dictionaries used by the game
            RPM_ITEM_PREFABS = At.GetValue(typeof(ResourcesPrefabManager), null, "ITEM_PREFABS") as Dictionary<string, Item>;
            ITEM_LOCALIZATION = At.GetValue(typeof(LocalizationManager), LocalizationManager.Instance, "m_itemLocalization") as Dictionary<int, ItemLocalization>;

            var tags = At.GetValue(typeof(TagSourceManager), TagSourceManager.Instance, "m_tags") as Tag[];
            foreach (var tag in tags)
            {
                AllTags.Add(tag.TagName, tag);
            }

            // Hooks for bug fixing
            On.ItemListDisplay.SortBySupport += SortBySupportHook;
        }

        private int SortBySupportHook(On.ItemListDisplay.orig_SortBySupport orig, Item _item1, Item _item2)
        {
            try
            {
                return orig(_item1, _item2);
            }
            catch
            {
                return -1;
            }
        }

        // ============================================================================================ //
        /*                                       Public Helpers                                         */
        // ============================================================================================ //

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

        public static Transform GetCustomItemVisuals(Item item, VisualPrefabType type)
        {
            if (!CustomItemVisuals.ContainsKey(item.ItemID))
            {
                // return ResourcesPrefabManager.Instance.GetItemVisuals ??
                return null;
            }

            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    return CustomItemVisuals[item.ItemID].ItemVisuals; // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    return CustomItemVisuals[item.ItemID].ItemSpecialVisuals;  // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    return CustomItemVisuals[item.ItemID].ItemSpecialFemaleVisuals;  // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
                default:
                    SL.Log("CustomItemVisuals dictionary contains this item ID, but the visuals are null! ID: " + item.ItemID + ", type: " + type, 1);
                    return null;
            }
        }

        /// <summary>
        /// Returns the game's actual Tag for the string you provide, if it exists.
        /// </summary>
        /// <param name="TagName">Eg "Food", "Blade", etc...</param>
        /// <returns></returns>
        public static Tag GetTag(string TagName)
        {
            if (AllTags.ContainsKey(TagName))
            {
                return AllTags[TagName];
            }
            else
            {
                SL.Log("GetTag :: Could not find a tag by the name: " + TagName);
                return Tag.None;
            }
        }

        // ============================================================================================ //
        /*                                   Setting up a Custom Item                                   */
        // ============================================================================================ //

        /// <summary>
        /// Clones an item prefab and returns the clone to you. Caches the original prefab for other mods or other custom items to reference.
        /// </summary>
        /// <param name="cloneTargetID">The Item ID of the Item you want to clone from</param>
        /// <param name="newID">The new Item ID for your cloned item. Can be the same as the target, will overwrite.</param>
        /// <param name="name">Only used for the gameObject name, not the actual Item Name. This is the name thats used in Debug Menus.</param>
        /// <param name="template">[Optional] If you want to apply a template for this item manually, you can provide it here.</param>
        /// <returns>Your cloned Item prefab</returns>
        public static Item CreateCustomItem(int cloneTargetID, int newID, string name, SL_Item template = null)
        {
            Item target;

            // Check if another Custom Item has already modified our target. If so, get the cached original.
            if (OrigItemPrefabs.ContainsKey(cloneTargetID))
            {
                SL.Log("CustomItems::CreateCustomItem - The target Item has already been modified. Getting the original item.");
                target = OrigItemPrefabs[cloneTargetID];
            }
            else
            {
                target = ResourcesPrefabManager.Instance.GetItemPrefab(cloneTargetID);

                if (!target)
                {
                    SL.Log("CustomItems::CreateCustomItem - Error! Could not find the clone target Item ID: " + cloneTargetID, 1);
                    return null;
                }
            }            

            if (newID == cloneTargetID && !OrigItemPrefabs.ContainsKey(newID))
            {
                SL.Log("CustomItems::CreateCustomItem - Modifying an original item for the first time, caching it.");
                OrigItemPrefabs.Add(target.ItemID, target);
            }

            var clone = Instantiate(target.gameObject).GetComponent<Item>();
            clone.gameObject.SetActive(false);
            DontDestroyOnLoad(clone);

            clone.gameObject.name = newID + "_" + name;

            clone.ItemID = newID;
            SetItemID(newID, clone);

            // fix for recipes (not sure if needed anymore?)
            if (!clone.GetComponent<TagSource>())
            {
                var tags = clone.gameObject.AddComponent<TagSource>();
                tags.RefreshTags();
            }

            if (template != null)
            {
                template.ApplyTemplateToItem();
            }

            return clone;
        }

        /// <summary>
        /// Fixes the ResourcesPrefabManager.ITEM_PREFABS dictionary for a custom Item ID. Will overwrite if the ID exists.
        /// This is called by CustomItems.CreateCustomItem
        /// </summary>
        /// <param name="ID">The Item ID you want to set</param>
        /// <param name="item">The Item prefab</param>
        public static void SetItemID(int ID, Item item)
        {
            //SL.Log("Setting a custom Item ID to the ResourcesPrefabManager dictionary. ID: " + ID + ", item name: " + item.Name);

            var idstring = ID.ToString();
            if (RPM_ITEM_PREFABS.ContainsKey(idstring))
            {
                RPM_ITEM_PREFABS[idstring] = item;
            }
            else
            {
                RPM_ITEM_PREFABS.Add(idstring, item);
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

            ItemLocalization loc = new ItemLocalization(name, desc);

            if (ITEM_LOCALIZATION.ContainsKey(_item.ItemID))
            {
                ITEM_LOCALIZATION[_item.ItemID] = loc;
            }
            else
            {
                ITEM_LOCALIZATION.Add(_item.ItemID, loc);
            }
        }

        /// <summary> Adds the range of tags to the Items' TagSource, and optionally destroys the existing tags.</summary>
        public static void SetItemTags(Item item, List<string> tags, bool destroyExisting)
        {
            if (destroyExisting && item.GetComponent<TagSource>() is TagSource origTags)
            {
                GameObject.DestroyImmediate(origTags);
            }

            var tagsource = item.transform.GetOrAddComponent<TagSource>();
            tagsource.RefreshTags();

            var taglist = new List<TagSourceSelector>();
            foreach (var tag in tags)
            {
                taglist.Add(new TagSourceSelector(GetTag(tag)));
            }

            At.SetValue(taglist, typeof(TagListSelectorComponent), tagsource as TagListSelectorComponent, "m_tagSelectors");
        }

        /// <summary> Small helper for destroying all children on a given Transform 't'. Uses DestroyImmediate(). </summary>
        public static void DestroyChildren(Transform t)
        {
            while (t.childCount > 0)
            {
                DestroyImmediate(t.GetChild(0).gameObject);
            }
        }

        /// <summary>Writes all the values from 'other' to 'comp', then returns comp.</summary>
        public static T GetCopyOf<T>(Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null;
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.Static;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { }
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        // ============================================================================================ //
        /*                                    CUSTOM ITEM VISUALS                                       */
        // ============================================================================================ //

        /*                              THIS WILL NEED REWORK AFTER DLC
         * 
         * The reason these methods are here instead of in ItemHolder is because ItemHolder is more 
         * focused on the template than the Item itself.
         * 
         * These functions don't necessarily require an ItemHolder, so that's why I put them here.
        */

        /// <summary>The three types of VisualPrefabs which custom items can use.</summary>
        public enum VisualPrefabType
        {
            VisualPrefab,
            SpecialVisualPrefabDefault,
            SpecialVisualPrefabFemale
        }

        public static void SetVisualPrefab(Item item, Transform origPrefab, Transform newPrefab, VisualPrefabType type, Vector3 positionOffset, Vector3 rotationOffset, bool hideFace = false, bool hideHair = false)
        {
            var clone = Instantiate(origPrefab.gameObject);
            DontDestroyOnLoad(clone.gameObject);
            clone.SetActive(false);

            var newModel = Instantiate(newPrefab.gameObject);
            DontDestroyOnLoad(newModel.gameObject);
            //newModel.SetActive(false);

            if (origPrefab.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                if (item is ProjectileWeapon)
                {
                    // bows are not yet supported, sorry!
                    SL.Log("Custom Visual Prefabs for Bows are not yet supported, sorry!", 0);
                    return;
                }
                else
                {
                    if (!newModel.GetComponent<ArmorVisuals>())
                    {
                        var component = newModel.AddComponent<ArmorVisuals>();
                        GetCopyOf<ArmorVisuals>(component, clone.GetComponent<ArmorVisuals>());
                    }

                    newModel.transform.position = clone.transform.position;
                    newModel.transform.rotation = clone.transform.rotation;

                    newModel.gameObject.SetActive(false);

                    // we no longer need the clone for these visuals. we should clean it up.
                    Destroy(clone.gameObject);
                }
            }
            else // setting normal item visual prefab.
            {
                // At the moment, we only use the 3d Model for standard Item Visuals, the rest of the prefab is original from the cloned item.
                foreach (Transform child in clone.transform)
                {
                    if (child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
                    {
                        child.gameObject.SetActive(false);

                        newModel.transform.position = child.position;
                        newModel.transform.rotation = child.rotation;
                        newModel.transform.parent = child.parent;

                        break;
                    }
                }
            }

            // add manual offsets
            newModel.transform.position += positionOffset;
            newModel.transform.eulerAngles += rotationOffset;

            // set ItemVisualsLink
            ItemVisualsLink link;
            if (!CustomItemVisuals.ContainsKey(item.ItemID))
            {
                CustomItemVisuals.Add(item.ItemID, new ItemVisualsLink()
                {
                    LinkedItem = item,
                });
            }
            link = CustomItemVisuals[item.ItemID];

            switch (type) // set to CLONE for ItemVisuals, and the ACTUAL MODEL for Special Visuals
            {
                case VisualPrefabType.VisualPrefab:
                    item.VisualPrefab = clone.transform;
                    link.ItemVisuals = clone.transform;
                    break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    item.SpecialVisualPrefabDefault = newModel.transform;
                    link.ItemSpecialVisuals = newModel.transform;
                    break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    item.SpecialVisualPrefabFemale = newModel.transform;
                    link.ItemSpecialFemaleVisuals = newModel.transform;
                    break;
            }
        }

        /// <summary> Clone's an items current visual prefab (and materials), then sets this item's visuals to the new cloned prefab. </summary>
        public static void CloneVisualPrefab(Item item, VisualPrefabType type)
        {
            Transform prefab = null;
            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    prefab = item.VisualPrefab;
                    break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    prefab = item.SpecialVisualPrefabDefault;
                    break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    prefab = item.SpecialVisualPrefabFemale;
                    break;
                default:
                    break;
            }

            if (!prefab)
            {
                SL.Log("Error, no VisualPrefabType defined or could not find visual prefab of that type!");
                return;
            }

            // Clone the visual prefab 
            var newVisuals = GameObject.Instantiate(prefab.gameObject);
            GameObject.DontDestroyOnLoad(newVisuals);
            newVisuals.SetActive(false);

            // set the item's visuals to our new clone
            At.SetValue(newVisuals.transform, typeof(Item), item, type.ToString());

            // add to our CustomVisualPrefab dictionary
            if (!CustomItemVisuals.ContainsKey(item.ItemID))
            {
                CustomItemVisuals.Add(item.ItemID, new ItemVisualsLink()
                {
                    LinkedItem = item
                });
            }
            var link = CustomItemVisuals[item.ItemID];
            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    link.ItemVisuals = newVisuals.transform;
                    break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    link.ItemSpecialVisuals = newVisuals.transform;
                    break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    link.ItemSpecialFemaleVisuals = newVisuals.transform;
                    break;
            }

            // Clone the materials too so that changes to them don't affect the original item visuals
            foreach (var skinnedMesh in newVisuals.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mats = skinnedMesh.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    var newmat = Instantiate(mats[i]);
                    DontDestroyOnLoad(newmat);
                    mats[i] = newmat;
                }

                skinnedMesh.materials = mats;
            }

            foreach (var mesh in newVisuals.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = mesh.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    var newmat = Instantiate(mats[i]);
                    DontDestroyOnLoad(newmat);
                    mats[i] = newmat;
                }

                mesh.materials = mats;
            }
        }

        /// <summary>
        /// Will check for the "SLPackFolder/Items/SubfolderName/Textures" folder (if it exists), and if so load and apply these textures to your item.
        /// </summary>
        /// <param name="template">The template for your custom item (must already be set up, including SLPackName and SubfolderName)</param>
        /// <param name="newItem">The actual new item prefab, already created by CreateCustomItem</param>
        public static void TryApplyCustomTextures(SL_Item template, Item newItem)
        {
            if (string.IsNullOrEmpty(template.SLPackName) || !SL.Packs.ContainsKey(template.SLPackName) || string.IsNullOrEmpty(template.SubfolderName))
            {
                SL.Log("Trying to CheckCustomTextures for " + newItem.Name + " but either SLPackName or SubfolderName is not set!", 0);
                return;
            }

            var pack = SL.Packs[template.SLPackName];
            var dir = pack.GetSubfolderPath(SLPack.SubFolders.Items) + @"\" + template.SubfolderName + @"\Textures";

            if (Directory.Exists(dir))
            {
                ApplyTexturesFromFolder(dir, newItem);
            }
        }        

        /// <summary>
        /// Gets an array of the Materials on the given visual prefab type for the given item.
        /// These are actual references to the Materials, not a copy like Unity's Renderer.Materials[]
        /// </summary>
        public static Material[] GetMaterials(Item item, VisualPrefabType type)
        {
            var transforms = new Transform[]
            {
                item.VisualPrefab,
                item.SpecialVisualPrefabDefault,
                item.SpecialVisualPrefabFemale
            };

            var prefab = transforms[(int)type];

            if (prefab != null)
            {
                var mats = new List<Material>();

                foreach (var skinnedMesh in prefab.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    mats.AddRange(skinnedMesh.materials);
                }

                foreach (var mesh in prefab.GetComponentsInChildren<MeshRenderer>())
                {
                    mats.AddRange(mesh.materials);
                }

                return mats.ToArray();
            }

            //SL.Log("No material found for this prefab/item!");
            return null;
        }

        /// <summary>
        /// Returns the true name of the given material name (removes "(Clone)" and "(Instance)", etc)
        /// </summary>
        public static string GetSafeMaterialName(string origName)
        {
            var reg = new Regex(@".+?(?= \()"); // match anything up to " ("

            return reg.Match(origName).Value;
        }

        /// <summary>
        /// INTERNAL. For applying textures to an item from a given directory.
        /// </summary>
        /// <param name="dir">Full path relative to Outward folder.</param>
        private static void ApplyTexturesFromFolder(string dir, Item item)
        {
            // Check for normal item icon
            var iconPath = dir + @"\icon.png";
            if (File.Exists(iconPath))
            {
                var tex = CustomTextures.LoadTexture(iconPath);
                var sprite = CustomTextures.CreateSprite(tex);
                GameObject.DontDestroyOnLoad(sprite);
                At.SetValue(sprite, typeof(Item), item, "m_itemIcon");
            }

            // check for Skill icon (if skill)
            var skillPath = dir + @"\skillicon.png";
            if (item is Skill skill && File.Exists(skillPath))
            {
                var tex = CustomTextures.LoadTexture(skillPath);
                var sprite = CustomTextures.CreateSprite(tex);
                GameObject.DontDestroyOnLoad(sprite);
                skill.SkillTreeIcon = CustomTextures.CreateSprite(tex);
            }

            // build dictionary of textures
            var textures = new Dictionary<string, List<Texture2D>>(); // Key: Material name (Safe), Value: Texture

            foreach (var subfolder in Directory.GetDirectories(dir))
            {
                var matname = Path.GetFileName(subfolder);

                SL.Log("reading folder " + matname);

                textures.Add(matname, new List<Texture2D>());

                foreach (var filepath in Directory.GetFiles(subfolder, "*.png"))
                {
                    var name = Path.GetFileNameWithoutExtension(filepath);
                    var tex = CustomTextures.LoadTexture(filepath);
                    tex.name = name;

                    //SL.Log("stored texture " + tex.name);

                    textures[matname].Add(tex);
                }
            }

            // apply to mats
            for (int i = 0; i < 3; i++)
            {
                var prefabtype = (VisualPrefabType)i;
                var mats = GetMaterials(item, prefabtype);

                if (mats == null)
                {
                    continue;
                }

                foreach (var mat in mats)
                {
                    var matname = GetSafeMaterialName(mat.name);

                    if (!textures.ContainsKey(matname))
                    {
                        if (mat.mainTexture != null)
                        {
                            //SL.Log("CustomItem Textures folder does not have textures defined for " + matname);
                        }
                        continue;
                    }

                    foreach (var tex in textures[matname])
                    {
                        if (mat.GetTexture(tex.name) is Texture)
                        {
                            SL.Log("Set texture " + tex.name + " on " + matname);
                            mat.SetTexture(tex.name, tex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves textures from an Item to a directory.
        /// </summary>
        /// <param name="dir">Full path, relative to Outward folder</param>
        public static void SaveAllItemTextures(Item item, string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (item.ItemIcon != null)
            {
                CustomTextures.SaveTextureAsPNG(item.ItemIcon.texture, dir, "icon");
            }

            if (item is Skill skill && skill.SkillTreeIcon != null)
            {
                CustomTextures.SaveTextureAsPNG(skill.SkillTreeIcon.texture, dir, "skillicon");
            }

            for (int i = 0; i < 3; i++)
            {
                if (GetMaterials(item, (VisualPrefabType)i) is Material[] mats)
                {
                    foreach (var mat in mats)
                    {
                        string subdir = GetSafeMaterialName(mat.name);

                        SaveMaterialTextures(mat, dir + @"\" + subdir);
                    }
                }
            }
        }

        // Internal. Called by function above.
        private static void SaveMaterialTextures(Material mat, string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            bool any = false;

            foreach (var layer in CustomTextures.SuffixToShaderLayer.Values)
            {
                var layername = layer.ToString();

                if (mat.GetTexture(layername) is Texture tex)
                {
                    CustomTextures.SaveTextureAsPNG(tex as Texture2D, dir, layername);

                    if (!any) 
                        any = true;
                }
            }

            if (!any) // this mat has no textures we can edit. just delete the folder.
            {
                SL.Log("Material " + mat.name + " has no textures. Deleting subfolder.");
                Directory.Delete(dir);
            }
        }

        // used internall for managing custom item visuals with the resources prefab manager.
        public class ItemVisualsLink
        {
            public Item LinkedItem;
            public SL_Item LinkedTemplate;

            public Transform ItemVisuals;
            public Transform ItemSpecialVisuals;
            public Transform ItemSpecialFemaleVisuals;
        }
    }        
}
