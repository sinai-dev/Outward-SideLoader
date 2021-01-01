using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx;
using SideLoader.GUI;
using BepInEx.Logging;
using System.Collections.Generic;

namespace SideLoader
{
    /// <summary> SideLoader's BepInEx plugin class. </summary>
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class SLPlugin : BaseUnityPlugin
    {
        public static SLPlugin Instance;

        // Mod Info
        public const string GUID = "com.sinai." + MODNAME;
        public const string MODNAME = "SideLoader";
        public const string VERSION = "3.1.3";

        // ================ Main Setup ====================

        internal void Awake()
        {
            Instance = this;
            new SL();

            SL.Log($"Version {VERSION} starting...");

            /* Create base SL folder */

            if (!Directory.Exists(SL.SL_FOLDER))
                Directory.CreateDirectory(SL.SL_FOLDER);

            /* setup Harmony */

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            /* SceneManager.sceneLoaded event */

            SceneManager.sceneLoaded += SL.Instance.SceneLoaded;

            /* setup custom textures */

            CustomTextures.Init();

            /* Setup Behaviour gameobject */

            var obj = new GameObject("SideLoader_Behaviour");
            GameObject.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;

            /* Add Behaviour Components */
            obj.AddComponent<SLGUI>();
        }

        // ========== Update ==========

        internal void Update()
        {
            Helpers.ForceUnlockCursor.UpdateCursorControl();

            //CustomScenes.Update();
        }

        // ========== Logging ==========

        public static void Log(string log, SL.LogLevel level)
        {
            if (Instance != null)
            {
                Instance.Logger.Log((LogLevel)level, log);
            }
            else
            {
                log = $"[SideLoader] {log}";
                switch (level)
                {
                    case SL.LogLevel.Message:
                        SL.Log(log); break;
                    case SL.LogLevel.Warning:
                        SL.LogWarning(log); break;
                    case SL.LogLevel.Error:
                        SL.LogError(log); break;
                }
            }
        }
    }
}
