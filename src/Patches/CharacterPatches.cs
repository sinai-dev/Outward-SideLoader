using HarmonyLib;
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
            var character = __instance.CharSave.CharacterUID;

            if (character != worldhost.UID)
                return;

            CustomCharacters.SaveCharacters();
        }
    }

    // Just catches a harmless null ref exception, hiding it until I figure out a cleaner fix
    [HarmonyPatch(typeof(Character), "ProcessOnEnable")]
    public class Character_ProcessOnEnable
    {
        [HarmonyFinalizer]
        public static Exception Finalizer()
        {
            return null;
        }
    }

    // This harmony patch is to sneak into when the game applies characters.
    // I figure it's best to do it at the same time.
    [HarmonyPatch(typeof(NetworkLevelLoader), "MidLoadLevel")]
    public class NetworkLevelLoader_MidLoadLevel
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            CustomCharacters.InvokeSpawnCharacters();
        }
    }

    [HarmonyPatch(typeof(NetworkLevelLoader), nameof(NetworkLevelLoader.JoinSequenceDone))]
    public class NetworkLevelLoader_JoinSequenceDone
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
                CustomCharacters.RequestSpawnedCharacters();
        }
    }

    // Like the last patch, we sneak into when the game should have destroyed previous scene characters to cleanup there.
    [HarmonyPatch(typeof(CharacterManager), "ClearNonPersitentCharacters")]
    [HarmonyPatch(typeof(NetworkLevelLoader), "StartConnectionCoroutine")]
    [HarmonyPatch(typeof(NetworkLevelLoader), "HostLost")]
    [HarmonyPatch(typeof(NetworkLevelLoader), "StartLoadLevel")]
    public class MultiPatch_CleanupCharacters
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            CustomCharacters.CleanupCharacters();
        }
    }
}
