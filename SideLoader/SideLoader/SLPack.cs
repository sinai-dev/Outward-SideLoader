using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader
{
    public class SLPack
    {
        /// <summary>The FolderName of this SLPack</summary>
        public string Name { get; private set; }

        /// <summary>
        /// True = folder is in <b>Outward\Mods\SideLoader\</b>. False = folder is in <b>Outward\BepInEx\plugins\MyPack\SideLoader\</b>.
        /// </summary>
        public bool InMainSLFolder = false;

        public string FolderPath 
        { 
            get 
            {
                return InMainSLFolder ?
                    $@"{SL.SL_FOLDER}\{Name}" :
                    $@"{SL.PLUGINS_FOLDER}\{Name}\SideLoader";
            } 
        }

        public Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        public Dictionary<string, Texture2D> Texture2D = new Dictionary<string, Texture2D>();
        public Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();

        public Dictionary<string, SL_Character> CharacterTemplates = new Dictionary<string, SL_Character>();

        public enum SubFolders
        {
            AudioClip,
            AssetBundles,
            Characters,
            Enchantments,
            Items,
            Recipes,
            StatusEffects,
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
        public static void TryLoadPack(string name, bool inMainFolder)
        {
            try
            {
                if (SL.Packs.ContainsKey(name))
                {
                    SL.Log($"ERROR: An SLPack already exists with the name '{name}'! Please use a unique name.", 1);
                    return;
                }

                var pack = LoadFromFolder(name, inMainFolder);
                SL.Packs.Add(pack.Name, pack);
            }
            catch (Exception e)
            {
                SL.Log("Error loading SLPack from folder: " + name + "\r\nMessage: " + e.Message + "\r\nStackTrace: " + e.StackTrace, 1);
            }
        }

        /// <summary>
        /// Loads all the assets from the specified SLPack name. Not for calling directly, just place your pack in the SideLoader folder and use SL.Packs["Folder"]
        /// </summary>
        /// <param name="name">The name of the SideLoader pack (ie. the name of the folder inside Mods/SideLoader/)</param>
        /// <param name="inMainSLFolder">Is the SLPack in Mods\SideLoader? If not, it should be Mods\ModName\SideLoader\ structure.</param>
        private static SLPack LoadFromFolder(string name, bool inMainSLFolder = true)
        {
            var pack = new SLPack()
            {
                Name = name,
                InMainSLFolder = inMainSLFolder
            };

            SL.Log("Reading SLPack " + pack.Name);

            // AssetBundles
            pack.LoadAssetBundles();

            // Audio Clips
            pack.LoadAudioClips();

            // Texture2D (Just for replacements)
            pack.LoadTexture2D();

            // Status Effect and Imbue Presets
            pack.LoadCustomStatuses();

            // Custom Items
            pack.LoadCustomItems();

            // Custom Recipes
            pack.LoadRecipes();

            // Character spawn callbacks
            pack.LoadCharacters();

            // Custom Enchantments
            pack.LoadEnchantments();

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
                    var bundle = SL.LoadAssetBundle(bundlePath);
                    if (bundle is AssetBundle)
                    {
                        string name = Path.GetFileName(bundlePath);
                        AssetBundles.Add(name, bundle);
                        SL.Log("Loaded assetbundle " + name);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unknown error (Bundle '{0}' was null)", Path.GetFileName(bundlePath)));
                    }
                }
                catch (Exception e)
                {
                    SL.Log("Error loading asset bundle! Message: " + e.Message + "\r\nStack: " + e.StackTrace, 1);
                }
            }
        }

        private void LoadAudioClips()
        {
            var dir = GetSubfolderPath(SubFolders.AudioClip);
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (var clipPath in Directory.GetFiles(dir, "*.wav"))
            {
                SL.Instance.StartCoroutine(CustomAudio.LoadClip(clipPath, this));
            }
        }

        // Note: Does NOT load Pngs from the CustomItems/*/Textures/ folders
        // That is done on CustomItem.ApplyTemplateToItem, those textures are not stored in the dictionary.
        private void LoadTexture2D()
        {
            if (!Directory.Exists(GetSubfolderPath(SubFolders.Texture2D)))
            {
                return;
            }

            foreach (var texPath in Directory.GetFiles(GetSubfolderPath(SubFolders.Texture2D)))
            {
                var texture = CustomTextures.LoadTexture(texPath, false, false);
                var name = Path.GetFileNameWithoutExtension(texPath);

                // add to the Texture2D dict for this pack
                Texture2D.Add(name, texture);

                // add to the global Tex replacements dict
                if (CustomTextures.Textures.ContainsKey(name))
                {
                    SL.Log("CustomTextures: A Texture already exists in the global list called " + name + "! Overwriting with this one...");
                    CustomTextures.Textures[name] = texture;
                }
                else
                {
                    CustomTextures.Textures.Add(name, texture);
                }
            }
        }

        private void LoadCustomStatuses()
        {
            var dir = GetSubfolderPath(SubFolders.StatusEffects);
            if (!Directory.Exists(dir))
            {
                return;
            }

            // Key: Filepath, Value: Subfolder name (if any)
            var dict = new Dictionary<string, string>();

            // get basic template xmls
            foreach (var path in Directory.GetFiles(dir, "*.xml"))
            {
                dict.Add(path, "");
            }

            // get subfolder-per-status
            foreach (var folder in Directory.GetDirectories(dir))
            {
                // get the xml inside this folder
                foreach (string path in Directory.GetFiles(folder, "*.xml"))
                {
                    dict.Add(path, Path.GetFileName(folder));
                }
            }

            // apply templates
            foreach (var entry in dict)
            {
                var template = Serializer.LoadFromXml(entry.Key);

                if (template is SL_StatusEffect statusTemplate)
                {
                    CustomStatusEffects.CreateCustomStatus(statusTemplate);
                    statusTemplate.SLPackName = Name;
                    statusTemplate.SubfolderName = entry.Value;
                }
                else if (template is SL_ImbueEffect imbueTemplate)
                {
                    CustomStatusEffects.CreateCustomImbue(imbueTemplate);
                    imbueTemplate.SLPackName = Name;
                    imbueTemplate.SubfolderName = entry.Value;
                }
                else
                {
                    SL.Log("Unrecognized status effect template: " + entry.Key, 1);
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
                {
                    templates.Add(path, "");
                }

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
                    {
                        templates.Add(path, Path.GetFileName(folder));
                    }
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
                        {
                            list.Add(holder as SL_Item);
                        }
                        else
                        {
                            list.AddRange((holder as SL_MultiItem).Items);
                        }

                        foreach (var itemHolder in list)
                        {
                            itemHolder.SubfolderName = entry.Value;
                            itemHolder.SLPackName = Name;

                            // Clone the target item. This also adds a callback for itemHolder.ApplyTemplateToItem
                            var item = CustomItems.CreateCustomItem(itemHolder);
                        }
                    }
                    catch (Exception e)
                    {
                        SL.Log("LoadFromFolder: Error creating custom item! \r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace);
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
                    if (SL.LoadAssetBundle(file) is AssetBundle bundle)
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
                {
                    SL.INTERNAL_ApplyRecipes += recipeHolder.ApplyRecipe;
                }
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
                    CharacterTemplates.Add(template.UID, template);

                    template.Prepare();
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
                    {
                        template.Apply();
                    }
                }
                catch
                {
                    SL.Log($"Exception loading Enchantment from {filePath}!");
                }
            }
        }
    }
}
