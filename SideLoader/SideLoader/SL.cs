using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Reflection;

namespace SideLoader
{
    /// <summary> The main SideLoader class. </summary>
    public class SL : MonoBehaviour
    {
        public static SL Instance;

        // Mod Info
        public const string MODNAME = "SideLoader";
        public const string VERSION = "2.0.4";

        // Folders
        public const string SL_FOLDER = @"Mods\SideLoader";
        public static string GENERATED_FOLDER { get => SL_FOLDER + @"\_GENERATED"; }

        // Loaded SLPacks
        public static Dictionary<string, SLPack> Packs = new Dictionary<string, SLPack>();
        public static bool PacksLoaded { get; private set; } = false;

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
            Instance = this;

            if (!Directory.Exists(SL_FOLDER))
            {
                Directory.CreateDirectory(SL_FOLDER);
            }
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

            // debug menu
            gameObject.AddComponent<DebugMenu>();

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

            // Preliminary SLPack load (load assets, dont apply items/recipes yet)
            foreach (string dir in Directory.GetDirectories(SL_FOLDER))
            {
                if (dir == GENERATED_FOLDER)
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

            Log("Applying custom Items", 0);
            INTERNAL_ApplyItems?.Invoke();

            Log("Applying custom Recipes", 0);
            INTERNAL_ApplyRecipes?.Invoke();

            PacksLoaded = true;
            SL.Log("------ SideLoader Setup Finished ------");
            OnPacksLoaded?.Invoke();

            //// *********************************** temp debug ***********************************

            //foreach (var type in Serializer.Types)
            //{
            //    Serializer.SaveToXml(GENERATED_FOLDER + @"\Types", type.Name, Activator.CreateInstance(type));
            //}

            //// **********************************************************************************
        }

        // =============== Scene Changes Event ====================

        // This is called when Unity says the scene is done loading, but we still want to wait for Outward to be done.
        private void SceneManager_sceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
        {
            if (!_scene.name.ToLower().Contains("lowmemory") && !_scene.name.ToLower().Contains("mainmenu"))
            {
                StartCoroutine(WaitForSceneReady());
            }
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

        /// <summary>Writes all the values from 'other' to 'comp', then returns comp.</summary>
        /// CREDIT: https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html
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
