using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;

namespace SideLoader
{
    /// <summary>SideLoader's public-facing core class.</summary>
    public class SL
    {
        public static SL Instance;

        public SL()
        {
            Instance = this;
        }

        // Folders
        public const string SL_FOLDER = @"Mods\SideLoader";
        public static string PLUGINS_FOLDER => Paths.PluginPath;
        public static string GENERATED_FOLDER => $@"{SL_FOLDER}\{_GENERATED}";
        public static string INTERNAL_FOLDER => $@"{SL_FOLDER}\{_INTERNAL}";

        public const string _GENERATED = "_GENERATED";
        public const string _INTERNAL = "_INTERNAL";

        // SL Packs
        internal static Dictionary<string, SLPack> Packs = new Dictionary<string, SLPack>();

        public static SLPack GetSLPack(string name)
        {
            Packs.TryGetValue(name, out SLPack pack);
            return pack;
        }

        /// <summary>Have SL Packs been loaded yet?</summary>
        public static bool PacksLoaded { get; private set; } = false;

        /// <summary>Invoked before packs are loaded and applied, but after ResouresPrefabManager 
        /// is loaded.</summary>
        public static event Action BeforePacksLoaded;

        /// <summary>Only called once on startup. This will be after ResourcesPrefabManager is 
        /// loaded, and all SLPacks are loaded and applied.</summary>
        public static event Action OnPacksLoaded;

        /// <summary>Use this to safely make changes to a scene when it is truly loaded. (All players 
        /// loaded, gameplay may not yet be resumed).</summary>
        public static event Action OnSceneLoaded;

        // Internal Events
        internal static event Action INTERNAL_ApplyStatuses;
        internal static event Action INTERNAL_ApplyItems;
        internal static event Action INTERNAL_ApplyRecipes;
        internal static event Action INTERNAL_ApplyLateItems;

        // ======== Scene Change Event ========

        // This is called when Unity says the scene is done loading, but we still want to wait for Outward to be done.
        internal void SceneLoaded(Scene scene, LoadSceneMode _)
        {
            if (!scene.name.ToLower().Contains("lowmemory") && !scene.name.ToLower().Contains("mainmenu"))
                SLPlugin.Instance.StartCoroutine(WaitForSceneReady());
        }

        private IEnumerator WaitForSceneReady()
        {
            while (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                yield return null;

            TryInvoke(OnSceneLoaded);
        }

        // ======== SL Packs Setup ========

        internal static void Setup(bool firstSetup = true)
        {
            if (firstSetup)
            {
                SLRPCManager.Setup();

                CheckPrefabDictionaries();

                TryInvoke(BeforePacksLoaded);
            }
            else
                Reset();

            // ====== Read SL Packs ======

            // 'BepInEx\plugins\...' packs:
            foreach (var dir in Directory.GetDirectories(PLUGINS_FOLDER))
            {
                var name = Path.GetFileName(dir);

                var slFolder = dir + @"\SideLoader";
                if (Directory.Exists(slFolder))
                {
                    SLPack.TryLoadPack(name, false, !firstSetup);
                }
            }

            // 'Mods\SideLoader\...' packs:
            foreach (var dir in Directory.GetDirectories(SL_FOLDER))
            {
                if (dir == GENERATED_FOLDER || dir == INTERNAL_FOLDER || dir == SLSaveManager.SAVEDATA_FOLDER)
                {
                    // not a real SLPack
                    continue;
                }

                var name = Path.GetFileName(dir);
                SLPack.TryLoadPack(name, true, !firstSetup);
            }

            // ====== Invoke Callbacks ======

            var delegates = new Dictionary<string, Action>
            {
                { "Status Effects", INTERNAL_ApplyStatuses },
                { "Items",          INTERNAL_ApplyItems },
                { "Recipes",        INTERNAL_ApplyRecipes },
                { "Late Items",     INTERNAL_ApplyLateItems },
            };

            foreach (var entry in delegates)
            {
                if (entry.Value != null)
                {
                    Log($"Applying custom {entry.Key}, count: {entry.Value.GetInvocationList().Length}");
                    TryInvoke(entry.Value);
                }
            }

            if (firstSetup)
            {
                foreach (var pack in Packs.Values)
                    pack.TryApplyItemTextureBundles();
            }

            PacksLoaded = true;
            Log("SideLoader Setup Finished");
            Log("-------------------------");

            if (firstSetup && OnPacksLoaded != null)
            {
                TryInvoke(OnPacksLoaded);
                Log("Finished invoking OnPacksLoaded.");
            }
        }

        internal static void CheckPrefabDictionaries()
        {
            //// This debug is for when we need to dump prefab names for enums.
            //// Only needs to be run on updates.

            //SL_ShootBlast.DebugBlastNames();
            //SL_ShootProjectile.DebugProjectileNames();
            //SL_PlayVFX.DebugVfxNames();

            // Once names have been dumped after an update and enums built, we only need this.

            SL_ShootBlast.BuildBlastsDictionary();
            SL_ShootProjectile.BuildProjectileDictionary();
            SL_PlayVFX.BuildPrefabDictionary();
        }

        internal static void Reset()
        {
            // Reset packs
            Packs.Clear();

            // Clear textures dictionary
            CustomTextures.Textures.Clear();

            // Reset internal invocation lists
            INTERNAL_ApplyItems = null;
            INTERNAL_ApplyLateItems = null;
            INTERNAL_ApplyRecipes = null;
            INTERNAL_ApplyStatuses = null;
        }

        public static void TryInvoke(MulticastDelegate _delegate, params object[] args)
        {
            if (_delegate == null)
                return;

            foreach (var action in _delegate.GetInvocationList())
            {
                try
                {
                    action.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    LogWarning("Exception invoking callback! Checking for InnerException...");
                    LogInnerException(e.InnerException);
                }
            }
        }

        // ==================== Logging ==================== //

        public enum LogLevel
        {
            Message = 8,
            Warning = 4,
            Error = 2
        }

        public static void Log(string log) => SLPlugin.Log(log, LogLevel.Message);
        public static void LogWarning(string log) => SLPlugin.Log(log, LogLevel.Warning);
        public static void LogError(string log) => SLPlugin.Log(log, LogLevel.Error);

        public static void LogInnerException(Exception ex)
        {
            if (ex == null)
                return;

            var inner = ex.InnerException;

            if (inner == null)
                LogWarning("---- Inner-most exception: ----");

            Log($"{ex.GetType().Name}: {ex.Message}");

            if (inner == null)
                Log(ex.StackTrace);
            else
                LogInnerException(inner);
        }
    }
}
