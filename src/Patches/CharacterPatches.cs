using HarmonyLib;
using SideLoader.SaveData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
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

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadEnvironment))]
    public class SaveManager_LoadEnvironment
    {
        [HarmonyPrefix]
        public static void Prefix(DictionaryExt<string, CharacterSaveInstanceHolder> ___m_charSaves)
        {
            var host = CharacterManager.Instance.GetWorldHostCharacter();
            if (!host || !host.IsPhotonPlayerLocal)
                return;

            if (___m_charSaves.TryGetValue(host.UID, out CharacterSaveInstanceHolder holder))
            {
                if (At.GetField(holder.CurrentSaveInstance, "m_loadedScene") is EnvironmentSave loadedScene)
                {
                    float age = (float)(loadedScene.GameTime - EnvironmentConditions.GameTime);

                    SLCharacterSaveManager.SceneResetWanted = AreaManager.Instance.IsAreaExpired(loadedScene.AreaName, age);
                }
                else
                    SLCharacterSaveManager.SceneResetWanted = true;
            }
            else
                SLCharacterSaveManager.SceneResetWanted = true;
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
            SLPlugin.Instance.StartCoroutine(DelayedSpawnRoutine());
        }

        private static IEnumerator DelayedSpawnRoutine()
        {
            while (!NetworkLevelLoader.Instance.AllPlayerReadyToContinue && NetworkLevelLoader.Instance.IsGameplayPaused)
                yield return null;

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

    // sneak into when the game should have destroyed previous scene characters to cleanup there.
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
