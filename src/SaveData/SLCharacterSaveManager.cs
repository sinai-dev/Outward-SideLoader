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
        public static bool SceneResetWanted { get; internal set; }

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

        internal static SL_CharacterSaveData[] TryLoadSaveData(CharSaveType type)
        {
            var savePath = GetCurrentSavePath(type);

            if (!File.Exists(savePath))
                return null;

            using (var file = File.OpenRead(savePath))
            {
                var serializer = Serializer.GetXmlSerializer(typeof(SL_CharacterSaveData[]));
                var list = serializer.Deserialize(file) as SL_CharacterSaveData[];

                return list;
            }
        }
    }
}
