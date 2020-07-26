using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SideLoader
{
    /// <summary>
    /// Work-in-progress class, used to manage custom scenes.
    /// </summary>
    public class CustomScenes 
    {
        /// <summary>
        /// Load a Scene from an AssetBundle.
        /// </summary>
        /// <param name="bundle">The AssetBundle to load from.</param>
        /// <param name="spawnPoint">A Vector3 to spawn the characters at.</param>
        /// <param name="bundleSceneIndex">The Index of this scene in the AssetBundle (default is 0)</param>
        /// <param name="timeOffset">A time offset (in hours) applied to the Characters.</param>
        public static void LoadSceneFromBundle(AssetBundle bundle, Vector3 spawnPoint, int bundleSceneIndex = 0, float timeOffset = 0f)
        {
            string scenePath = bundle.GetAllScenePaths()[bundleSceneIndex];
            NetworkLevelLoader.Instance.LoadLevel(timeOffset, scenePath, 0);

            SL.Instance.StartCoroutine(LoadSceneCoroutine(spawnPoint));
        }

        private static IEnumerator LoadSceneCoroutine(Vector3 spawnPoint)
        {
            while (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
            {
                yield return null;
            }

            foreach (Character c in CharacterManager.Instance.Characters.Values.Where(x => !x.IsAI))
            {
                c.Teleport(spawnPoint, Quaternion.identity);
            }
        }
    }
}
