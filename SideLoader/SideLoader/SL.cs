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
using HarmonyLib;
using BepInEx;
using SideLoader.UI;

namespace SideLoader
{
    /// <summary> The main SideLoader class. </summary>
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class SL : BaseUnityPlugin
    {
        public static SL Instance;

        // Mod Info
        public const string GUID = "com.sinai." + MODNAME;
        public const string MODNAME = "SideLoader";
        public const string VERSION = "2.6.2";

        // Folders
        public static string PLUGINS_FOLDER => Paths.PluginPath;
        public const string SL_FOLDER = @"Mods\SideLoader";
        public static string GENERATED_FOLDER { get => SL_FOLDER + @"\_GENERATED"; }

        // Loaded SLPacks
        public static Dictionary<string, SLPack> Packs = new Dictionary<string, SLPack>();
        public static bool PacksLoaded { get; private set; } = false;

        // Events
        /// <summary>Invoked before packs are loaded and applied, but after ResouresPrefabManager is loaded.</summary>
        public static event Action BeforePacksLoaded;
        /// <summary>Only called once on startup. This will be after ResourcesPrefabManager is loaded, and all SLPacks are loaded and applied.</summary>
        public static event UnityAction OnPacksLoaded;
        /// <summary>Use this to safely make changes to a scene when it is truly loaded. (All players loaded, gameplay may not yet be resumed).</summary>
        public static event UnityAction OnSceneLoaded;

        // Internal Events
        /// <summary>Only called once on startup. It is a callback used by SL_StatusEffect and SL_ImbueEffect to apply after assets are loaded.</summary>
        public static event Action INTERNAL_ApplyStatuses;
        /// <summary>Only called once on startup. It is a callback used by SL_Items to apply after all assets are loaded.</summary>
        public static event Action INTERNAL_ApplyItems;
        /// <summary>Only called once on startup. It is a callback used by SL_Recipes and SL_EnchantmentRecipes to apply after all CustomItems are loaded.</summary>
        public static event Action INTERNAL_ApplyRecipes;
        /// <summary>Only called once on startup. It is a callback used by SL_RecipeItems to apply after all SL_Recipes are loaded.</summary>
        public static event Action INTERNAL_ApplyRecipeItems;

        // ================ Main Setup ====================

        internal void Awake()
        {
            Log($"Version {VERSION} starting...", 0);

            Instance = this;

            /* Create base SL folder */
            if (!Directory.Exists(SL_FOLDER))
            {
                Directory.CreateDirectory(SL_FOLDER);
            }
            
            /* setup Harmony */
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            /* subscribe to SceneLoaded event */
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            /* Setup Behaviour gameobject */
            var obj = new GameObject("SideLoader_Behaviour");
            DontDestroyOnLoad(obj);

            /* Add Behaviour Components */
            obj.AddComponent<CustomCharacters>();
            obj.AddComponent<CustomItems>();
            obj.AddComponent<CustomScenes>();
            obj.AddComponent<CustomStatusEffects>();
            obj.AddComponent<CustomTextures>();
            obj.AddComponent<SL_GUI>();
            obj.AddComponent<RPCManager>();
        }

        /// <summary>
        /// Called by a Harmony Patch Finalizer on ResourcesPrefabManager.Load
        /// </summary>
        public static void Setup()
        {
            // Prepare Blast and Projectile prefab dictionaries.

            //SL_ShootBlast.DebugBlastNames();
            //SL_ShootProjectile.DebugProjectileNames();

            SL_ShootBlast.BuildBlastsDictionary();
            SL_ShootProjectile.BuildProjectileDictionary();

            // ==========================

            // BeforePacksLoaded callback
            TryInvoke(BeforePacksLoaded);

            // ====== Read SL Packs ======

            // new structure: BepInEx\plugins\ModName\SideLoader\
            foreach (string modFolder in Directory.GetDirectories(PLUGINS_FOLDER))
            {
                string name = Path.GetFileName(modFolder);

                if (name == "SideLoader" || name == "ModConfigs" || name == "PartialityWrapper")
                {
                    continue;
                }

                var slFolder = modFolder + @"\SideLoader";
                if (Directory.Exists(slFolder))
                {
                    SLPack.TryLoadPack(name, slFolder, false);
                }
            }

            // Mods\SideLoader\ folder packs:
            foreach (string dir in Directory.GetDirectories(SL_FOLDER))
            {
                if (dir == GENERATED_FOLDER)
                {
                    // this is SideLoader's folder for generated templates and textures.
                    continue;
                }

                var packname = Path.GetFileName(dir);
                SLPack.TryLoadPack(packname, dir, true);
            }

             // ====== Invoke Callbacks ======

            Log("Applying custom Statuses", 0);
            TryInvoke(INTERNAL_ApplyStatuses);

            Log("Applying custom Items", 0);
            TryInvoke(INTERNAL_ApplyItems);

            Log("Applying custom Recipes", 0);
            TryInvoke(INTERNAL_ApplyRecipes);

            Log("Applying custom Recipe Items", 0);
            TryInvoke(INTERNAL_ApplyRecipeItems);

            PacksLoaded = true;
            Log("SideLoader Setup Finished");
            Log("-------------------------");
            TryInvoke(OnPacksLoaded);

            //// *********************************** temp debug ***********************************

            //// I use this to generate the "Types" xml resources.

            //foreach (var type in Serializer.SLTypes.Where(x => !x.IsAbstract))
            //{
            //    Serializer.SaveToXml(GENERATED_FOLDER + @"\Types", type.Name, Activator.CreateInstance(type));
            //}

            //// **********************************************************************************
        }

        // =============== Scene Change Events ====================

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

            TryInvoke(OnSceneLoaded);
        }


        // ==================== Helpers ========================= //

        /// <summary>
        /// Generic helper for invoking a MulticastDelegate safely
        /// </summary>
        /// <param name="_delegate">Either an Action or UnityAction, generally.</param>
        /// <param name="args">Any arguments the delegate expects.</param>
        public static void TryInvoke(MulticastDelegate _delegate, params object[] args)
        {
            if (_delegate != null)
            {
                foreach (var action in _delegate.GetInvocationList())
                {
                    try
                    {
                        action.DynamicInvoke(args);
                    }
                    catch (Exception e)
                    {
                        Log("Exception invoking callback! Checking for InnerException...", 1);
                        LogInnerException(e);
                    }
                }
            }
        }

        /// <summary>
        /// Simple helper for loading an AssetBundle inside a try/catch.
        /// </summary>
        public static AssetBundle LoadAssetBundle(string filepath)
        {
            try
            {
                return AssetBundle.LoadFromFile(filepath);
            }
            catch (Exception e)
            {
                Log($"Error loading bundle: {filepath}\r\nMessage: {e.Message}\r\nStack Trace: {e.StackTrace}");
                return null;
            }
        }

        /// <summary> Small helper for destroying all children on a given Transform 't'. Uses DestroyImmediate(). </summary>
        /// <param name="t">The transform whose children you want to destroy.</param>
        /// <param name="destroyContent">If true, will destroy children called "Content" (used for Bags)</param>
        public static void DestroyChildren(Transform t, bool destroyContent = false)
        {
            var list = new List<GameObject>();
            foreach (Transform child in t)
            {
                if (destroyContent || child.name != "Content")
                {
                    list.Add(child.gameObject);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                DestroyImmediate(list[i]);
            }
        }

        /// <summary>Writes all the values from 'other' to 'comp', then returns comp.</summary>
        public static T GetCopyOf<T>(Component comp, T other) where T : Component
        {
            var type = comp.GetType();
            if (!typeof(T).IsAssignableFrom(type))
            {
                SL.Log("Cannot assign " + typeof(T) + " from " + type);
                return null;
            }
            
            foreach (var pinfo in type.GetProperties(At.FLAGS))
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

            At.CopyFieldValues(comp, other);

            return comp as T;
        }

        // ==================== Internal / Misc ======================== //

        /// <summary>Debug.Log with [SideLoader] prefix.</summary>
        /// <param name="log">The message to log.</param>
        /// <param name="errorLevel">-1 = Debug.Log, 0 = Debug.LogWarning, 1 = Debug.LogError</param>
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

        /// <summary>
        /// Recursively logs inner exceptions from an Exception, if there are any.
        /// </summary>
        public static void LogInnerException(Exception ex)
        {
            var inner = ex.InnerException;

            if (inner != null)
            {
                Log($"Inner Exception: {inner.Message}");

                if (inner.InnerException != null)
                {
                    // There is another level, keep going.
                    LogInnerException(inner);
                }
                else
                {
                    // We reached the end, log the stack.
                    Log($"Inner-most Stack Trace: {inner.StackTrace}");
                }
            }
        }
    }
}
