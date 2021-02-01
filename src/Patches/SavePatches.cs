using HarmonyLib;
using SideLoader.SaveData;
using System;

namespace SideLoader.Patches
{
    // Save custom characters when game does a save
    [HarmonyPatch(typeof(SaveInstance), nameof(SaveInstance.Save))]
    public class SaveInstance_Save
    {
        [HarmonyPostfix]
        public static void Postfix(SaveInstance __instance)
        {
            SLSaveManager.OnSaveInstanceSave(__instance);
        }
    }

    [HarmonyPatch(typeof(CharacterSaveInstanceHolder), "ApplyLoadedSaveToChar")]
    public class CharacterSaveInstanceHolder_ApplyLoadedSaveToChar
    {
        [HarmonyPostfix]
        public static void Postfix(Character _character)
        {
            try
            {
                PlayerSaveExtension.LoadExtensions(_character);
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception on loading SL PlayerSaveExtensions!");
                SL.LogInnerException(ex);
            }
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadEnvironment))]
    public class SaveManager_LoadEnvironment
    {
        [HarmonyPrefix]
        public static void Prefix(DictionaryExt<string, CharacterSaveInstanceHolder> ___m_charSaves)
        {
            SLSaveManager.OnEnvironmentSaveLoaded(___m_charSaves);
        }
    }
}
