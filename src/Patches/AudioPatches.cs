using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace SideLoader.Patches
{
    [HarmonyPatch(typeof(GlobalAudioManager), "CleanUpMusic")]
    public class GAM_CleanupMusic
    {
        [HarmonyPrefix]
        public static bool Prefix(ref DictionaryExt<GlobalAudioManager.Sounds, GlobalAudioManager.MusicSource> ___s_musicSources,
            ref GlobalAudioManager.Sounds ___s_currentMusic)
        {
            string name = SceneManager.GetActiveScene().name;
            for (int i = 0; i < ___s_musicSources.Values.Count; i++)
            {
                var key = ___s_musicSources.Keys[i];
                var value = ___s_musicSources.Values[i];

                if (key != ___s_currentMusic && value.SceneName != name)
                {
                    if (CustomAudio.ReplacedClips.Contains(key))
                    {
                        //SL.Log("Game tried to clean up " + key + ", but we skipped it!");
                        continue;
                    }

                    UnityEngine.Object.Destroy(value.Source.gameObject);
                    ___s_musicSources.Remove(key);
                    i--;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(GlobalAudioManager), "ReplaceClip")]
    public class GAM_ReplaceClip
    {
        [HarmonyPrefix]
        public static bool Prefix(GlobalAudioManager.Sounds _sound)
        {
            if (CustomAudio.ReplacedClips.Contains(_sound))
            {
                SL.Log("Game tried to replace " + _sound + ", but it is already replaced with a custom sound! Skipping...");
                return false;
            }

            return true;
        }
    }
}
