using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using HarmonyLib;

namespace SideLoader
{
    // Work in progress

    /// <summary>
    /// Work-in-progress class, used to manage custom scenes.
    /// </summary>
    public class CustomScenes
    {
        // ***** debug *****

        private static readonly SL_Scene TestScene = new SL_Scene
        {
            SLPackName = "Test",
            AssetBundleName = "testscenebundle",
            ScenePath = "Assets/Scenes/TestScene.unity",
            DefaultSpawnIndex = 0,
            PlayerSpawnPoints = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f)
            }
        };

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                TestScene.LoadScene();
            }
        }

        // *****************

        public static bool IsLoadingCustomScene { get; private set; } = false;

        private const string SCENECORE_BUNDLE_NAME = "scenecore.bundle";
        private static bool m_shouldInstantiateCore = false;

        public static bool IsRealScene(Scene scene)
        {
            var name = scene.name.ToLower();
            return !(name.Contains("lowmemory") && !name.Contains("mainmenu"));
        }

        /// <summary>
        /// Load a Scene from an AssetBundle.
        /// </summary>
        /// <param name="bundle">The AssetBundle to load from.</param>
        /// <param name="spawnPoint">A Vector3 to spawn the characters at.</param>
        /// <param name="onSceneLoaded">A method to invoke when the scene has finished loading.</param>
        /// <param name="bundleSceneIndex">The Index of this scene in the AssetBundle (default is 0)</param>
        /// <param name="timeOffset">A time offset (in hours) applied to the Characters.</param>
        public static void LoadSceneFromBundle(AssetBundle bundle, Vector3 spawnPoint, Action onSceneLoaded, int bundleSceneIndex = 0, float timeOffset = 0f)
        {
            string scenePath = bundle.GetAllScenePaths()[bundleSceneIndex];

            IsLoadingCustomScene = true;
            m_shouldInstantiateCore = true;

            NetworkLevelLoader.Instance.LoadLevel(timeOffset, scenePath, 0);

            SL.Instance.StartCoroutine(LoadSceneCoroutine(spawnPoint, onSceneLoaded));
        }

        private static IEnumerator LoadSceneCoroutine(Vector3 spawnPoint, Action onSceneLoaded)
        {
            while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
            {
                SL.Log($"{Time.time} | Waiting for scene to be ready...");
                yield return new WaitForSeconds(1f);
            }

            onSceneLoaded?.Invoke();

            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                SL.Log($"{Time.time} | Waiting for players to be done loading...");
                yield return new WaitForSeconds(1f);
            }

            foreach (Character c in CharacterManager.Instance.Characters.Values.Where(x => !x.IsAI))
            {
                c.Teleport(spawnPoint, Quaternion.identity);
            }

            IsLoadingCustomScene = false;
        }

        public static void PopulateNecessarySceneContents()
        {
            if (!m_shouldInstantiateCore) return;

            m_shouldInstantiateCore = false;

            if (SL.Internal_Pack is SLPack internalPack)
            {
                SL.Log("[CustomScenes] Populating necessary scene contents...");

                var bundle = internalPack.AssetBundles[SCENECORE_BUNDLE_NAME];
                foreach (var go in bundle.LoadAllAssets<GameObject>())
                {
                    try
                    {
                        var obj = GameObject.Instantiate(go);
                        obj.name = obj.name.Replace("(Clone)", "");
                        SL.Log($"[CustomScenes] - Created core object '{obj.name}'");
                    }
                    catch (Exception e)
                    {
                        SL.Log($"Exception instantiating core object '{go.name}'");
                        SL.Log($"{e.GetType()}, {e.Message}\r\n{e.StackTrace}");
                    }
                }
            }
            else
            {
                SL.Log("[CustomScenes] Could not get internal SL pack!");
            }
        }
    }
}
