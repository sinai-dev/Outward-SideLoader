using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
    [HarmonyPatch(typeof(Deployable), nameof(Deployable.StartDeployAnimation))]
    public class Deployable_StartDeployAnimation
    {
        [HarmonyPrefix]
        public static void Prefix(Deployable __instance)
        {
            var item = At.GetField(__instance as ItemExtension, "m_item") as Item;
            if (!item)
                item = __instance.GetComponent<Item>();

            At.SetField(__instance as ItemExtension, "m_item", item);
        }
    }
}
