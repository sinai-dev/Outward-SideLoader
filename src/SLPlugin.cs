using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using SideLoader.UI;
using SideLoader.UI.Modules;

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
        public const string VERSION = "3.2.8";

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

            /* Setup keybinding */

            CustomKeybindings.AddAction(UIManager.MENU_TOGGLE_KEY, KeybindingsCategory.CustomKeybindings);

            /* Setup Behaviour gameobject */

            //var obj = new GameObject("SideLoader_Behaviour");
            //GameObject.DontDestroyOnLoad(obj);
            //obj.hideFlags = HideFlags.HideAndDontSave;

            /* Add Behaviour Components */
            //obj.AddComponent<SLGUI>();
        }

        // ========== Update ==========

        internal void Update()
        {
            UIManager.Update();

            Helpers.ForceUnlockCursor.UpdateCursorControl();

            //CustomScenes.Update();
        }

        // ========== Logging ==========

        public static void Log(string log, SL.LogLevel level)
        {
            Instance.Logger.Log((LogLevel)level, log);

            string color = ColorUtility.ToHtmlStringRGB(Color.white);
            if (level == SL.LogLevel.Warning)
                color = ColorUtility.ToHtmlStringRGB(Color.yellow);
            else if (level == SL.LogLevel.Error)
                color = ColorUtility.ToHtmlStringRGB(Color.red);

            DebugConsole.Log(log, color);
        }
    }
}
