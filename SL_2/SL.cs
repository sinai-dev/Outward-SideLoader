using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Reflection;

namespace SideLoader_2
{
    /// <summary> The main SideLoader class. </summary>
    public class SL : MonoBehaviour
    {
        //public static SL Instance;

        // Mod Info
        public static readonly string MODNAME = "SideLoader";
        public static readonly string VERSION = "2.0.0";

        // Folders
        public static readonly string SL_FOLDER = @"Mods\SideLoader_2";

        // Loaded SLPacks
        public static Dictionary<string, SLPack> Packs = new Dictionary<string, SLPack>();
        public static bool PacksLoaded { get; private set; } = false;

        // Loaded AssetBundles
        public static Dictionary<string, AssetBundle> CachedBundles = new Dictionary<string, AssetBundle>();

        // Events
        /// <summary>Only called once on startup. This will be after ResourcesPrefabManager is loaded, and all SLPacks are loaded and applied.</summary>
        public static event UnityAction OnPacksLoaded;
        /// <summary>Use this to safely make changes to a scene when it is truly loaded. (All players loaded, gameplay may not yet be resumed).</summary>
        public static event UnityAction OnSceneLoaded;

        // Internal Events
        /// <summary>Only called once on startup. This is mainly for internal use, it is a callback used by ItemHolders to apply after all assets are loaded.</summary>
        public static event UnityAction INTERNAL_ApplyItems;
        /// <summary>Only called once on startup. This is mainly for internal use, it is a callback used by RecipeHolders to apply after all CustomItems are loaded.</summary>
        public static event UnityAction INTERNAL_ApplyRecipes;

        internal void Awake()
        {
            //// Currently don't actually have a use for SL.Instance. Removing for now.
            //Instance = this;
        }

        // ================ Main Setup ====================

        internal void Start()
        {
            /* subscribe to SceneLoaded event */
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            /* Add non-static Components */
            gameObject.AddComponent<CustomCharacters>();
            gameObject.AddComponent<CustomItems>();
            gameObject.AddComponent<CustomScenes>();
            gameObject.AddComponent<CustomTextures>();

            // temp debug menu
            gameObject.AddComponent<TempDebugGui>();

            StartCoroutine(StartupCoroutine());
        }

        private IEnumerator StartupCoroutine()
        {
            Log("Version " + VERSION + " starting...", 0);

            // wait for RPM to finish loading
            while (ResourcesPrefabManager.Instance == null || !ResourcesPrefabManager.Instance.Loaded) 
            {
                yield return null;
            }

            TempDebug();

            // Preliminary SLPack load (load assets, dont apply items/recipes yet)
            foreach (string dir in Directory.GetDirectories(SL_FOLDER))
            {
                if (Path.GetFileName(dir) == "_GENERATED")
                {
                    // this is SideLoader's folder for generated templates and textures.
                    continue;
                }

                try
                {
                    var pack = SLPack.LoadFromFolder(Path.GetFileName(dir));
                    Packs.Add(pack.Name, pack);

                }
                catch (Exception e)
                {
                    Log("Error loading SLPack from folder: " + dir + "\r\nMessage: " + e.Message + "\r\nStackTrace: " + e.StackTrace, 1);
                }
            }

            Log("Assets loaded, applying custom items", 0);
            INTERNAL_ApplyItems?.Invoke();

            Log("Custom items applied, applying custom recipes", 0);
            INTERNAL_ApplyRecipes?.Invoke();

            Log("Finished initialization, calling OnPacksLoaded", 0);
            PacksLoaded = true;
            OnPacksLoaded?.Invoke();

            PacksLoaded = true;
            SL.Log("------ INIT FINALIZED, PACKSLOADED = TRUE ------");
        }

        private void TempDebug()
        {
            //var item = ResourcesPrefabManager.Instance.GetItemPrefab(2000010);
            //var template = ItemHolder.ParseItemToTemplate(item);
            //Serializer.SaveToXml("", "test", template);

            //string path = "test.xml";
            //var obj = Serializer.LoadFromXml(path);
            //Debug.Log("loaded xml, serialized type is " + obj.GetType());
        }

        // =============== Scene Changes Event ====================

        // This is called when Unity says the scene is done loading, but we still want to wait for Outward to be done.
        private void SceneManager_sceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
        {
            StartCoroutine(WaitForSceneReady());
        }

        private IEnumerator WaitForSceneReady()
        {
            while (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return null;
            }

            OnSceneLoaded?.Invoke();
        }


        // ==================== Helpers ========================= //


        public static AssetBundle LoadAssetBundle(string filepath)
        {
            try
            {
                return AssetBundle.LoadFromFile(filepath);
            }
            catch (Exception e)
            {
                Log(string.Format("Error loading bundle: {0}\r\nMessage: {1}\r\nStack Trace: {2}", filepath, e.Message, e.StackTrace), 1);
                return null;
            }
        }


        // ==================== Internal / Misc ======================== //

        /// <summary>Debug.Log with [SideLoader] prefix.</summary> <param name="errorLevel">-1 = Debug.Log, 0 = Debug.LogWarning, 1 = Debug.LogError</param>
        public static void Log(string log, int errorLevel = -1)
        {
            log = "[SideLoader] " + log;
            if (errorLevel > 0)
            {
                Debug.LogError(log);
            }
            else if (errorLevel == 0)
            {
                Debug.LogWarning(log);
            }
            else if (errorLevel < 0)
            {
                Debug.Log(log);
            }
        }

        /// <summary>Remove invalid filename characters from a string</summary>
        public static string ReplaceInvalidChars(string s)
        {
            return string.Join("_", s.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
