using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
    // Fixes a ReflectionTypeLoadException which can happen in Caldera, caused by VegetationStudio when you have certain mods.

    [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetTypes), new Type[0])]
    public class Assembly_GetTypes
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(Assembly __instance, Exception __exception, ref Type[] __result)
        {
            // if there was an exception, use GetTypesSafe (a method from At).
            if (__exception != null)
                __result = __instance.GetTypesSafe().ToArray();

            // rethrow as no exception.
            return null;
        }
    }
}
