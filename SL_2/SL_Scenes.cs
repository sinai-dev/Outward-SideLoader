using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SideLoader_2
{
    public static class SL_Scenes
    {
        public static void LoadSceneFromBundle(AssetBundle bundle, int bundleSceneIndex, Vector3 spawnPoint, float timeOffset = 0f)
        {
            if (spawnPoint == null)
            {
                spawnPoint = Vector3.zero;
            }

            string scenePath = bundle.GetAllScenePaths()[0];
            NetworkLevelLoader.Instance.LoadLevel(scenePath, 0);
        }
    }
}
