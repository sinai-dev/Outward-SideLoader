using HarmonyLib;
using SideLoader.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
    // Save custom characters when game does a save
    [HarmonyPatch(typeof(SaveInstance), nameof(SaveInstance.Save))]
    public class SaveInstance_Save
    {
        [HarmonyPostfix]
        public static void Postfix(SaveInstance __instance)
        {
            try
            {
                if (__instance.CharSave == null || string.IsNullOrEmpty(__instance.CharSave.CharacterUID))
                    return;

                var worldhost = CharacterManager.Instance?.GetWorldHostCharacter();
                var charUID = __instance.CharSave.CharacterUID;

                if (worldhost && charUID == worldhost.UID)
                    SLCharacterSaveManager.SaveCharacters();

                PlayerSaveExtension.SaveAllExtensions(CharacterManager.Instance.GetCharacter(charUID));
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception on SaveInstance.Save!");
                SL.LogInnerException(ex);
            }
        }
    }


    //ApplyLoadedSaveToChar(Character _character)
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
                SL.LogWarning("Exception on CharacterSaveInstanceHolder.ApplyLoadedSvaeToChar!");
                SL.LogInnerException(ex);
            }
        }
    }
}
