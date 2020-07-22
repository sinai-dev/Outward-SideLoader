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
        public const string VERSION = "2.6.7";

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
        /// <summary>Only called once on startup. It is a callback used by SL_RecipeItems and other templates to apply after all other templates are loaded.</summary>
        public static event Action INTERNAL_ApplyLateItems;

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

            /* setup custom textures */
            CustomTextures.Init();

            /* Setup Behaviour gameobject */
            var obj = new GameObject("SideLoader_Behaviour");
            DontDestroyOnLoad(obj);

            /* Add Behaviour Components */            
            obj.AddComponent<SL_GUI>();
            obj.AddComponent<RPCManager>();
        }

        /// <summary>
        /// Called by a Harmony Patch Finalizer on ResourcesPrefabManager.Load
        /// </summary>
        public static void Setup()
        {
            // ==========================
            // Prepare Blast and Projectile prefab dictionaries.

            //SL_ShootBlast.DebugBlastNames();
            //SL_ShootProjectile.DebugProjectileNames();

            SL_ShootBlast.BuildBlastsDictionary();
            SL_ShootProjectile.BuildProjectileDictionary();

            // ==========================

            // BeforePacksLoaded callback
            TryInvoke(BeforePacksLoaded);

            // ====== Read SL Packs ======

            // 'BepInEx\plugins\...' packs:
            foreach (string modFolder in Directory.GetDirectories(PLUGINS_FOLDER))
            {
                string name = Path.GetFileName(modFolder);

                var slFolder = modFolder + @"\SideLoader";
                if (Directory.Exists(slFolder))
                {
                    SLPack.TryLoadPack(name, slFolder, false);
                }
            }

            // 'Mods\SideLoader\...' packs:
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

            Dictionary<string, MulticastDelegate> delegates = new Dictionary<string, MulticastDelegate>
            {
                { "Status Effects", INTERNAL_ApplyStatuses },
                { "Items", INTERNAL_ApplyItems },
                { "Recipes", INTERNAL_ApplyRecipes },
                { "Recipe Items", INTERNAL_ApplyLateItems },
            };

            foreach (var entry in delegates)
            {
                if (entry.Value != null)
                {
                    Log($"Applying custom {entry.Key}, count: {entry.Value.GetInvocationList().Length}");
                    TryInvoke(entry.Value);
                }
            }

            // Check for TextureBundles in SL Packs
            foreach (var pack in Packs.Values)
            {
                pack.TryApplyItemTextureBundles();
            }

            PacksLoaded = true;
            Log("SideLoader Setup Finished");
            Log("-------------------------");

            TryInvoke(OnPacksLoaded);
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
        /// <param name="destroyActivator">If true, will destroy children called "Activator" (used for Deployables / Traps)</param>
        public static void DestroyChildren(Transform t, bool destroyContent = false, bool destroyActivator = false)
        {
            var list = new List<GameObject>();
            foreach (Transform child in t)
            {
                if ((destroyContent || child.name != "Content") && (destroyActivator || child.name != "Activator"))
                {
                    list.Add(child.gameObject);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                DestroyImmediate(list[i]);
            }
        }

        /// <summary>
        /// Replaces existingComponent type with desiredType ONLY if desiredType is not assignable from the existingComponent type.
        /// That means if desiredType is Item and existingComponent type is Weapon, this will do nothing.
        /// If both types are the same, this will do nothing.
        /// Otherwise, this will replace existingComponent with a desiredType component and inherit all possible values.
        /// </summary>
        /// <param name="desiredType">The desired class type (the game type, not the SL type)</param>
        /// <param name="existingComponent">The existing component</param>
        /// <returns>The component left on the transform after the method runs.</returns>
        public static Component FixComponentType(Type desiredType, Component existingComponent)
        {
            if (!existingComponent || !existingComponent.transform || desiredType == null || desiredType.IsAbstract)
            {
                return existingComponent;
            }

            var currentType = existingComponent.GetType();

            // If currentType derives from desiredType (or they are the same type), do nothing
            // This is to allow using basic SL_Item (or whatever) templates on more complex types without replacing them.
            if (desiredType.IsAssignableFrom(currentType))
            {
                return existingComponent;
            }

            var newComp = existingComponent.gameObject.AddComponent(desiredType);

            while (!currentType.IsAssignableFrom(desiredType) && currentType.BaseType != null && currentType.BaseType != typeof(MonoBehaviour))
            {
                // Desired type does not derive from current type.
                // We need to recursively dive through currentType's BaseTypes until we find a type we can assign from.
                // Eg, current is MeleeWeapon and we want a ProjectileWeapon. We need to get the common base class (Weapon, in that case).
                // When currentType reaches Weapon, Weapon.IsAssignableFrom(ProjectileWeapon) will return true.
                // We also want to make sure we didnt reach MonoBehaviour, and at least got a game class.
                currentType = currentType.BaseType;
            }

            // Final check if the value copying is valid, after operations above.
            if (currentType.IsAssignableFrom(desiredType))
            {
                // recursively get all the field values
                At.CopyFields(newComp, existingComponent, currentType, true);
                At.CopyProperties(newComp, existingComponent, currentType, true);
            }
            else
            {
                Log($"FixComponentTypeIfNeeded - could not find a compatible type of {currentType.Name} which is assignable to desired type: {desiredType.Name}!");
            }

            // remove the old component
            GameObject.DestroyImmediate(existingComponent);

            return newComp;
        }

        /// <summary>
        /// Gets a copy of Component and adds it to the transform provided.
        /// </summary>
        /// <typeparam name="T">The Type of Component which will be added to the transform.</typeparam>
        /// <param name="component">The existing component to copy from (and the T if not directly supplied)</param>
        /// <param name="transform">The Transform to add to</param>
        /// <returns></returns>
        public static T GetCopyOf<T>(T component, Transform transform) where T : Component
        {
            var comp = transform.gameObject.AddComponent(component.GetType());

            At.CopyProperties(comp, component, null, true);
            At.CopyFields(comp, component, null, true);

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
                // There is another level, we'll keep going.
                Log($"Exception: {ex.Message}");
                LogInnerException(inner);
            }
            else
            {
                // We reached the end, log the stack.
                Log($"Inner-most Exception: {ex.Message}");
                Log($"Inner-most Stack Trace: {ex.StackTrace}");
            }
        }
    }
}
