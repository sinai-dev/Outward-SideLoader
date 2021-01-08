using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
    //// This is to fix some things when loading custom scenes.
    //// Note: There is another patch on this method for SL_Characters which is a Postfix, this one is a Prefix.
    //[HarmonyPatch(typeof(NetworkLevelLoader), "MidLoadLevel")]
    //public class NetworkLevelLoader_MidLoadLevel_2
    //{
    //    [HarmonyPostfix]
    //    public static void Prefix()
    //    {
    //        if (CustomScenes.IsLoadingCustomScene && CustomScenes.IsRealScene(SceneManager.GetActiveScene()))
    //        {
    //            CustomScenes.PopulateNecessarySceneContents();
    //        }
    //    }
    //}
}
