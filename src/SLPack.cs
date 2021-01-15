using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SideLoader.Helpers;
using SideLoader.Model;
using SideLoader.SaveData;

namespace SideLoader
{
    /// <summary>
    /// Handles internal management of SL Packs (folders which SideLoader will load and apply).
    /// </summary>
    public class SLPack
    {
        #region STATIC

        // used internally to manage assetbundles
        internal static Dictionary<string, AssetBundle> s_allLoadedAssetBundles = new Dictionary<string, AssetBundle>();

        internal static void ApplyAllSLPacks(bool firstSetup)
        {
            // 'BepInEx\plugins\...' packs:
            var directories = Directory.GetDirectories(SL.PLUGINS_FOLDER);
            for (int i = 0; i < directories.Length; i++)
            {
                var dir = directories[i];
                var name = Path.GetFileName(dir);

                var slFolder = dir + @"\SideLoader";
                if (Directory.Exists(slFolder))
                {
                    try
                    {
                        SLPack.TryLoadPack(name, false, !firstSetup);
                    }
                    catch (Exception e)
                    {
                        SL.Log("Exception loading pack: " + slFolder);
                        SL.LogInnerException(e);
                    }
                }
            }

            // 'Mods\SideLoader\...' packs:
            directories = Directory.GetDirectories(SL.SL_FOLDER);
            for (int i = 0; i < directories.Length; i++)
            {
                var dir = directories[i];
                if (dir == SL.GENERATED_FOLDER || dir == SL.INTERNAL_FOLDER || dir == SLSaveManager.SAVEDATA_FOLDER)
                    continue;

                var name = Path.GetFileName(dir);
                try
                {
                    SLPack.TryLoadPack(name, true, !firstSetup);
                }
                catch (Exception e)
                {
                    SL.Log("Exception loading pack: SideLoader\\" + name);
                    SL.LogInnerException(e);
                }
            }
        }

        #endregion

        /// <summary>The FolderName of this SLPack</summary>
        public string Name { get; private set; }

        /// <summary>
        /// Used internally to track where this SL Pack was loaded from.
        /// True = folder is `Outward\Mods\SideLoader\{Name}`. 
        /// False = folder is `Outward\BepInEx\plugins\{Name}\SideLoader\`.
        /// </summary>
        public bool InMainSLFolder = false;

        /// <summary>
        /// Returns the folder path for this SL Pack (relative to Outward directory).
        /// </summary>
        public string FolderPath => InMainSLFolder ?
            $@"{SL.SL_FOLDER}\{Name}" :
            $@"{SL.PLUGINS_FOLDER}\{Name}\SideLoader";

        /// <summary>AssetBundles loaded from the `AssetBundles\` folder. Dictionary Key is the file name.</summary>
        public Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        /// <summary>Texture2Ds loaded from the PNGs in the `Texture2D\` folder (not from the `Items\...` folders). Dictionary Key is the file name (without ".png")</summary>
        public Dictionary<string, Texture2D> Texture2D = new Dictionary<string, Texture2D>();
        /// <summary>AudioClips loaded from the WAV files in the `AudioClip\` folder. Dictionary Key is the file name (without ".wav")</summary>
        public Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();
        
        /// <summary>SL_Characters loaded from the `Characters\` folder. Dictionary Key is the SL_Character.UID value.</summary>
        public Dictionary<string, SL_Character> CharacterTemplates = new Dictionary<string, SL_Character>();

        /// <summary>
        /// The supported sub-folders in an SL Pack. 
        /// </summary>
        public enum SubFolders
        {
            AudioClip,
            AssetBundles,
            Characters,
            Enchantments,
            Items,
            Recipes,
            StatusEffects,
            StatusFamilies,
            Texture2D,
        }

        /// <summary>
        /// Returns the full (relative to the Outward folder) path for the specified subfolder, for this SLPack. Eg, "Mods/SideLoader/SLPACKNAME/SubFolder"
        /// </summary>
        /// <param name="subFolder">The SubFolder you want the path for</param>
        public string GetSubfolderPath(SubFolders subFolder)
        {
            return $@"{this.FolderPath}\{subFolder}";
        }

        /// <summary>
        /// Safely tries to load an SLPack with the provided name, either in the Mods\SideLoader\ folder or the BepInEx\plugins\ folder.
        /// </summary>
        /// <param name="name">The name of the SLPack folder.</param>
        /// <param name="inMainFolder">Is it in the Mods\SideLoader\ directory? (If not, it should be in BepInEx\plugins\)</param>
        /// <param name="hotReload">Is this a hot reload?</param>
        public static void TryLoadPack(string name, bool inMainFolder, bool hotReload)
        {
            try
            {
                if (SL.Packs.ContainsKey(name))
                {
                    SL.LogError($"ERROR: An SLPack already exists with the name '{name}'! Please use a unique name.");
                    return;
                }

                var pack = LoadFromFolder(name, inMainFolder, hotReload);
                SL.Packs.Add(pack.Name, pack);
            }
            catch (Exception e)
            {
                SL.LogError("Error loading SLPack from folder: " + name + "\r\nMessage: " + e.Message + "\r\nStackTrace: " + e.StackTrace);
            }
        }

        /// <summary>
        /// Loads all the assets from the specified SLPack name. Not for calling directly, just place your pack in the SideLoader folder and use SL.Packs["Folder"]
        /// </summary>
        /// <param name="name">The name of the SideLoader pack (ie. the name of the folder inside Mods/SideLoader/)</param>
        /// <param name="inMainSLFolder">Is the SLPack in Mods\SideLoader? If not, it should be Mods\ModName\SideLoader\ structure.</param>
        /// <param name="hotReload">Is this a hot reload?</param>
        private static SLPack LoadFromFolder(string name, bool inMainSLFolder, bool hotReload)
        {
            var pack = new SLPack()
            {
                Name = name,
                InMainSLFolder = inMainSLFolder
            };

            SL.Log("Reading SLPack " + pack.Name);

            // order is somewhat important.
            
            if (!hotReload)
                pack.LoadAssetBundles();

            pack.LoadAudioClips();
            pack.LoadTexture2D();

            pack.LoadStatusFamilies();
            pack.LoadCustomStatuses();

            pack.LoadCustomItems();

            pack.LoadRecipes();

            if (!hotReload)
            {
                pack.LoadCharacters();
                pack.LoadEnchantments();
            }

            return pack;
        }

        private void LoadAssetBundles()
        {
            var dir = GetSubfolderPath(SubFolders.AssetBundles);
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (var bundlePath in Directory.GetFiles(GetSubfolderPath(SubFolders.AssetBundles))
                                                .Where(x => !x.EndsWith(".meta") 
                                                         && !x.EndsWith(".manifest")))
            {
                try
                {
                    if (s_allLoadedAssetBundles.ContainsKey(bundlePath))
                    {
                        if (s_allLoadedAssetBundles[bundlePath])
                            s_allLoadedAssetBundles[bundlePath].Unload(true);

                        s_allLoadedAssetBundles.Remove(bundlePath);
                    }

                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (bundle is AssetBundle)
                    {
                        string name = Path.GetFileName(bundlePath);
                        AssetBundles.Add(name, bundle);
                        s_allLoadedAssetBundles.Add(bundlePath, bundle);
                        SL.Log("Loaded assetbundle " + name);
                    }
                    else
                        throw new Exception($"Unknown error (Bundle '{Path.GetFileName(bundlePath)}' was null)");
                }
                catch (Exception e)
                {
                    SL.LogError("Error loading asset bundle! Message: " + e.Message + "\r\nStack: " + e.StackTrace);
                }
            }
        }

        private void LoadAudioClips()
        {
            var dir = GetSubfolderPath(SubFolders.AudioClip);
            if (!Directory.Exists(dir))
                return;

            foreach (var clipPath in Directory.GetFiles(dir, "*.wav"))
            {
                SLPlugin.Instance.StartCoroutine(CustomAudio.LoadClip(clipPath, this));
            }
        }

        // Note: only loads textures in the Texture2D folder.
        private void LoadTexture2D()
        {
            if (!Directory.Exists(GetSubfolderPath(SubFolders.Texture2D)))
                return;

            var dir = GetSubfolderPath(SubFolders.Texture2D);
            foreach (var texPath in Directory.GetFiles(dir, "*.png"))
            {
                LoadTexture(texPath, true);

                var localDir = dir + @"\Local";
                if (Directory.Exists(localDir))
                {
                    foreach (var localTex in Directory.GetFiles(localDir, "*.png"))
                        LoadTexture(localTex, false);
                }
            }
        }

        private void LoadTexture(string texPath, bool addGlobal)
        {
            var texture = CustomTextures.LoadTexture(texPath, false, false);
            var name = Path.GetFileNameWithoutExtension(texPath);

            // add to the Texture2D dict for this pack
            if (Texture2D.ContainsKey(name))
                SL.LogWarning("Trying to load two textures with the same name into the same SLPack: " + name);
            else
                Texture2D.Add(name, texture);

            if (addGlobal)
            {
                // add to the global Tex replacements dict
                if (CustomTextures.Textures.ContainsKey(name))
                {
                    SL.Log("Custom Texture2Ds: A Texture already exists in the global list called " + name + "! Overwriting with this one...");
                    CustomTextures.Textures[name] = texture;
                }
                else
                    CustomTextures.Textures.Add(name, texture);
            }
        }

        private void LoadStatusFamilies()
        {
            var dir = GetSubfolderPath(SubFolders.StatusFamilies);
            if (!Directory.Exists(dir))
                return;

            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*.xml"))
                {
                    if (Serializer.LoadFromXml(file) is SL_StatusEffectFamily template)
                        template.Apply();
                }
            }
        }

        private void LoadCustomStatuses()
        {
            var dir = GetSubfolderPath(SubFolders.StatusEffects);
            if (!Directory.Exists(dir))
                return;

            // Key: Filepath, Value: Subfolder name (if any)
            var dict = new Dictionary<string, string>();

            // get basic template xmls
            foreach (var path in Directory.GetFiles(dir, "*.xml"))
                dict.Add(path, "");

            // get subfolder-per-status
            foreach (var folder in Directory.GetDirectories(dir))
            {
                // get the xml inside this folder
                foreach (string path in Directory.GetFiles(folder, "*.xml"))
                    dict.Add(path, Path.GetFileName(folder));
            }

            // apply templates
            foreach (var entry in dict)
            {
                var template = Serializer.LoadFromXml(entry.Key);

                if (template is SL_StatusEffect statusTemplate)
                {
                    statusTemplate.SLPackName = Name;
                    statusTemplate.SubfolderName = entry.Value;
                    statusTemplate.Apply();                 
                }
                else if (template is SL_ImbueEffect imbueTemplate)
                {
                    imbueTemplate.SLPackName = Name;
                    imbueTemplate.SubfolderName = entry.Value;
                    imbueTemplate.Apply();
                }
                else
                {
                    SL.LogError("Unrecognized status effect template: " + entry.Key);
                }
            }
        }

        private void LoadCustomItems()
        {
            var itemsfolder = GetSubfolderPath(SubFolders.Items);

            if (Directory.Exists(itemsfolder))
            {
                // ******** Build the list of template xml paths ******** //

                // Key: full Template filepath, Value:SubFolder name (if any)
                var templates = new Dictionary<string, string>(); 

                // get basic xml templates in the Items folder
                foreach (var path in Directory.GetFiles(itemsfolder, "*.xml"))
                    templates.Add(path, "");

                // check for subfolders (items which are using custom texture pngs)
                foreach (var folder in Directory.GetDirectories(itemsfolder))
                {
                    if (Path.GetFileName(folder) == "TextureBundles")
                    {
                        // folder used to load bulk textures for items, continue for now
                        continue;
                    }

                    //SL.Log("Parsing CustomItem subfolder: " + Path.GetFileName(folder));

                    foreach (string path in Directory.GetFiles(folder, "*.xml"))
                        templates.Add(path, Path.GetFileName(folder));
                }

                // ******** Serialize and prepare each template (does not apply the template, but does clone the base prefab) ******** //

                foreach (var entry in templates)
                {
                    try
                    {
                        // load the ItemHolder template and set the pack/folder info
                        var holder = Serializer.LoadFromXml(entry.Key);

                        var list = new List<SL_Item>();

                        if (holder is SL_Item)
                            list.Add(holder as SL_Item);
                        else
                            list.AddRange((holder as SL_MultiItem).Items);

                        foreach (var itemHolder in list)
                        {
                            itemHolder.SubfolderName = entry.Value;
                            itemHolder.SLPackName = Name;
                            itemHolder.Apply();
                        }
                    }
                    catch (Exception e)
                    {
                        SL.Log("Error loading custom item! " + e.GetType() + ", " + e.Message);
                        while (e != null)
                        {
                            SL.Log(e.ToString());
                            e = e.InnerException;
                        }
                    }
                }
            }
        }

        public void TryApplyItemTextureBundles()
        {
            var itemsFolder = this.GetSubfolderPath(SubFolders.Items);
            var bundlesFolder = $@"{itemsFolder}\TextureBundles";
            if (Directory.Exists(bundlesFolder))
            {
                foreach (var file in Directory.GetFiles(bundlesFolder))
                {
                    if (AssetBundle.LoadFromFile(file) is AssetBundle bundle)
                    {
                        CustomItemVisuals.ApplyTexturesFromAssetBundle(bundle);
                    }
                }
            }
        }

        private void LoadRecipes()
        {
            var path = GetSubfolderPath(SubFolders.Recipes);

            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var recipePath in Directory.GetFiles(path))
            {
                if (Serializer.LoadFromXml(recipePath) is SL_Recipe recipeHolder)
                    SL.PendingRecipes.Add(recipeHolder);
            }
        }

        private void LoadCharacters()
        {
            var path = GetSubfolderPath(SubFolders.Characters);

            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var filePath in Directory.GetFiles(path))
            {
                if (Serializer.LoadFromXml(filePath) is SL_Character template)
                {
                    //SL.Log("Serialized SL_Character " + template.Name + " (" + template.UID + ")");

                    template.SLPackName = this.Name;
                    CharacterTemplates.Add(template.UID, template);
                    SL.PendingCharacters.Add(template);
                }
            }
        }

        private void LoadEnchantments()
        {
            var dir = GetSubfolderPath(SubFolders.Enchantments);
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (var filePath in Directory.GetFiles(GetSubfolderPath(SubFolders.Enchantments)))
            {
                try
                {
                    if (Serializer.LoadFromXml(filePath) is SL_EnchantmentRecipe template)
                        template.Apply();
                }
                catch
                {
                    SL.Log($"Exception loading Enchantment from {filePath}!");
                }
            }
        }
    }
}
