using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SideLoader.UI;
using SideLoader.UI.Modules;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SideLoader
{
    /// <summary> SideLoader's BepInEx plugin class. </summary>
    [BepInPlugin(SL.GUID, SL.MODNAME, SL.VERSION)]
    public class SLPlugin : BaseUnityPlugin
    {
        public static SLPlugin Instance;

        [Obsolete("Use SL.GUID now.")]
        public const string GUID = SL.GUID;

        // ================ Main Setup ====================

        internal void Awake()
        {
            Instance = this;
            new SL();

            SL.Log($"Version {SL.VERSION} starting...");

            /* setup Harmony */

            var harmony = new Harmony(SL.GUID);
            harmony.PatchAll();

            /* SceneManager.sceneLoaded event */

            SceneManager.sceneLoaded += SL.Instance.SceneLoaded;

            /* Initialize custom textures callbacks */

            CustomTextures.Init();

            /* Setup keybinding */

            CustomKeybindings.AddAction(UIManager.MENU_TOGGLE_KEY, KeybindingsCategory.CustomKeybindings);
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
