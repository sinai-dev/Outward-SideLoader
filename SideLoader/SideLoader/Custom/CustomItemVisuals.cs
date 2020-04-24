using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace SideLoader
{
    public class CustomItemVisuals
    {
        // ============================================================================================ //
        /*                                    CUSTOM ITEM VISUALS                                       */
        // ============================================================================================ //

        /* 
         * This will need fixing after DLC (Item.ItemVisual not a direct link, only string reference to Resources path)
         * Will probably have to hook something like ResourcesPrefabManager.GetItemVisual(int id) / GetSpecialVisual / GetSpecialFemaleVisual
        */

        /// <summary> Custom Item Visual prefabs (including retexture-only) </summary>
        private static readonly Dictionary<int, ItemVisualsLink> ItemVisuals = new Dictionary<int, ItemVisualsLink>();

        /// <summary>The three types of VisualPrefabs which custom items can use.</summary>
        public enum VisualPrefabType
        {
            VisualPrefab,
            SpecialVisualPrefabDefault,
            SpecialVisualPrefabFemale
        }

        /// <summary>Use this to get the current ItemVisuals prefab an item is using (custom or original)</summary>
        public static Transform GetItemVisuals(Item item, VisualPrefabType type)
        {
            if (!ItemVisuals.ContainsKey(item.ItemID))
            {
                // return ResourcesPrefabManager.Instance.GetItemVisuals ??
                return null;
            }

            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    return ItemVisuals[item.ItemID].ItemVisuals; // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    return ItemVisuals[item.ItemID].ItemSpecialVisuals;  // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    return ItemVisuals[item.ItemID].ItemSpecialFemaleVisuals;  // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
                default:
                    SL.Log("CustomItemVisuals dictionary contains this item ID, but the visuals are null! ID: " + item.ItemID + ", type: " + type, 1);
                    return null;
            }
        }

        /// <summary>
        /// For replacing an item's visual prefab with your own prefab. For standard ItemVisuals, this only replaces the 3D model and leaves the rest.
        /// </summary>
        /// <param name="origPrefab">The original Transform you are replacing (eg Item.VisualPrefab, Item.SpecialVisualPrefabDefault, etc)</param>
        /// <param name="newPrefab">Your new prefab Transform.</param>
        public static void SetVisualPrefab(Item item, Transform origPrefab, Transform newPrefab, VisualPrefabType type, Vector3 positionOffset, Vector3 rotationOffset, bool hideFace = false, bool hideHair = false)
        {
            var clone = GameObject.Instantiate(origPrefab.gameObject);
            GameObject.DontDestroyOnLoad(clone.gameObject);
            clone.SetActive(false);

            var newModel = GameObject.Instantiate(newPrefab.gameObject);
            GameObject.DontDestroyOnLoad(newModel.gameObject);

            if (origPrefab.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                if (item is ProjectileWeapon p && p.Type == Weapon.WeaponType.Bow) // Bows
                {
                    SL.Log("Custom Visual Prefabs for Bows are not yet supported, sorry!", 0);
                    return;
                }
                else // Armor
                {
                    if (!newModel.GetComponent<ArmorVisuals>())
                    {
                        var component = newModel.AddComponent<ArmorVisuals>();
                        SL.GetCopyOf<ArmorVisuals>(component, clone.GetComponent<ArmorVisuals>());
                    }

                    newModel.transform.position = clone.transform.position;
                    newModel.transform.rotation = clone.transform.rotation;

                    newModel.gameObject.SetActive(false);

                    // we no longer need the clone for these visuals. we should clean it up.
                    GameObject.Destroy(clone.gameObject);
                }
            }
            else // setting normal item visual prefab.
            {
                // At the moment, the only thing we replace on ItemVisuals is the 3d model, everything else is a clone.
                foreach (Transform child in clone.transform)
                {
                    // the real 3d model will always have boxcollider and meshrenderer. this is the object we want to replace.
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
            if (!ItemVisuals.ContainsKey(item.ItemID))
            {
                ItemVisuals.Add(item.ItemID, new ItemVisualsLink()
                {
                    LinkedItem = item,
                });
            }
            link = ItemVisuals[item.ItemID];

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
            newVisuals.SetActive(false);
            GameObject.DontDestroyOnLoad(newVisuals);

            // set the item's visuals to our new clone
            At.SetValue(newVisuals.transform, typeof(Item), item, type.ToString());

            // add to our CustomVisualPrefab dictionary
            if (!ItemVisuals.ContainsKey(item.ItemID))
            {
                ItemVisuals.Add(item.ItemID, new ItemVisualsLink()
                {
                    LinkedItem = item
                });
            }
            var link = ItemVisuals[item.ItemID];
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
                    var newmat = GameObject.Instantiate(mats[i]);
                    GameObject.DontDestroyOnLoad(newmat);
                    mats[i] = newmat;
                }

                skinnedMesh.materials = mats;
            }

            foreach (var mesh in newVisuals.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = mesh.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    var newmat = GameObject.Instantiate(mats[i]);
                    GameObject.DontDestroyOnLoad(newmat);
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

        // In english: Match anything up to " (" 
        private static readonly Regex materialRegex = new Regex(@".+?(?= \()");

        /// <summary>
        /// Returns the true name of the given material name (removes "(Clone)" and "(Instance)", etc)
        /// </summary>
        public static string GetSafeMaterialName(string origName)
        {
            return materialRegex.Match(origName).Value;
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
                var tex = CustomTextures.LoadTexture(iconPath, false);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.ItemIcon);
                GameObject.DontDestroyOnLoad(sprite);
                At.SetValue(sprite, typeof(Item), item, "m_itemIcon");
            }

            // check for Skill icon (if skill)
            var skillPath = dir + @"\skillicon.png";
            if (item is Skill skill && File.Exists(skillPath))
            {
                var tex = CustomTextures.LoadTexture(skillPath, false);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.SkillTreeIcon);
                GameObject.DontDestroyOnLoad(sprite);
                skill.SkillTreeIcon = sprite;
            }

            // build dictionary of textures
            var textures = new Dictionary<string, List<Texture2D>>(); // Key: Material name (Safe), Value: Texture

            foreach (var subfolder in Directory.GetDirectories(dir))
            {
                var matname = Path.GetFileName(subfolder);

                SL.Log("Reading folder " + matname);

                textures.Add(matname, new List<Texture2D>());

                foreach (var filepath in Directory.GetFiles(subfolder, "*.png"))
                {
                    var name = Path.GetFileNameWithoutExtension(filepath);

                    bool isNormal = name == "_BumpMap" || name == "_NormTex";
                    var tex = CustomTextures.LoadTexture(filepath, isNormal);

                    tex.name = name;
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

                    string matPath = dir + @"\" + matname + @"\properties.xml";
                    if (File.Exists(matPath))
                    {
                        var matHolder = Serializer.LoadFromXml(matPath) as SL_Material;

                        matHolder.ApplyToMaterial(mat);
                    }

                    if (!textures.ContainsKey(matname))
                    {
                        continue;
                    }

                    foreach (var tex in textures[matname])
                    {
                        try
                        {
                            if (mat.HasProperty(tex.name))
                            {
                                mat.SetTexture(tex.name, tex);
                                //mat.EnableKeyword(tex.name);
                                SL.Log("Set texture " + tex.name + " on " + matname);
                            }
                            else
                            {
                                SL.Log("Couldn't find a shader property called " + tex.name + "!");
                            }
                        }
                        catch 
                        {
                            SL.Log("Exception setting texture " + tex.name + " on material!");
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
                        string subdir = dir + @"\" + GetSafeMaterialName(mat.name);

                        var matHolder = SL_Material.ParseMaterial(mat);
                        Serializer.SaveToXml(subdir, "properties", matHolder);

                        SaveMaterialTextures(mat, subdir);
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

                if (mat.HasProperty(layername) && mat.GetTexture(layername) is Texture tex)
                {
                    CustomTextures.SaveTextureAsPNG(tex as Texture2D, dir, layername);

                    if (!any) { any = true; }                        
                }
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
