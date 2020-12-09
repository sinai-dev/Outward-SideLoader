using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using HarmonyLib;
using Steamworks;

namespace SideLoader
{
    /// <summary>SideLoader's manger class for Custom Item Visuls. Contains useful methods for managing item visuals.</summary>
    public class CustomItemVisuals
    {
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

        public static ItemVisualsLink GetItemVisualLink(Item item)
        {
            if (ItemVisuals.ContainsKey(item.ItemID))
            {
                return ItemVisuals[item.ItemID];
            }
            return null;
        }

        public static ItemVisualsLink GetOrAddVisualLink(Item item)
        {
            if (!ItemVisuals.ContainsKey(item.ItemID))
            {
                ItemVisuals.Add(item.ItemID, new ItemVisualsLink());
            }

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

        /// <summary>
        /// Helper to set the ItemVisualLink Icon or SkillTreeIcon for an Item.
        /// </summary>
        /// <param name="item">The item you want to set to.</param>
        /// <param name="sprite">The Sprite you want to set.</param>
        /// <param name="skill">Whether this is a "small skill tree icon", or just the main item icon.</param>
        public static void SetSpriteLink(Item item, Sprite sprite, bool skill = false)
        {
            var link = GetOrAddVisualLink(item);

            if (skill)
            {
                (item as Skill).SkillTreeIcon = sprite;
                link.SkillTreeIcon = sprite;
            }
            else
            {
                At.SetValue(sprite, "m_itemIcon", item);

                if (item.HasDefaultIcon)
                    At.SetValue("notnull", "m_itemIconPath", item);

                link.ItemIcon = sprite;
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

            return CloneVisualPrefab(item, prefab.gameObject, type, logging);
        }

        /// <summary>
        /// Clones the provided 'prefab' GameObject, and sets it to the provided Item and VisualPrefabType.
        /// </summary>
        /// <param name="item">The Item to apply to.</param>
        /// <param name="prefab">The visual prefab to clone and set.</param>
        /// <param name="type">The Type of VisualPrefab you are setting.</param>
        /// <param name="logging">Whether to log errors or not.</param>
        /// <returns>The cloned gameobject.</returns>
        public static GameObject CloneVisualPrefab(Item item, GameObject prefab, VisualPrefabType type, bool logging = false)
        {
            // Clone the visual prefab 
            var newVisuals = UnityEngine.Object.Instantiate(prefab.gameObject);
            newVisuals.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(newVisuals);

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
                SL.Log("Trying to CheckCustomTextures for " + newItem.Name + " but either SLPackName or SubfolderName is not set!");
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
        /// Applies textures and icons to the item from the given directory.
        /// The icons should be in the base folder, called "icon.png" and "skillicon.png".
        /// The textures should be in sub-folders for each material (name of folder is material name), and each texture should be named after the shader layer it is setting.
        /// </summary>
        /// <param name="dir">Full path relative to Outward folder.</param>
        /// <param name="item">The item to apply to.</param>
        private static void ApplyTexturesFromFolder(string dir, Item item)
        {
            var sprites = GetIconsFromFolder(dir);
            ApplyIconsByName(sprites, item);

            var textures = GetTexturesFromFolder(dir, out Dictionary<string, SL_Material> slMaterials);
            ApplyTexturesByName(textures, slMaterials, item);
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

        // ========= asset bundle item textures =========

        /// <summary>
        /// Searches the provided AssetBundle for folders in the expected format, and applies textures to the corresponding Item.
        /// Each item must have its own sub-folder, where the name of this folder starts with the Item's ItemID.
        /// The folder name can have anything else after the ID, but it must start with the ID.
        /// Eg., '2000010_IronSword\' would be valid to set the textures on the Iron Sword. 
        /// The textures should be placed inside this folder and should match the Shader Layer names of the texture (the same way you set Item textures from a folder).
        /// </summary>
        /// <param name="bundle">The AssetBundle to apply Textures from.</param>
        public static void ApplyTexturesFromAssetBundle(AssetBundle bundle)
        {
            // Keys: Item IDs
            // Values: List of Textures/Sprites to apply to the item
            // The ItemTextures Value Key is the Material name, and the List<Texture2D> are the actual textures.
            var itemTextures = new Dictionary<int, Dictionary<string, List<Texture2D>>>();
            var icons = new Dictionary<int, List<Sprite>>();

            string[] names = bundle.GetAllAssetNames();
            foreach (var name in names)
            {
                try
                {
                    SL.Log("Loading texture from AssetBundle, path: " + name);

                    Texture2D tex = bundle.LoadAsset<Texture2D>(name);

                    // cleanup the name (remove ".png")
                    tex.name = tex.name.Replace(".png", "");

                    // Split the assetbundle path by forward slashes
                    string[] splitPath = name.Split('/');

                    // Get the ID from the first 7 characters of the path
                    int id = int.Parse(splitPath[1].Substring(0, 7));

                    // Identify icons by name
                    if (tex.name.Contains("icon"))
                    {
                        if (!icons.ContainsKey(id))
                        {
                            icons.Add(id, new List<Sprite>());
                        }

                        Sprite sprite;
                        if (tex.name.Contains("skill"))
                        {
                            sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.SkillTreeIcon);
                        }
                        else
                        {
                            sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.ItemIcon);
                        }
                        icons[id].Add(sprite);
                    }
                    else // is not an icon
                    {
                        var mat = "";
                        if (splitPath[2] == "textures")
                        {
                            mat = splitPath[3];
                        }
                        else
                        {
                            mat = splitPath[2];
                        }

                        if (!itemTextures.ContainsKey(id))
                        {
                            itemTextures.Add(id, new Dictionary<string, List<Texture2D>>());
                        }
                        if (!itemTextures[id].ContainsKey(mat))
                        {
                            itemTextures[id].Add(mat, new List<Texture2D>());
                        }

                        itemTextures[id][mat].Add(tex);
                    }
                }
                catch (InvalidCastException) 
                {
                    // suppress
                }
                catch (Exception ex)
                {
                    SL.Log("Exception loading textures from asset bundle!");
                    SL.Log(ex.Message);
                    SL.Log(ex.StackTrace);
                }
            }

            // Apply material textures
            foreach (var entry in itemTextures)
            {
                if (ResourcesPrefabManager.Instance.GetItemPrefab(entry.Key) is Item item)
                {
                    ApplyTexturesByName(entry.Value, null, item);
                }
            }

            // Apply icons
            foreach (var entry in icons)
            {
                if (ResourcesPrefabManager.Instance.GetItemPrefab(entry.Key) is Item item)
                {
                    ApplyIconsByName(entry.Value.ToArray(), item);
                }
            }
        }

        // ====== getting textures from folder ===========

        public static Sprite[] GetIconsFromFolder(string dir)
        {
            List<Sprite> list = new List<Sprite>();
            // Check for normal item icon
            var iconPath = dir + @"\icon.png";
            if (File.Exists(iconPath))
            {
                var tex = CustomTextures.LoadTexture(iconPath, false, false);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.ItemIcon);
                UnityEngine.Object.DontDestroyOnLoad(sprite);
                list.Add(sprite);
                sprite.name = "icon";
            }

            // check for Skill icon (if skill)
            var skillPath = dir + @"\skillicon.png";
            if (File.Exists(skillPath))
            {
                var tex = CustomTextures.LoadTexture(skillPath, false, false);
                var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.SkillTreeIcon);
                UnityEngine.Object.DontDestroyOnLoad(sprite);
                list.Add(sprite);
                sprite.name = "skillicon";
            }

            return list.ToArray();
        }

        /// <summary>
        /// Checks the provided folder for sub-folders, each sub-folder should be the name of a material.
        /// Inside this folder there should be the texture PNG files (named after Shader Layers), and the properties.xml file.
        /// SideLoader will load everything and return it to you in two dictionaries.
        /// </summary>
        /// <param name="dir">The base directory to check (eg. "SLPack\Items\MyItem\Textures\")</param>
        /// <param name="slMaterials">Secondary out paramater for the SL Material templates. Key: Material Name, Value: SL_Material.</param>
        /// <returns>Key: Material name, Value: List of Texture2D for the material.</returns>
        public static Dictionary<string, List<Texture2D>> GetTexturesFromFolder(string dir, out Dictionary<string, SL_Material> slMaterials)
        {
            // build dictionary of textures per material
            // Key: Material name (Safe), Value: Texture
            var textures = new Dictionary<string, List<Texture2D>>();

            // also keep a dict of the SL_Material templates
            slMaterials = new Dictionary<string, SL_Material>();

            foreach (var subfolder in Directory.GetDirectories(dir))
            {
                var matname = Path.GetFileName(subfolder).ToLower();

                SL.Log("Reading folder " + matname);

                // check for the SL_Material xml
                Dictionary<string, SL_Material.TextureConfig> texCfgDict = null;
                string matPath = subfolder + @"\properties.xml";
                if (File.Exists(matPath))
                {
                    var matHolder = Serializer.LoadFromXml(matPath) as SL_Material;
                    texCfgDict = matHolder.TextureConfigsToDict();
                    slMaterials.Add(matname, matHolder);
                }

                // read the textures
                var texFiles = Directory.GetFiles(subfolder, "*.png");
                if (texFiles.Length > 0)
                {
                    textures.Add(matname, new List<Texture2D>());

                    foreach (var filepath in texFiles)
                    {
                        var name = Path.GetFileNameWithoutExtension(filepath);

                        var check = name.ToLower();
                        bool linear = check.Contains("normtex") || check == "_bumpmap" || check == "_normalmap";

                        bool mipmap = true;
                        if (texCfgDict != null && texCfgDict.ContainsKey(name))
                        {
                            mipmap = texCfgDict[name].UseMipMap;
                        }

                        // at this point we can safely turn it lower case for compatibility going forward
                        name = name.ToLower();

                        var tex = CustomTextures.LoadTexture(filepath, mipmap, linear);
                        tex.name = name;
                        textures[matname].Add(tex);
                    }
                }
            }

            return textures;
        }

        // ========= setting textures on item =========

        /// <summary>
        /// Sets the provided sprites to the item. The list (of 1 or 2 length) should contain either/or: the main item icon called "icon", and the skill tree icon called "skillicon".
        /// </summary>
        /// <param name="icons">A list of 1 or 2 length. Item icons should be called "icon", and skill tree icons should be called "skillicon".</param>
        /// <param name="item">The item to set to.</param>
        public static void ApplyIconsByName(Sprite[] icons, Item item)
        {
            foreach (var sprite in icons)
            {
                if (sprite.name.ToLower() == "icon")
                {
                    SetSpriteLink(item, sprite, false);
                }
                else if (sprite.name.ToLower() == "skillicon")
                {
                    SetSpriteLink(item, sprite, true);
                }
            }
        }

        /// <summary>
        /// Applies textures to the item using the provided dictionary.
        /// </summary>
        /// <param name="textures">Key: Material names (with GetSafeMaterialName), Value: List of Textures to apply, names should match the shader layers of the material.</param>
        /// <param name="slMaterials">[OPTIONAL] Key: Material names with GetSafeMaterialName, Value: SL_Material template to apply.</param>
        /// <param name="item">The item to apply to</param>
        public static void ApplyTexturesByName(Dictionary<string, List<Texture2D>> textures, Dictionary<string, SL_Material> slMaterials, Item item)
        {
            if (slMaterials == null)
            {
                slMaterials = new Dictionary<string, SL_Material>();
            }

            // apply to mats
            for (int i = 0; i < 3; i++)
            {
                var prefabtype = (VisualPrefabType)i;

                if (!ItemVisuals.ContainsKey(item.ItemID) || !ItemVisuals[item.ItemID].GetVisuals(prefabtype))
                {
                    var prefab = CloneVisualPrefab(item, prefabtype, false);
                    if (!prefab)
                    {
                        continue;
                    }
                }

                var mats = GetMaterials(item, prefabtype);

                if (mats == null || mats.Length < 1)
                {
                    continue;
                }

                foreach (var mat in mats)
                {
                    var matname = GetSafeMaterialName(mat.name).ToLower();

                    // apply the SL_material template first (set shader, etc)
                    SL_Material matHolder = null;
                    if (slMaterials.ContainsKey(matname))
                    {
                        matHolder = slMaterials[matname];
                        matHolder.ApplyToMaterial(mat);
                    }
                    else if (!textures.ContainsKey(matname))
                    {
                        continue;
                    }

                    // build list of actual shader layer names.
                    // Key: ToLower(), Value: original.
                    Dictionary<string, string> layersToLower = new Dictionary<string, string>();
                    foreach (var layer in mat.GetTexturePropertyNames())
                    {
                        layersToLower.Add(layer.ToLower(), layer);
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
                            else if (layersToLower.ContainsKey(tex.name))
                            {
                                var realname = layersToLower[tex.name];
                                mat.SetTexture(realname, tex);
                                SL.Log("Set texture " + realname + " on " + matname);
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
        /// <param name="item">The item to apply to.</param>
        /// <param name="dir">Full path, relative to Outward folder</param>
        public static void SaveAllItemTextures(Item item, string dir)
        {
            SL.Log("Saving item textures...");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                Sprite icon = At.GetValue("m_itemIcon", item) as Sprite;
                if (!icon)
                    icon = ResourcesPrefabManager.Instance.GetItemIcon(item);
                if (!icon)
                    icon = (Sprite)At.GetValue("DefaultIcon", item);

                CustomTextures.SaveIconAsPNG(icon, dir, "icon");

            }
            catch (Exception e)
            {
                SL.Log(e.ToString());
            }

            if (item is Skill skill && skill.SkillTreeIcon)
                CustomTextures.SaveIconAsPNG(skill.SkillTreeIcon, dir, "skillicon");

            for (int i = 0; i < 3; i++)
            {
                SL.Log("Checking materials (" + ((VisualPrefabType)i) + ")");

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
                Directory.CreateDirectory(dir);

            foreach (var texName in mat.GetTexturePropertyNames())
            {
                if (mat.GetTexture(texName) is Texture tex)
                {
                    bool normal = texName.Contains("NormTex") || texName.Contains("BumpMap") || texName == "_NormalMap";

                    CustomTextures.SaveTextureAsPNG(tex as Texture2D, dir, texName, normal);
                }
            }
        }

        /// <summary>
        /// Used internally for managing custom item visuals for the ResourcesPrefabManager.
        /// </summary>
        public class ItemVisualsLink
        {
            /// <summary>
            /// Returns the linked ItemVisuals for the provided VisualPrefabType (if any), otherwise null.
            /// </summary>
            /// <param name="type">The type of Visual Prefab you want.</param>
            /// <returns>The linked Transform, or null.</returns>
            public Transform GetVisuals(VisualPrefabType type)
            {
                switch (type)
                {
                    case VisualPrefabType.VisualPrefab: return ItemVisuals; 
                    case VisualPrefabType.SpecialVisualPrefabDefault: return ItemSpecialVisuals;
                    case VisualPrefabType.SpecialVisualPrefabFemale: return ItemSpecialFemaleVisuals;
                    default: return null;
                }
            }

            public Sprite ItemIcon;
            public Sprite SkillTreeIcon;

            public Transform ItemVisuals;
            public Transform ItemSpecialVisuals;
            public Transform ItemSpecialFemaleVisuals;
        }
    }
}
