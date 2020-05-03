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
    public class CustomAudio
    {
        public static GlobalAudioManager GAMInstance;

        public static void ReplaceAudio(GlobalAudioManager.Sounds sound, AudioClip clip)
        {
            if (GAMInstance == null)
            {
                var list = Resources.FindObjectsOfTypeAll<GlobalAudioManager>();
                if (list != null && list.Length > 0 && list[0] != null)
                {
                    GAMInstance = list[0];                    
                }
                else
                { 
                    Debug.LogWarning("Cannot find GlobalAudioManager Instance!");
                    return;
                }
            }

            try
            {
                GAM_ReplaceClip(sound, clip);
            }
            catch (Exception e)
            {
                Debug.Log("Exception replacing clip " + sound + ".\r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace);
            }
        }

        private static void GAM_ReplaceClip(GlobalAudioManager.Sounds _sound, AudioClip _newClip)
        {
            if (_newClip == null)
            {
                Debug.LogWarning("The replacement clip for " + _sound.ToString() + " is null");
            }

            var path = (string)At.Call(GAMInstance, "GetPrefabPath", new object[] { _sound });
            var resource = Resources.Load("_Sounds/" + path) as GameObject;
            var component = resource.GetComponent<AudioSource>();
            component.clip = _newClip;

            Debug.Log("Replaced " + _sound + " AudioSource with new clip!");
        }

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
