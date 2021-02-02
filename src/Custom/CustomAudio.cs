using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SideLoader
{
    /// <summary>Helper class used to manage and replace Audio and Music.</summary>
    public class CustomAudio
    {
        /// <summary>The GlobalAudioManager Instance reference (since its not public)</summary>
        public static GlobalAudioManager GAMInstance => References.GLOBALAUDIOMANAGER;

        /// <summary>
        /// List of AudioClips which have been replaced, acting as a blacklist.
        /// </summary>
        public static readonly List<GlobalAudioManager.Sounds> ReplacedClips = new List<GlobalAudioManager.Sounds>();

        /// <summary>Replace a global sound with the provided AudioClip.</summary>
        public static void ReplaceAudio(GlobalAudioManager.Sounds sound, AudioClip clip)
        {
            if (!GAMInstance)
            {
                SL.LogWarning("Cannot find GlobalAudioManager Instance!");
                return;
            }

            if (ReplacedClips.Contains(sound))
                SL.Log($"The Sound clip '{sound}' has already been replaced, replacing again...");

            try
            {
                DoReplaceClip(sound, clip);
            }
            catch (Exception e)
            {
                SL.LogError($"Exception replacing clip '{sound}'.\r\nMessage: {e.Message}\r\nStack: {e.StackTrace}");
            }
        }

        private static void DoReplaceClip(GlobalAudioManager.Sounds _sound, AudioClip _newClip)
        {
            if (!_newClip)
            {
                SL.LogWarning($"The replacement clip for '{_sound}' is null");
                return;
            }

            //var path = GAMInstance.GetPrefabPath(_sound);
            var path = (string)At.Invoke(GAMInstance, "GetPrefabPath", _sound);
            var resource = Resources.Load<GameObject>("_Sounds/" + path);
            var component = resource.GetComponent<AudioSource>();
            component.clip = _newClip;

            resource.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            if (!ReplacedClips.Contains(_sound))
                ReplacedClips.Add(_sound);

            SL.Log("Replaced " + _sound + " AudioSource with new clip!");
        }

        public static AudioClip LoadAudioClip(string filePath, SLPack pack = null)
        {
            if (!File.Exists(filePath))
                return null;

            var data = File.ReadAllBytes(filePath);

            return LoadAudioClip(data, Path.GetFileNameWithoutExtension(filePath), pack);
        }

        public static AudioClip LoadAudioClip(byte[] data, string name, SLPack pack = null)
        {
            try
            {
                float[] floatArr = new float[data.Length / 4];
                for (int i = 0; i < floatArr.Length; i++)
                {
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(data, i * 4, 4);

                    floatArr[i] = BitConverter.ToSingle(data, i * 4) / 0x80000000;
                }

                AudioClip clip = AudioClip.Create(name, floatArr.Length, 1, 44100, false);
                clip.SetData(floatArr, 0);

                return LoadAudioClip(clip, name, pack);
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception loading AudioClip!");
                SL.LogInnerException(ex);
                return null;
            }
        }

        public static AudioClip LoadAudioClip(AudioClip clip, string name, SLPack pack = null)
        {
            clip.name = name;

            if (pack != null)
            {
                if (pack.AudioClips.ContainsKey(name))
                {
                    SL.LogWarning("Replacing clip '" + name + "' in pack '" + pack.Name + "'");

                    if (pack.AudioClips[name])
                        GameObject.Destroy(pack.AudioClips[name]);

                    pack.AudioClips.Remove(name);
                }

                pack.AudioClips.Add(name, clip);
            }

            if (Enum.TryParse(name, out GlobalAudioManager.Sounds sound))
                ReplaceAudio(sound, clip);

            return clip;
        }
    }
}
