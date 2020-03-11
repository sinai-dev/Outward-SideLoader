using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Partiality.Modloader;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace SideLoader_2
{
    /// <summary> Use SL.Instance to access the active SideLoader instance. </summary>
    public class SL : PartialityMod
    {
        /// <summary>The active SideLoader Instance</summary>
        public static SideLoader Instance;

        public SL()
        {
            this.author = "Sinai";
            this.ModID = SideLoader.MODNAME;
            this.Version = SideLoader.VERSION;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            var obj = new GameObject(SideLoader.MODNAME);
            GameObject.DontDestroyOnLoad(obj);

            obj.AddComponent<SideLoader>();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }

    /// <summary> For internal use only. Use SL.Instance to access the active SideLoader instance. </summary>
    public class SideLoader : MonoBehaviour
    {
        public static readonly string MODNAME = "SideLoader";
        public static readonly string VERSION = "2.0.0";

        public static readonly string SL_FOLDER = @"Mods\SideLoader";

        public delegate void SceneLoaded();
        public static event SceneLoaded OnSceneLoaded;

        public Dictionary<string, SLPack> Packs = new Dictionary<string, SLPack>();

        public bool PacksLoaded { get; private set; } = false;
        public delegate void LoadedPacks();
        public static event LoadedPacks OnPacksLoaded;

        internal void Awake()
        {
            SL.Instance = this;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        internal void Start()
        {
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            Log("Version " + VERSION + " starting...", 0);

            // Add non-static Components
            gameObject.AddComponent<SL_AssetBundles>();
            gameObject.AddComponent<SL_Textures>();
            gameObject.AddComponent<SL_Items>();
            gameObject.AddComponent<SL_Audio>();

            // wait for RPM to finish loading
            while (ResourcesPrefabManager.Instance == null || !ResourcesPrefabManager.Instance.Loaded) 
            {
                yield return null;
            }

            // Load SLPacks
            foreach (string dir in Directory.GetDirectories(SL_FOLDER))
            {
                try
                {
                    var pack = new SLPack();

                    pack.LoadFromFolder(dir);
                    pack.ApplyPack();

                    Packs.Add(pack.Name, pack);

                }
                catch (Exception e)
                {
                    Log("Error loading SLPack from folder: " + dir + "\r\nMessage: " + e.Message + "\r\nStackTrace: " + e.StackTrace, 1);
                }
            }

            Log("Finished initialization.", 0);

            PacksLoaded = true;

            OnPacksLoaded?.Invoke();
        }

        // This is called when Unity says the scene is done loading, but we still want to wait for Outward to be done.
        private void SceneManager_sceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
        {
            StartCoroutine(WaitForSceneReady());
        }

        private IEnumerator WaitForSceneReady()
        {
            while (!ResourcesPrefabManager.Instance.Loaded || !NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return null;
            }

            OnSceneLoaded?.Invoke();
        }

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
