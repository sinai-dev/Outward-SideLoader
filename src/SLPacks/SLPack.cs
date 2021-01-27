using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SideLoader.Helpers;
using SideLoader.Model;
using SideLoader.SaveData;
using SideLoader.Model.Status;
using System.Collections;
using SideLoader.SLPacks;
using SLPackContent = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<string, object>>;

namespace SideLoader
{
    public class SLPack
    {
        /// <summary>The Folder Name of this SLPack</summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Used internally to track where this SL Pack was loaded from.
        /// True = folder is `Outward\Mods\SideLoader\{Name}`. 
        /// False = folder is `Outward\BepInEx\plugins\{Name}\SideLoader\`.
        /// </summary>
        public bool InMainSLFolder { get; internal set; }

        /// <summary>
        /// Returns the folder path for this SL Pack (relative to Outward directory).
        /// </summary>
        public string FolderPath => InMainSLFolder ?
            $@"{SL.SL_FOLDER}\{Name}" :
            $@"{SL.PLUGINS_FOLDER}\{Name}\SideLoader";

        public string GetPathForCategory<T>() where T : SLPackCategory => GetPathForCategory(typeof(T));

        public string GetPathForCategory(Type type)
        {
            if (type == null || !typeof(SLPackCategory).IsAssignableFrom(type))
                throw new ArgumentException("type");

            var instance = SLPackManager.GetCategoryInstance(type);
            if (instance == null)
                throw new Exception($"Trying to get folder path for '{type.FullName}', but category instance is null!");

            return $@"{this.FolderPath}\{instance.FolderName}";
        }

        internal SLPackContent LoadedContent = new SLPackContent();

        public Dictionary<string, object> GetContentForCategory<T>() where T : SLPackCategory
            => GetContentForCategory(typeof(T));

        public Dictionary<string, object> GetContentForCategory(Type type)
        {
            if (type == null || !typeof(SLPackCategory).IsAssignableFrom(type))
                throw new ArgumentException("type");

            if (LoadedContent == null)
                return null;

            LoadedContent.TryGetValue(type, out Dictionary<string, object> ret);

            return ret;
        }

        /// <summary>AssetBundles loaded from the `AssetBundles\` folder. Dictionary Key is the file name.</summary>
        public Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        /// <summary>Texture2Ds loaded from the PNGs in the `Texture2D\` folder (not from the `Items\...` folders). Dictionary Key is the file name (without ".png")</summary>
        public Dictionary<string, Texture2D> Texture2D = new Dictionary<string, Texture2D>();
        /// <summary>AudioClips loaded from the WAV files in the `AudioClip\` folder. Dictionary Key is the file name (without ".wav")</summary>
        public Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();
        /// <summary>SL_Characters loaded from the `Characters\` folder. Dictionary Key is the file name without xml.</summary>
        public Dictionary<string, SL_Character> CharacterTemplates = new Dictionary<string, SL_Character>();

        internal void TryApplyItemTextureBundles()
        {
            var bundlesFolder = $@"{this.FolderPath}\Items\TextureBundles";
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
    }
}
