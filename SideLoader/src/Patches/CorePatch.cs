using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
    [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
    public class ResourcesPrefabManager_Load
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                SL.Log("Exception on ResourcesPrefabManager.Load: " + __exception.GetType().FullName);
                SL.Log(__exception.Message);
                SL.Log(__exception.StackTrace);
            }

            SL.Setup();

            return null;
        }
    }
}
