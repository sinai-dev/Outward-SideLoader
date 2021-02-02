﻿using BepInEx;
using SideLoader.SaveData;
using SideLoader.SLPacks;
using SideLoader.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SideLoader
{
    public class SL
    {
        public static SL Instance { get; private set; }

        public SL()
        {
            Instance = this;
        }

        // Folders
        public const string SL_FOLDER = @"Mods\SideLoader";
        public static string PLUGINS_FOLDER => @"BepInEx\plugins\";

        public static string INTERNAL_FOLDER => $@"{SL_FOLDER}\{_INTERNAL}";
        public const string _INTERNAL = "_INTERNAL";

        internal static Transform s_cloneHolder;

        // SL Packs
        internal static readonly Dictionary<string, SLPack> s_packs = new Dictionary<string, SLPack>();
        internal static readonly Dictionary<string, SLPackArchive> s_archivePacks = new Dictionary<string, SLPackArchive>();
        internal static readonly Dictionary<string, SLPackBundle> s_bundlePacks = new Dictionary<string, SLPackBundle>();

        /// <summary>
        /// Get an SLPack from a provided SLPack folder name.
        /// </summary>
        /// <param name="name">The folder name, either `Mods\SideLoader\{Name}\` or `BepInEx\plugins\{Name}\SideLoader\`</param>
        /// <returns>The SLPack instance, if one was loaded with that name.</returns>
        public static SLPack GetSLPack(string name)
        {
            s_packs.TryGetValue(name, out SLPack pack);

            if (pack == null && s_archivePacks.TryGetValue(name, out SLPackArchive archive))
            { 
                pack = archive;

                if (pack == null && s_bundlePacks.TryGetValue(name, out SLPackBundle bundle))
                    pack = bundle;
            }

            return pack;
        }

        /// <summary>Have SL Packs been loaded yet?</summary>
        public static bool PacksLoaded { get; internal set; } = false;

        /// <summary>Invoked before packs are loaded and applied, but after ResouresPrefabManager 
        /// is loaded.</summary>
        public static event Action BeforePacksLoaded;

        /// <summary>Only called once on startup. This will be after ResourcesPrefabManager is 
        /// loaded, and all SLPacks are loaded and applied.</summary>
        public static event Action OnPacksLoaded;

        /// <summary>Use this to safely make changes to a scene when it is truly loaded. (All players 
        /// loaded, gameplay may not yet be resumed).</summary>
        public static event Action OnSceneLoaded;

        /// <summary>This event is invoked when gameplay actually resumes after a scene is loaded.</summary>
        public static event Action OnGameplayResumedAfterLoading;

        /// <summary>
        /// Whether or not the last scene loaded had an Area Reset performed on it (ie, 7 days passed or was first ever load).<br/><br/>
        /// <b>NOTE: </b> this is only reliable when you are the host character!
        /// </summary>
        public static bool WasLastSceneReset;

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

            SLPlugin.Instance.StartCoroutine(WaitForGameplayResumed());
        }

        private IEnumerator WaitForGameplayResumed()
        {
            while (NetworkLevelLoader.Instance.IsGameplayPaused)
                yield return null;

            TryInvoke(OnGameplayResumedAfterLoading);
        }

        // ======== SL Setup ========

        internal static void Setup(bool isFirstSetup = true)
        {
            var start = Time.realtimeSinceStartup;

            s_cloneHolder = new GameObject("SL_CloneHolder").transform;
            GameObject.DontDestroyOnLoad(s_cloneHolder.gameObject);

            if (isFirstSetup)
            {
                SLRPCManager.Setup();

                PlayerSaveExtension.LoadExtensionTypes();

                CheckPrefabDictionaries();

                TryInvoke(BeforePacksLoaded);
            }
            else
                ResetForHotReload();

            // Load SL Packs
            SLPackManager.LoadAllPacks(isFirstSetup);

            if (isFirstSetup)
            {
                foreach (var pack in SL.s_packs)
                    pack.Value.TryApplyItemTextureBundles();
            }

            // Invoke OnPacksLoaded and cleanup
            PacksLoaded = true;
            Log("SideLoader Setup Finished");
            Log("-------------------------");

            var end = Time.realtimeSinceStartup - start;

            SL.Log("SL Setup took " + end + " seconds");

            if (isFirstSetup)
            {
                if (OnPacksLoaded != null)
                {
                    TryInvoke(OnPacksLoaded);
                    Log("Finished invoking OnPacksLoaded.");
                }

                /* Setup UI */
                UIManager.Init();
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

            foreach (var vfx in SL_ShootProjectile.ProjectilePrefabCache.Values)
                vfx.transform.parent = SL.s_cloneHolder;

            foreach (var vfx in SL_PlayVFX.VfxPrefabCache.Values)
                vfx.transform.parent = SL.s_cloneHolder;

            foreach (var vfx in SL_ShootBlast.BlastPrefabCache.Values)
                vfx.transform.parent = SL.s_cloneHolder;

            SL.Log("Built FX prefab dictionaries");
        }

        internal static void ResetForHotReload()
        {
            foreach (var ctg in SLPackManager.SLPackCategories)
            {
                try
                {
                    ctg.OnHotReload();
                }
                catch (Exception e)
                {
                    LogWarning("Exception Hot Reloading category: " + ctg.ToString());
                    LogInnerException(e);
                }
            }

            // Reset packs
            PacksLoaded = false;
            s_packs.Clear();
            s_archivePacks.Clear();
            s_bundlePacks.Clear();
        }

        // ==================== Helpers ==================== //

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
