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
    /// <summary>
    /// Use SL.Instance to access the active SideLoader instance. 
    /// </summary>
    public class SL : PartialityMod
    {
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

    /// <summary>
    /// For internal use only. Use SL.Instance to access the active SideLoader instance.
    /// </summary>
    public class SideLoader : MonoBehaviour
    {
        public static readonly string MODNAME = "OTW_SideLoader";
        public static readonly string VERSION = "2.0.0";

        public static readonly string SL_FOLDER = @"Mods\SideLoader";

        /// <summary>
        /// Use this to check if SideLoader is ready to use.
        /// </summary>
        public bool InitDone { get; private set; } = false;

        /// <summary>
        /// INTERNAL USE ONLY. For coroutines. Don't use this to check if SL is done loading.
        /// </summary>
        public bool Loading = false;

        // List of supported Resource types (each type represents a SL Pack folder)
        public string[] SupportedResources = 
        {
            ResourceTypes.Texture,
            ResourceTypes.AssetBundle,
            ResourceTypes.CustomItems,
            ResourceTypes.Audio,
        };

        // Coroutines to run on load (in this order)
        private readonly List<IEnumerator> LoadingCoroutines = new List<IEnumerator>
        {
            SL_AssetBundles.Instance.LoadAssetBundles(),
            SL_Textures.Instance.LoadTextures(),
            SL_Audio.Instance.LoadAudioClips(),
            SL_Items.Instance.LoadItemXMLs(),
            SL_Textures.Instance.ReplaceActiveTextures() // replace active textures after everything else
        };

        // Just used for startup
        public Dictionary<string, List<string>> FilePaths = new Dictionary<string, List<string>>();

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

            // read folders, store all file paths in FilePaths dictionary
            CheckFolders();

            // wait for RPM to finish loading
            while (ResourcesPrefabManager.Instance == null || !ResourcesPrefabManager.Instance.Loaded) 
            {
                yield return null;
            }

            foreach (var coroutine in LoadingCoroutines)
            {
                Loading = true;
                StartCoroutine(coroutine);
                while (Loading)
                {
                    yield return null;
                }
            }

            Log("Finished initialization.", 0);

            InitDone = true;
        }

        // Checks all the folders in the SideLoader folder and makes a list of all Assets which should be loaded.
        private void CheckFolders()
        {
            int totalFiles = 0;

            Log("Checking for SideLoader packs...");

            foreach (string pack in Directory.GetDirectories(SL_FOLDER))
            {
                Log("Checking pack " + pack + "...");

                foreach (string resourceType in SupportedResources)
                {
                    // Make sure we have the key initialized
                    if (!FilePaths.ContainsKey(resourceType))
                        FilePaths.Add(resourceType, new List<string>());

                    string dir = pack + @"\" + resourceType;

                    if (!Directory.Exists(dir))
                    {
                        continue;
                    }

                    string[] paths = Directory.GetFiles(dir);

                    foreach (string file in paths)
                    {
                        if (resourceType == ResourceTypes.AssetBundle && (file.EndsWith(".manifest") || file.EndsWith(".meta")))
                        {
                            continue;
                        }

                        string assetPath = new FileInfo(file).Name;
                        FilePaths[resourceType].Add(dir + @"\" + assetPath);

                        totalFiles++; // add to total asset counter
                    }
                }
            }

            Log(string.Format("Found {0} total files to load.", totalFiles));
        }

        // This is called when Unity says the scene is done loading, but we still want to wait for Outward to be done.
        private void SceneManager_sceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
        {
            // We don't care about doing this for the Main Menu. Active textures are replaced after game has loaded anyway.
            if (_scene.name != "MainMenu_Empty")
            {
                StartCoroutine(OnSceneDoneLoading());
            }
        }

        // SceneChange callbacks go here
        private IEnumerator OnSceneDoneLoading()
        {
            while (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.1f);
            }

            StartCoroutine(SL_Textures.Instance.ReplaceActiveTextures());
        }

        // Small Logging helper
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
    }

    public static class ResourceTypes
    {
        public static string Texture = "Texture2D";
        public static string AssetBundle = "AssetBundles";
        public static string CustomItems = "CustomItems";
        public static string Audio = "Audio";
    }
}
