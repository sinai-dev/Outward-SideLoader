using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SideLoader.SaveData
{
    /// <summary>
    /// Handles the saving and loading of SL_Character save data
    /// </summary>
    public static class SLCharacterSaveManager
    {
        public static bool WasLastAreaReset { get; internal set; }

        // ~~~~~ Saving ~~~~~

        // called from SLSaveManager.Save()
        internal static void SaveCharacters()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || (bool)At.GetField(NetworkLevelLoader.Instance, "m_saveOnHostLost"))
                return;

            //SL.LogWarning("~~~~~~~~~~ Saving Characters ~~~~~~~~~~");
            //SL.Log(SceneManager.GetActiveScene().name);

            var savedUIDs = new HashSet<string>();
            var activeScene = SceneManager.GetActiveScene().name;

            var sceneSaveDataList = new List<SL_CharacterSaveData>();
            var followerDataList = new List<SL_CharacterSaveData>();

            foreach (var info in CustomCharacters.ActiveCharacters)
            {
                if (info.Template != null && info.ActiveCharacter)
                {
                    if (savedUIDs.Contains(info.ActiveCharacter.UID))
                        continue;

                    if (info.Template.SaveType == CharSaveType.Scene)
                    {
                        if (activeScene != info.Template.SceneToSpawn)
                            continue;

                        var data = info.ToSaveData();
                        if (data != null)
                        {
                            sceneSaveDataList.Add(data);
                            savedUIDs.Add(info.ActiveCharacter.UID);
                        }
                    }
                    else if (info.Template.SaveType == CharSaveType.Follower)
                    {
                        var data = info.ToSaveData();
                        if (data != null)
                        {
                            followerDataList.Add(data);
                            savedUIDs.Add(info.ActiveCharacter.UID);
                        }
                    }
                }
            }

            int count = (sceneSaveDataList.Count + followerDataList.Count);

            if (count > 0)
                SL.Log("Saving " + count + " characters");

            SaveCharacterList(sceneSaveDataList.ToArray(), CharSaveType.Scene);
            SaveCharacterList(followerDataList.ToArray(), CharSaveType.Follower);
        }

        private static void SaveCharacterList(SL_CharacterSaveData[] list, CharSaveType type)
        {
            var savePath = GetCurrentSavePath(type);

            if (File.Exists(savePath))
                File.Delete(savePath);

            if (list == null || list.Length < 1)
                return;

            using (var file = File.Create(savePath))
            {
                var serializer = Serializer.GetXmlSerializer(typeof(SL_CharacterSaveData[]));
                serializer.Serialize(file, list);
            }
        }

        internal static string GetCurrentSavePath(CharSaveType saveType)
        {
            string folder = SLSaveManager.GetSaveFolderForWorldHost();
            if (string.IsNullOrEmpty(folder))
                throw new Exception("Trying to save world host SL_Characters, but couldn't get a folder!");

            var saveFolder = $@"{folder}\{SLSaveManager.CHARACTERS_FOLDER}";

            return saveType == CharSaveType.Scene
                ? saveFolder + $@"\{SceneManager.GetActiveScene().name}.chardata"
                : saveFolder + $@"\followers.chardata";
        }

        // ~~~~~ Loading ~~~~~

        internal static IEnumerator TryLoadSaveData()
        {
            while (!NetworkLevelLoader.Instance.AllPlayerReadyToContinue && NetworkLevelLoader.Instance.IsGameplayPaused)
                yield return null;

            if (!WasLastAreaReset)
            {
                TryLoadSaveData(CharSaveType.Scene);
            }
            else
            {
                var path = GetCurrentSavePath(CharSaveType.Scene);
                if (File.Exists(path))
                    File.Delete(path);
            }

            TryLoadSaveData(CharSaveType.Follower);
        }

        internal static void TryLoadSaveData(CharSaveType type)
        {
            var savePath = GetCurrentSavePath(type);

            if (!File.Exists(savePath))
                return;

            using (var file = File.OpenRead(savePath))
            {
                var serializer = Serializer.GetXmlSerializer(typeof(SL_CharacterSaveData[]));
                var list = serializer.Deserialize(file) as SL_CharacterSaveData[];

                var playerPos = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;

                foreach (var saveData in list)
                {
                    if (type == CharSaveType.Scene)
                    {
                        var character = CharacterManager.Instance.GetCharacter(saveData.CharacterUID);
                        if (!character)
                        {
                            SL.LogWarning($"Trying to apply a Scene-type SL_CharacterSaveData but could not find character with UID '{saveData.CharacterUID}'");
                            continue;
                        }

                        saveData.ApplyToCharacter(character);
                    }
                    else
                    {
                        // Followers loaded from a save should be re-spawned.
                        if (!CustomCharacters.Templates.TryGetValue(saveData.TemplateUID, out SL_Character template))
                        {
                            SL.LogWarning($"Loading a follower save data, but cannot find any SL_Character template with the UID '{saveData.TemplateUID}'");
                            continue;
                        }

                        var character = template.Spawn(playerPos, saveData.CharacterUID, saveData.ExtraRPCData);
                        saveData.ApplyToCharacter(character);
                    }
                }
            }
        }
    }
}
