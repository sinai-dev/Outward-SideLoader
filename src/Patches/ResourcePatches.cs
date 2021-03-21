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
                    SL.Log($"Exception on ResourcesPrefabManager.Load: {__exception.GetType().Name}, {__exception.Message}");
                    SL.LogInnerException(__exception);
                }

                SL.Setup();

                return null;
            }
        }

        // Patches for StatusEffects and EffectPresets to fix duplicates.

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
                        dict[status.IdentifierName] = status;
                    else
                        dict.Add(status.IdentifierName, status);
                }

                var resources = Resources.LoadAll("_StatusEffects", typeof(StatusEffect));
                foreach (StatusEffect status in resources)
                {
                    if (!dict.ContainsKey(status.IdentifierName))
                        dict.Add(status.IdentifierName, status);
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
                        dict[effect.PresetID] = effect;
                    else
                        dict.Add(effect.PresetID, effect);
                }

                var resources = Resources.LoadAll("_StatusEffects", typeof(EffectPreset));
                foreach (EffectPreset effect in resources)
                {
                    if (!dict.ContainsKey(effect.PresetID))
                        dict.Add(effect.PresetID, effect);
                }

                return false;
            }
        }
    }
}
