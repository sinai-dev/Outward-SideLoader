//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;

//namespace SideLoader
//{
//    // Work in progress

//    [SL_Serialized]
//    public class SL_Scene
//    {
//        // Some things the scene might need to define:
//        // * Spawn points
//        // * Weather
//        // * SL_Character spawns (need to be savable first?)
//        // * ...

//        public string SLPackName;
//        public string AssetBundleName;
//        public string ScenePath;

//        public List<Vector3> PlayerSpawnPoints;
//        public int DefaultSpawnIndex = 0;

//        /// <summary>
//        /// Calls LoadScene with a spawnPointIndex of 0 and a timeOffset of 0f.
//        /// </summary>
//        public void LoadScene()
//        {
//            LoadScene(0, 0f);
//        }

//        /// <summary>
//        /// Load the scene as specified by this template, and apply the template.
//        /// </summary>
//        /// <param name="spawnPointIndex">The index of your PlayerSpawnPoints list to spawn the players at.</param>
//        /// <param name="timeOffset">A time offset (in in-game hours) to apply to the players.</param>
//        public void LoadScene(int spawnPointIndex, float timeOffset = 0f)
//        {
//            try
//            {
//                var bundle = SL.Packs[this.SLPackName].AssetBundles[this.AssetBundleName];
//                var sceneIndex = bundle.GetAllScenePaths().IndexOf(this.ScenePath);

//                CustomScenes.LoadSceneFromBundle(bundle, this.PlayerSpawnPoints[spawnPointIndex], ApplyTemplate, sceneIndex, timeOffset);
//            }
//            catch (Exception ex)
//            {
//                SL.Log($"Exception loading SL_Scene: {ex.GetType()}, {ex.Message}\r\n{ex.StackTrace}");
//            }
//        }

//        public void ApplyTemplate()
//        {
//            SL.Log("Applying SL_Scene template active scene...");

//            // todo
//        }
//    }
//}
