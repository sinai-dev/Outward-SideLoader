using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Localizer;
using System.Text.RegularExpressions;

namespace SideLoader_2
{
    public class CustomItems : MonoBehaviour
    {
        public static CustomItems Instance;

        // cached ResourcesPrefabManager.ITEM_PREFABS Dictionary (reference to actual object)
        private static Dictionary<string, Item> RPM_ITEM_PREFABS;

        // cached ORIGINAL PREFABS (not modified)
        private static readonly Dictionary<int, Item> ORIG_ITEMS = new Dictionary<int, Item>();

        // cached LocalizationManager.Instance.ItemLocalization
        private static Dictionary<int, ItemLocalization> ITEM_LOCALIZATION;

        /// <summary> Will return the true original prefab for this Item ID. </summary>
        public static Item GetOriginalItemPrefab(int ItemID)
        {
            if (ORIG_ITEMS.ContainsKey(ItemID))
            {
                return ORIG_ITEMS[ItemID];
            }
            else
            {
                return ResourcesPrefabManager.Instance.GetItemPrefab(ItemID);
            }
        }

        internal void Awake()
        {
            Instance = this;

            // Cache useful dictionaries used by the game
            RPM_ITEM_PREFABS = At.GetValue(typeof(ResourcesPrefabManager), null, "ITEM_PREFABS") as Dictionary<string, Item>;
            ITEM_LOCALIZATION = At.GetValue(typeof(LocalizationManager), LocalizationManager.Instance, "m_itemLocalization") as Dictionary<int, ItemLocalization>;

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

        /// <summary>
        /// Clones an item prefab and returns the clone to you. Caches the original prefab for other mods or other custom items to reference.
        /// </summary>
        /// <param name="cloneTargetID">The Item ID of the Item you want to clone from</param>
        /// <param name="newID">The new Item ID for your cloned item. Can be the same as the target, will overwrite.</param>
        /// <param name="name">Only used for the gameObject name, not the actual Item Name. This is the name thats used in Debug Menus.</param>
        /// <param name="template">[Optional] If you want to apply a template for this item manually, you can provide it here.</param>
        /// <returns>Your cloned Item prefab</returns>
        public static Item CreateCustomItem(int cloneTargetID, int newID, string name, ItemHolder template = null)
        {
            Item target;

            // Check if another Custom Item has already modified our target. If so, get the cached original.
            if (ORIG_ITEMS.ContainsKey(cloneTargetID))
            {
                SL.Log("CustomItems::CreateCustomItem - The target Item has already been modified. Getting the original item.");
                target = ORIG_ITEMS[cloneTargetID];
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

            if (newID == cloneTargetID && !ORIG_ITEMS.ContainsKey(newID))
            {
                SL.Log("CustomItems::CreateCustomItem - Modifying an original item for the first time, caching it.");
                ORIG_ITEMS.Add(target.ItemID, target);
            }

            var clone = Instantiate(target.gameObject).GetComponent<Item>();
            clone.gameObject.SetActive(false);
            DontDestroyOnLoad(clone);

            clone.gameObject.name = newID + "_" + name;

            clone.ItemID = newID;
            SetItemID(newID, clone);

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


        /// <summary> Small helper for destroying all children on a given Transform 't'. Uses DestroyImmediate(). </summary>
        public static void DestroyChildren(Transform t)
        {
            while (t.childCount > 0)
            {
                DestroyImmediate(t.GetChild(0).gameObject);
            }
        }

        // ***********************   CUSTOM ITEM VISUALS   *********************** //

            /*
             * The reason these methods are here instead of in ItemHolder is because ItemHolder is more 
             * focused on the template than the Item itself.
             * 
             * These functions don't necessarily require an ItemHolder, so that's why I put them here.
            */

        // *********************** THIS WILL NEED REWORK AFTER DLC *********************** //


        public static void CloneVisualPrefab(Item newItem, VisualPrefabType type)
        {
            Transform prefab = null;
            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    prefab = newItem.VisualPrefab;
                    break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    prefab = newItem.SpecialVisualPrefabDefault;
                    break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    prefab = newItem.SpecialVisualPrefabFemale;
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
            var cloneVisuals = GameObject.Instantiate(prefab.gameObject);
            GameObject.DontDestroyOnLoad(cloneVisuals);
            At.SetValue(cloneVisuals.transform, typeof(Item), newItem, type.ToString());
            cloneVisuals.SetActive(false);

            // Clone the materials too so that changes to them don't affect the original item visuals
            foreach (var skinnedMesh in cloneVisuals.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var newmat = GameObject.Instantiate(skinnedMesh.material);
                GameObject.DontDestroyOnLoad(newmat);
                skinnedMesh.material = newmat;
            }

            foreach (var mesh in cloneVisuals.GetComponentsInChildren<MeshRenderer>())
            {
                var newmat = GameObject.Instantiate(mesh.material);
                GameObject.DontDestroyOnLoad(newmat);
                mesh.material = newmat;
            }
        }

        /// <summary>
        /// Will check for the "SLPackFolder/Items/SubfolderName/Textures" folder (if it exists), and if so load and apply these textures to your item.
        /// </summary>
        /// <param name="template">The template for your custom item (must already be set up, including SLPackName and SubfolderName)</param>
        /// <param name="newItem">The actual new item prefab, already created by CreateCustomItem</param>
        public static void CheckCustomTextures(ItemHolder template, Item newItem)
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

        public static string GetSafeMaterialName(string origName)
        {
            var reg = new Regex(@".+?(?= \()"); // match anything up to " ("

            return reg.Match(origName).Value;
        }

        private static void ApplyTexturesFromFolder(string dir, Item newItem)
        {
            // Check for normal item icon
            var iconPath = dir + @"\icon.png";
            if (File.Exists(iconPath))
            {
                var tex = CustomTextures.LoadTexture(iconPath);
                var sprite = CustomTextures.CreateSprite(tex);
                GameObject.DontDestroyOnLoad(sprite);
                At.SetValue(sprite, typeof(Item), newItem, "m_itemIcon");
            }

            // check for Skill icon (if skill)
            var skillPath = dir + @"\skillicon.png";
            if (newItem is Skill skill && File.Exists(skillPath))
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

                    SL.Log("stored texture " + tex.name);

                    textures[matname].Add(tex);
                }
            }

            // apply to mats
            for (int i = 0; i < 3; i++)
            {
                var prefabtype = (VisualPrefabType)i;
                var mats = GetMaterials(newItem, prefabtype);

                if (mats == null)
                {
                    continue;
                }

                foreach (var mat in mats)
                {
                    var matname = GetSafeMaterialName(mat.name);

                    if (!textures.ContainsKey(matname))
                    {
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

        public static void SaveMaterialTextures(Material mat, string dir)
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



        /// <summary>The three types of VisualPrefabs which custom items can use.</summary>
        public enum VisualPrefabType
        {
            VisualPrefab,
            SpecialVisualPrefabDefault,
            SpecialVisualPrefabFemale
        }


    }        
}
