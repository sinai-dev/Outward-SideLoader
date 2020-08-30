using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
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
                SL.Log("Cannot find GlobalAudioManager Instance!", 0);
                return;
            }
            
            if (ReplacedClips.Contains(sound))
            {
                SL.Log("The Sound clip '" + sound + "' has already been replaced!");
            }

            try
            {
                GAM_ReplaceClip(sound, clip);
                ReplacedClips.Add(sound);
            }
            catch (Exception e)
            {
                SL.Log("Exception replacing clip " + sound + ".\r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace, 1);
            }
        }

        private static void GAM_ReplaceClip(GlobalAudioManager.Sounds _sound, AudioClip _newClip)
        {
            if (!_newClip)
            {
                Debug.LogWarning("The replacement clip for " + _sound.ToString() + " is null");
            }

            var path = (string)At.Call(typeof(GlobalAudioManager), GAMInstance, "GetPrefabPath", null, new object[] { _sound });
            var resource = Resources.Load("_Sounds/" + path) as GameObject;
            var component = resource.GetComponent<AudioSource>();
            component.clip = _newClip;

            Debug.Log("Replaced " + _sound + " AudioSource with new clip!");
        }

        /// <summary>Coroutine used to load an AudioClip.</summary>
        public static IEnumerator LoadClip(string path, SLPack pack = null)
        {
            var fullPath = @"file://" + Path.GetFullPath(path);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV))
            {
                Debug.Log("Loading audio clip " + path);

                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.error != null)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    var clip = DownloadHandlerAudioClip.GetContent(www);
                    GameObject.DontDestroyOnLoad(clip);

                    SL.Log("Loaded audio clip " + name);

                    if (pack != null)
                    {
                        pack.AudioClips.Add(name, clip);
                    }

                    if (Enum.TryParse(name, out GlobalAudioManager.Sounds sound))
                    {
                        ReplaceAudio(sound, clip);
                    }
                }
            }
        }
    }
}
