using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace SideLoader
{
    /// <summary>Helper class used to manage custom Item Visuals.</summary>
    public class CustomItemVisuals
    {
        /* 
         * This will need fixing after DLC (Item.ItemVisual not a direct link, only string reference to Resources path)
         * Will probably have to hook something like ResourcesPrefabManager.GetItemVisual(int id) 
        */

        /// <summary> Custom Item Visual prefabs (including retexture-only) </summary>
        private static readonly Dictionary<int, ItemVisualsLink> ItemVisuals = new Dictionary<int, ItemVisualsLink>();        

        // Match anything up to " (" 
        private static readonly Regex materialRegex = new Regex(@".+?(?= \()");

        /// <summary>
        /// Returns the true name of the given material name (removes "(Clone)" and "(Instance)", etc)
        /// </summary>
        public static string GetSafeMaterialName(string origName)
        {
            return materialRegex.Match(origName).Value;
        }

        public static ItemVisualsLink GetOrAddVisualLink(Item item)
        {
            if (!ItemVisuals.ContainsKey(item.ItemID))
            {
                ItemVisuals.Add(item.ItemID, new ItemVisualsLink()
                {
                    LinkedItem = item
                });
            }
            // Set the item visuals dictionary link
            var link = ItemVisuals[item.ItemID];

            return link;
        }

        public static void SetVisualPrefabLink(Item item, GameObject newVisuals, VisualPrefabType type)
        {
            var link = GetOrAddVisualLink(item);
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
        }

        public static void SetSpriteLink(Item item, Sprite sprite, bool skill = false)
        {
            var link = GetOrAddVisualLink(item);

            if (skill)
            {
                link.SkillTreeIcon = sprite;
            }
            else
            {
                link.ItemIcon = sprite;
            }
        }

        [HarmonyPatch(typeof(Item), "GetItemVisual", new Type[] { typeof(bool) })]
        public class Item_GetItemVisuals
        {
            [HarmonyPrefix]
            public static bool Prefix(Item __instance, bool _special, ref Transform __result)
            {
                if (ItemVisuals.ContainsKey(__instance.ItemID))
                {
                    var link = ItemVisuals[__instance.ItemID];
                    if (!_special)
                    {
                        if (link.ItemVisuals)
                        {
                            __result = link.ItemVisuals;
                            return false;
                        }
                    }
                    else
                    {
                        if (__instance.HasSpecialVisualFemalePrefab && link.ItemSpecialFemaleVisuals)
                        {
                            __result = link.ItemSpecialFemaleVisuals;
                            return false;
                        }
                        else if (!__instance.HasSpecialVisualFemalePrefab && link.ItemSpecialVisuals)
                        {
                            __result = link.ItemSpecialVisuals;
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Item), "ItemIcon", MethodType.Getter)]
        public class Item_ItemIcon
        {
            [HarmonyPrefix]
            public static bool Prefix(Item __instance, ref Sprite __result)
            {
                if (ItemVisuals.ContainsKey(__instance.ItemID) && ItemVisuals[__instance.ItemID] is ItemVisualsLink link && link.ItemIcon)
                {
                    __result = link.ItemIcon;
                    return false;
                }

                return true;
            }
        }

        /// <summary>Returns the original Item Visuals for the given Item and VisualPrefabType</summary>
        public static Transform GetOrigItemVisuals(Item item, VisualPrefabType type)
        {
            Transform prefab = null;
            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    prefab = ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.VisualPrefabPath);
                    break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    prefab = ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabDefaultPath);
                    break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    prefab = ResourcesPrefabManager.Instance.GetItemVisualPrefab(item.SpecialVisualPrefabFemalePath);
                    break;
                default:
                    break;
            }
            return prefab;
        }

        /// <summary> Clone's an items current visual prefab (and materials), then sets this item's visuals to the new cloned prefab. </summary>
        public static GameObject CloneVisualPrefab(Item item, VisualPrefabType type, bool logging = false)
        {
            var prefab = GetOrigItemVisuals(item, type);

            if (!prefab)
            {
                if (logging)
                {
                    SL.Log("Error, no VisualPrefabType defined or could not find visual prefab of that type!");
                }
                return null;
            }

            // Clone the visual prefab 
            var newVisuals = UnityEngine.Object.Instantiate(prefab.gameObject);
            newVisuals.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(newVisuals);

            //// set the item's visuals to our new clone
            //At.SetValue(newVisuals.transform, typeof(Item), item, type.ToString());

            // add to our CustomVisualPrefab dictionary
            SetVisualPrefabLink(item, newVisuals, type);

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

        /// <summary>Try apply textures to an item from the specified directory 'texturesFolder'.</summary>
        public static void TryApplyCustomTextures(string texturesFolder, Item item)
        {
            if (Directory.Exists(texturesFolder))
            {
                ApplyTexturesFromFolder(texturesFolder, item);
            }
            else
            {
                SL.Log("Directory does not exist: " + texturesFolder);
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
            Transform prefab = null;
            if (ItemVisuals.ContainsKey(item.ItemID))
            {
                var link = ItemVisuals[item.ItemID];
                switch (type)
                {
                    case VisualPrefabType.VisualPrefab:
                        prefab = link.ItemVisuals; break;
                    case VisualPrefabType.SpecialVisualPrefabDefault:
                        prefab = link.ItemSpecialVisuals; break;
                    case VisualPrefabType.SpecialVisualPrefabFemale:
                        prefab = link.ItemSpecialFemaleVisuals; break;
                }
            }
            if (!prefab)
            {
                prefab = GetOrigItemVisuals(item, type);
            }

            if (prefab)
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
        /// INTERNAL. For applying textures to an item from a given directory.
        /// </summary>
        /// <param name="dir">Full path relative to Outward folder.</param>
        /// <param name="item">The item to apply to.</param>
        private static void ApplyTexturesFromFolder(string dir, Item item)
        {
            // Check for normal item icon
            var iconPath = dir + @"\icon.png";
            if (File.Exists(iconPath))
            {
                var tex = CustomTextures.LoadTexture(iconPath, false, false);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.ItemIcon);
                UnityEngine.Object.DontDestroyOnLoad(sprite);
                At.SetValue(sprite, typeof(Item), item, "m_itemIcon");
                if (item.HasDefaultIcon)
                {
                    At.SetValue("not null", typeof(Item), item, "m_itemIconPath");
                }
                SetSpriteLink(item, sprite, false);
            }

            // check for Skill icon (if skill)
            var skillPath = dir + @"\skillicon.png";
            if (item is Skill skill && File.Exists(skillPath))
            {
                var tex = CustomTextures.LoadTexture(skillPath, false, false);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.SkillTreeIcon);
                UnityEngine.Object.DontDestroyOnLoad(sprite);
                skill.SkillTreeIcon = sprite;
                SetSpriteLink(item, sprite, true);
            }

            // build dictionary of textures per material
            // Key: Material name (Safe), Value: Texture
            var textures = new Dictionary<string, List<Texture2D>>();

            // also keep a dict of the SL_Material templates
            var matHolders = new Dictionary<string, SL_Material>();

            foreach (var subfolder in Directory.GetDirectories(dir))
            {
                var matname = Path.GetFileName(subfolder);

                SL.Log("Reading folder " + matname);

                // check for the SL_Material xml
                Dictionary<string, SL_Material.TextureConfig> texCfgDict = null;
                string matPath = subfolder + @"\properties.xml";
                if (File.Exists(matPath))
                {
                    var matHolder = Serializer.LoadFromXml(matPath) as SL_Material;
                    texCfgDict = matHolder.TextureConfigsToDict();
                    matHolders.Add(matname, matHolder);
                }

                // read the textures
                var texFiles = Directory.GetFiles(subfolder, "*.png");
                if (texFiles.Length > 0)
                {
                    textures.Add(matname, new List<Texture2D>());

                    foreach (var filepath in texFiles)
                    {
                        var name = Path.GetFileNameWithoutExtension(filepath);

                        bool linear = name.Contains("NormTex") || name == "_BumpMap" || name == "_NormalMap";

                        bool mipmap = true;
                        if (texCfgDict != null && texCfgDict.ContainsKey(name))
                        {
                            mipmap = texCfgDict[name].UseMipMap;
                        }

                        var tex = CustomTextures.LoadTexture(filepath, mipmap, linear);
                        tex.name = name;
                        textures[matname].Add(tex);
                    }
                }
            }

            // apply to mats
            for (int i = 0; i < 3; i++)
            {
                var prefabtype = (VisualPrefabType)i;
                var mats = GetMaterials(item, prefabtype);

                if (mats == null || mats.Length < 1)
                {
                    continue;
                }

                foreach (var mat in mats)
                {
                    var matname = GetSafeMaterialName(mat.name);

                    // apply the SL_material template first (set shader, etc)
                    SL_Material matHolder = null;
                    if (matHolders.ContainsKey(matname))
                    {
                        matHolder = matHolders[matname];
                        matHolder.ApplyToMaterial(mat);
                    }
                    else if (!textures.ContainsKey(matname))
                    {
                        continue;
                    }

                    // set actual textures
                    foreach (var tex in textures[matname])
                    {
                        try
                        {
                            if (mat.HasProperty(tex.name))
                            {
                                mat.SetTexture(tex.name, tex);
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

                    // finalize texture settings after they've been applied
                    if (matHolder != null)
                    {
                        matHolder.ApplyTextureSettings(mat);
                    }
                }
            }
        }


        // ******************* FOR SAVING ITEM TEXTURES/MATERIALS *******************

        /// <summary>
        /// Saves textures from an Item to a directory.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dir">Full path, relative to Outward folder</param>
        public static void SaveAllItemTextures(Item item, string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (item.ItemIcon)
            {
                CustomTextures.SaveIconAsPNG(item.ItemIcon, dir, "icon");
            }

            if (item is Skill skill && skill.SkillTreeIcon)
            {
                CustomTextures.SaveIconAsPNG(skill.SkillTreeIcon, dir, "skillicon");
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

            foreach (var texName in mat.GetTexturePropertyNames())
            {
                if (mat.GetTexture(texName) is Texture tex)
                {
                    bool normal = texName.Contains("NormTex") || texName.Contains("BumpMap") || texName == "_NormalMap";

                    CustomTextures.SaveTextureAsPNG(tex as Texture2D, dir, texName, normal);
                }
            }
        }

        // used internally for managing custom item visuals with the resources prefab manager.
        public class ItemVisualsLink
        {
            public Item LinkedItem;
            public SL_Item LinkedTemplate;

            public Sprite ItemIcon;
            public Sprite SkillTreeIcon;

            public Transform ItemVisuals;
            public Transform ItemSpecialVisuals;
            public Transform ItemSpecialFemaleVisuals;
        }
    }
}
