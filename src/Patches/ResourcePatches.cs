using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SideLoader.Patches
{
    public class ResourcePatches
    {
        [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
        public class ResourcesPrefabManager_Load
        {
            [HarmonyFinalizer]
            public static Exception Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    SL.Log("Exception on ResourcesPrefabManager.Load: " + __exception.GetType().FullName);
                    SL.LogInnerException(__exception);
                }

                SL.Setup();

                return null;
            }
        }

        [HarmonyPatch(typeof(ResourcesPrefabManager), "LoadStatusEffectPrefabs")]
        public class RPM_LoadStatusEffectPrefabs
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                var dict = References.RPM_STATUS_EFFECTS;
                dict.Clear();

                var statuses = Resources.FindObjectsOfTypeAll<StatusEffect>();
                foreach (var status in statuses)
                {
                    if (dict.ContainsKey(status.IdentifierName))
                    {
                        //SL.LogWarning($"Duplicate status identifier: {status.IdentifierName}");
                        var old = dict[status.IdentifierName];
                        GameObject.Destroy(old.gameObject);
                        dict[status.IdentifierName] = status;
                        continue;
                    }

                    dict.Add(status.IdentifierName, status);
                    //SL.Log($"Loaded status: {status.IdentifierName}");
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ResourcesPrefabManager), "LoadEffectPresetPrefabs")]
        public class RPM_LoadEffectPresetPrefabs
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                var dict = References.RPM_EFFECT_PRESETS;
                dict.Clear();

                var statuses = Resources.FindObjectsOfTypeAll<EffectPreset>();
                foreach (var effect in statuses)
                {
                    if (dict.ContainsKey(effect.PresetID))
                    {
                        //SL.LogWarning($"Duplicate effect preset ID: {effect.PresetID} (skipping {effect.name})");
                        var old = dict[effect.PresetID];
                        GameObject.Destroy(old.gameObject);
                        dict[effect.PresetID] = effect;
                        continue;
                    }

                    dict.Add(effect.PresetID, effect);
                    //SL.Log($"Loaded effect preset: {effect.name}");
                }

                return false;
            }
        }
    }
}
