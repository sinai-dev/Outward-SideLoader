using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader_2
{
    public class SL_Audio : MonoBehaviour
    {
        public static SL_Audio Instance;

        public static Dictionary<string, AudioClip> CustomMusic = new Dictionary<string, AudioClip>();
        private AudioSource m_currentMusicScource;

        internal void Start()
        {
            Instance = this;

            m_currentMusicScource = gameObject.GetOrAddComponent<AudioSource>();
            DontDestroyOnLoad(m_currentMusicScource);

            On.GlobalAudioManager.PlayMusic += PlayMusicHook;
        }

        public static AudioClip LoadAudioClip(string filePath)
        {
            filePath = @"file://" + Path.GetFullPath(filePath);

            return WWWAudioExtensions.GetAudioClip(new WWW(filePath));
        }

        public IEnumerator LoadCustomMusic()
        {
            if (Directory.Exists(SideLoader.SL_FOLDER + "/CustomMusic"))
            {
                foreach (string filePath in Directory.GetFiles(SideLoader.SL_FOLDER + "/CustomMusic"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    var clip = LoadAudioClip(filePath);

                    if (clip != null)
                    {
                        DontDestroyOnLoad(clip);

                        while (clip.loadState != AudioDataLoadState.Loaded)
                            yield return null;

                        if (CustomMusic.ContainsKey(fileName))
                        {
                            CustomMusic[fileName] = clip;
                        }
                        else
                        {
                            CustomMusic.Add(fileName, clip);
                        }

                        // todo if its main menu music, call PlayMusic
                    }
                }
            }
        }

        private AudioSource PlayMusicHook(On.GlobalAudioManager.orig_PlayMusic orig, GlobalAudioManager self, GlobalAudioManager.Sounds _sound, float _fade)
        {
            string songName = _sound.ToString();

            if (CustomMusic.ContainsKey(songName)
                && At.GetValue(typeof(GlobalAudioManager), self, "s_musicSources") is DictionaryExt<GlobalAudioManager.Sounds, GlobalAudioManager.MusicSource> dict)
            {
                // set our custom clip to the actual GlobalAudioManager dictionary, so it works with the game systems as expected

                if (!dict.ContainsKey(_sound) && At.Call(self, "GetPrefabPath", new object[] { _sound }) is string prefabPath)
                {
                    GameObject gameObject = Resources.Load("_Sounds/" + prefabPath) as GameObject;
                    gameObject = Instantiate(gameObject);
                    AudioSource component = gameObject.GetComponent<AudioSource>();
                    DontDestroyOnLoad(gameObject);
                    dict.Add(_sound, new GlobalAudioManager.MusicSource(component));
                }

                dict[_sound].Source.clip = CustomMusic[_sound.ToString()];

                At.SetValue(dict, typeof(GlobalAudioManager), self, "s_musicSources");

                At.Call(self, "CleanUpMusic", null);

                At.SetValue(_sound, typeof(GlobalAudioManager), self, "s_currentMusic");

                StartCoroutine(FadeMusic(self, _sound, dict[_sound], _fade));

                return dict[_sound].Source;
            }
            else
            {
                return orig(self, _sound, _fade);
            }
        }

        private IEnumerator FadeMusic(GlobalAudioManager _manager, GlobalAudioManager.Sounds _music, GlobalAudioManager.MusicSource musSource, float _time, bool _in = true)
        {
            float vol;
            float targetVol;
            if (!_in)
            {
                vol = musSource.Source.volume;
                targetVol = 0f;
            }
            else
            {
                vol = 0f;
                targetVol = musSource.OrigVolume;
            }
            if (!musSource.Source.gameObject.activeSelf)
            {
                musSource.Source.gameObject.SetActive(true);
            }
            if (_in)
            {
                musSource.Source.Play();
            }
            float lerp = 0f;
            while (lerp < 1f)
            {
                if (!musSource.Source)
                {
                    break;
                }
                lerp = Mathf.Clamp01(lerp + Time.deltaTime / _time);
                musSource.Source.volume = Mathf.Lerp(vol, targetVol, lerp);
                yield return null;
            }
            if (!_in && musSource.Source)
            {
                if (At.GetValue(typeof(GlobalAudioManager), _manager, "m_eventMusic") is GlobalAudioManager.Sounds _eventMusic && _eventMusic != GlobalAudioManager.Sounds.NONE
                    && _music == _eventMusic)
                {
                    musSource.Source.Stop();
                }
                else
                {
                    musSource.Source.Pause();
                }
            }

            yield return null;
        }
    }
}
