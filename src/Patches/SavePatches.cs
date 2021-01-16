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
            var worldhost = CharacterManager.Instance.GetWorldHostCharacter();
            var character = __instance.CharSave?.CharacterUID;

            if (!worldhost || string.IsNullOrEmpty(character))
                return;

            if (character == worldhost.UID)
                SLCharacterSaveManager.SaveCharacters();

            PlayerSaveExtension.SaveAllExtensions(CharacterManager.Instance.GetCharacter(character));
        }
    }


    //ApplyLoadedSaveToChar(Character _character)
    [HarmonyPatch(typeof(CharacterSaveInstanceHolder), "ApplyLoadedSaveToChar")]
    public class CharacterSaveInstanceHolder_ApplyLoadedSaveToChar
    {
        [HarmonyPostfix]
        public static void Postfix(Character _character)
        {
            PlayerSaveExtension.LoadExtensions(_character);
        }
    }
}
