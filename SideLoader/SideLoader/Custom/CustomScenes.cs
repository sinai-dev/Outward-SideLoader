using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/*
 *  This class is not yet finished. Still in testing phases. At the moment it just loads a scene from an AssetBundle.
*/

namespace SideLoader
{
    public class CustomScenes : MonoBehaviour
    {
        public static CustomScenes Instance;

        internal void Awake()
        {
            Instance = this;
        }

        public static void LoadSceneFromBundle(AssetBundle bundle, Vector3 spawnPoint, int bundleSceneIndex = 0, float timeOffset = 0f)
        {
            if (spawnPoint == null)
            {
                spawnPoint = Vector3.zero;
            }

            string scenePath = bundle.GetAllScenePaths()[bundleSceneIndex];
            NetworkLevelLoader.Instance.LoadLevel(timeOffset, scenePath, 0);

            Instance.StartCoroutine(LoadSceneCoroutine(spawnPoint));
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
