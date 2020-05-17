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
        /* 
         * This will need fixing after DLC (Item.ItemVisual not a direct link, only string reference to Resources path)
         * Will probably have to hook something like ResourcesPrefabManager.GetItemVisual(int id) 
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


        public static Transform GetItemVisuals(Item item, VisualPrefabType type)
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
            return prefab;
        }

        ///// <summary>Use this to get the current ItemVisuals prefab an item is using (custom or original)</summary>
        //public static Transform GetCustomItemVisuals(Item item, VisualPrefabType type)
        //{
        //    if (!ItemVisuals.ContainsKey(item.ItemID))
        //    {
        //        // return ResourcesPrefabManager.Instance.GetItemVisuals ??
        //        return null;
        //    }

        //    switch (type)
        //    {
        //        case VisualPrefabType.VisualPrefab:
        //            return ItemVisuals[item.ItemID].ItemVisuals; // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
        //        case VisualPrefabType.SpecialVisualPrefabDefault:
        //            return ItemVisuals[item.ItemID].ItemSpecialVisuals;  // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
        //        case VisualPrefabType.SpecialVisualPrefabFemale:
        //            return ItemVisuals[item.ItemID].ItemSpecialFemaleVisuals;  // ?? ResourcesPrefabManager.Instance.GetItemVisuals(id, type);
        //        // impossible outcome, but needs to be here for compiler warning
        //        default: return null; 
        //    }
        //}

        /// <summary>
        /// For replacing an item's visual prefab with your own prefab. For standard ItemVisuals, this only replaces the 3D model and leaves the rest.
        /// </summary>
        /// <param name="origPrefab">The original Transform you are replacing (eg Item.VisualPrefab, Item.SpecialVisualPrefabDefault, etc)</param>
        /// <param name="newVisuals">Your new prefab Transform.</param>
        public static void SetVisualPrefab(Item item, Transform newVisuals, VisualPrefabType type, Vector3 positionOffset, Vector3 rotationOffset, bool? hideFace = null, bool? hideHair = null)
        {
            var basePrefab = GameObject.Instantiate(GetItemVisuals(item, type).gameObject);
            GameObject.DontDestroyOnLoad(basePrefab);
            basePrefab.SetActive(false);

            Debug.Log("Setting the " + type + " of " + item.Name + ", origprefab: " + basePrefab.name + ", newPrefab: " + newVisuals.name);            

            var visualModel = UnityEngine.Object.Instantiate(newVisuals.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(visualModel.gameObject);

            if (type == VisualPrefabType.VisualPrefab)
            {
                // At the moment, the only thing we replace on ItemVisuals is the 3d model, everything else is a clone.
                foreach (Transform child in basePrefab.transform)
                {
                    // the real 3d model will always have boxcollider and meshrenderer. this is the object we want to replace.
                    if (child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
                    {
                        child.gameObject.SetActive(false);

                        visualModel.transform.position = child.position;
                        visualModel.transform.rotation = child.rotation;
                        visualModel.transform.parent = child.parent;

                        break;
                    }
                }
            }
            else
            {
                if (!visualModel.GetComponent<ItemVisual>())
                {
                    if (basePrefab.GetComponent<ItemVisual>() is ItemVisual itemVisual)
                    {
                        if (itemVisual is ArmorVisuals armorVisuals)
                        {
                            var newcomp = visualModel.AddComponent<ArmorVisuals>();
                            SL.GetCopyOf(newcomp, armorVisuals);
                            if (hideHair != null)
                            {
                                newcomp.HideHair = (bool)hideHair;
                            }
                            if (hideFace != null)
                            {
                                newcomp.HideFace = (bool)hideFace;
                            }
                        }
                        else
                        {
                            var newcomp = visualModel.AddComponent<ItemVisual>();
                            SL.GetCopyOf(newcomp, itemVisual);
                        }
                    }
                }

                visualModel.transform.position = basePrefab.transform.position;
                visualModel.transform.rotation = basePrefab.transform.rotation;
                visualModel.gameObject.SetActive(false);

                // we no longer need the clone for these visuals. we should clean it up.
                UnityEngine.Object.DestroyImmediate(basePrefab.gameObject);
            }

            // add manual offsets
            visualModel.transform.position += positionOffset;
            visualModel.transform.eulerAngles += rotationOffset;

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
                    item.VisualPrefab = basePrefab.transform;
                    link.ItemVisuals = basePrefab.transform;
                    break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    item.SpecialVisualPrefabDefault = visualModel.transform;
                    link.ItemSpecialVisuals = visualModel.transform;
                    break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    item.SpecialVisualPrefabFemale = visualModel.transform;
                    link.ItemSpecialFemaleVisuals = visualModel.transform;
                    break;
            }
        }


        /// <summary> Clone's an items current visual prefab (and materials), then sets this item's visuals to the new cloned prefab. </summary>
        public static GameObject CloneVisualPrefab(Item item, VisualPrefabType type, Vector3 positionOffset, Vector3 rotationOffset)
        {
            var prefab = GetItemVisuals(item, type);

            if (!prefab)
            {
                SL.Log("Error, no VisualPrefabType defined or could not find visual prefab of that type!");
                return null;
            }

            // Clone the visual prefab 
            var newVisuals = UnityEngine.Object.Instantiate(prefab.gameObject);
            newVisuals.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(newVisuals);

            // add the position and rotation offsets
            if (newVisuals.GetComponent<SkinnedMeshRenderer>())
            {
                newVisuals.transform.position += positionOffset;
                newVisuals.transform.eulerAngles += rotationOffset;
            }
            else
            {
                foreach (Transform child in newVisuals.transform)
                {
                    if (child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
                    {
                        if (positionOffset != Vector3.zero)
                        {
                            child.transform.position += positionOffset;
                        }
                        if (rotationOffset != Vector3.zero)
                        {
                            child.transform.eulerAngles += rotationOffset;
                        }
                        break;
                    }
                }
            }

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
                    var newmat = UnityEngine.Object.Instantiate(mats[i]);
                    UnityEngine.Object.DontDestroyOnLoad(newmat);
                    mats[i] = newmat;
                }

                skinnedMesh.materials = mats;
            }

            foreach (var mesh in newVisuals.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = mesh.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    var newmat = UnityEngine.Object.Instantiate(mats[i]);
                    UnityEngine.Object.DontDestroyOnLoad(newmat);
                    mats[i] = newmat;
                }

                mesh.materials = mats;
            }

            return newVisuals;
        }

        public static void TryApplyCustomTextures(string texturesFolder, Item item)
        {
            if (Directory.Exists(texturesFolder))
            {
                ApplyTexturesFromFolder(texturesFolder, item);
            }
            else
            {
                Debug.Log("Directory does not exist: " + texturesFolder);
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
                var tex = CustomTextures.LoadTexture(iconPath, CustomTextures.TextureType.Default);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.ItemIcon);
                UnityEngine.Object.DontDestroyOnLoad(sprite);
                At.SetValue(sprite, typeof(Item), item, "m_itemIcon");
            }

            // check for Skill icon (if skill)
            var skillPath = dir + @"\skillicon.png";
            if (item is Skill skill && File.Exists(skillPath))
            {
                var tex = CustomTextures.LoadTexture(skillPath, CustomTextures.TextureType.Default);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.SkillTreeIcon);
                UnityEngine.Object.DontDestroyOnLoad(sprite);
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

                    Texture2D tex;
                    if (name == "_BumpMap" || name == "_NormTex")
                    {
                        tex = CustomTextures.LoadTexture(filepath, CustomTextures.TextureType.Normal);
                    }
                    else if (name == "_GenTex")
                    {
                        tex = CustomTextures.LoadTexture(filepath, CustomTextures.TextureType.GenTex);
                    }
                    else
                    {
                        tex = CustomTextures.LoadTexture(filepath, CustomTextures.TextureType.Default);
                    }

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

            foreach (var layer in CustomTextures.SuffixToShaderLayer.Values)
            {
                var layername = layer.ToString();

                if (mat.HasProperty(layername) && mat.GetTexture(layername) is Texture tex)
                {
                    CustomTextures.SaveTextureAsPNG(tex as Texture2D, dir, layername);                    
                }
            }
        }

        // used internally for managing custom item visuals with the resources prefab manager.
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
