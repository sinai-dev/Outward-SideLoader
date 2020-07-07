using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using HarmonyLib;

namespace SideLoader
{
    public class CustomStatusEffects : MonoBehaviour
    {
        public static CustomStatusEffects Instance;

        public static readonly Dictionary<int, EffectPreset> OrigEffectPresets = new Dictionary<int, EffectPreset>();

        public static Dictionary<int, EffectPreset> RPM_EFFECT_PRESETS;
        public static Dictionary<string, StatusEffect> RPM_STATUS_EFFECTS;

        public static Dictionary<string, string> GENERAL_LOCALIZATION;

        // ================== HELPERS ==================

        /// <summary>
        /// Helper to get the cached ORIGINAL (not modified) EffectPreset of this PresetID.
        /// </summary>
        /// <param name="presetID">The Preset ID of the effect preset you want.</param>
        public static EffectPreset GetOrigEffectPreset(int presetID)
        {
            if (OrigEffectPresets.ContainsKey(presetID))
            {
                return OrigEffectPresets[presetID];
            }
            else
            {
                return ResourcesPrefabManager.Instance.GetEffectPreset(presetID);
            }
        }

        /// <summary>
        /// Use this to create or modify a Status Effect.
        /// </summary>
        /// <param name="template">The SL_StatusEffect template.</param>
        public static StatusEffect CreateCustomStatus(SL_StatusEffect template)
        {
            var preset = GetOrigEffectPreset(template.TargetStatusID);
            var original = preset.GetComponent<StatusEffect>();

            if (!original)
            {
                SL.Log("Could not find a Status Effect with the Preset ID " + template.TargetStatusID, 1);
                return null;
            }

            StatusEffect newEffect;

            if (template.TargetStatusID == template.NewStatusID)
            {
                if (!OrigEffectPresets.ContainsKey(template.TargetStatusID))
                {
                    // instantiate and cache original
                    var cached = Instantiate(original.gameObject).GetComponent<EffectPreset>();
                    cached.gameObject.SetActive(false);
                    DontDestroyOnLoad(cached.gameObject);
                    OrigEffectPresets.Add(template.TargetStatusID, cached);
                }

                newEffect = original;
            }
            else
            {
                // instantiate original and use that as newEffect
                newEffect = Instantiate(original.gameObject).GetComponent<StatusEffect>();
                newEffect.gameObject.SetActive(false);

                // Set Preset ID
                At.SetValue(template.NewStatusID, typeof(EffectPreset), preset, "m_StatusEffectID");

                // Set Status identifier
                At.SetValue(template.StatusIdentifier, typeof(StatusEffect), newEffect, "m_identifierName");

                // Fix localization
                GetStatusLocalization(original, out string name, out string desc);
                SetStatusLocalization(newEffect, name, desc);
            }

            newEffect.gameObject.name = template.NewStatusID + "_" + (template.Name ?? newEffect.IdentifierName);

            // fix RPM_STATUS_EFFECTS dictionary
            if (!RPM_STATUS_EFFECTS.ContainsKey(newEffect.IdentifierName))
            {
                RPM_STATUS_EFFECTS.Add(newEffect.IdentifierName, newEffect);
            }
            else
            {
                RPM_STATUS_EFFECTS[newEffect.IdentifierName] = newEffect;
            }

            // fix RPM_Presets dictionary
            if (!RPM_EFFECT_PRESETS.ContainsKey(template.NewStatusID))
            {
                RPM_EFFECT_PRESETS.Add(template.NewStatusID, newEffect.GetComponent<EffectPreset>());
            }
            else
            {
                //SL.Log("A Status Effect already exists with the Identifier " + template.StatusIdentifier + ", replacing with " + template.Name);
                RPM_EFFECT_PRESETS[template.NewStatusID] = newEffect.GetComponent<EffectPreset>();
            }

            // Always do this
            DontDestroyOnLoad(newEffect.gameObject);

            // Apply template
            if (SL.PacksLoaded)
            {
                template.ApplyTemplate();
            }
            else
            {
                SL.INTERNAL_ApplyStatuses += template.ApplyTemplate;
            }

            return newEffect;
        }

        public static void GetStatusLocalization(StatusEffect effect, out string name, out string desc)
        {
            var namekey = (string)At.GetValue(typeof(StatusEffect), effect, "m_nameLocKey");
            name = LocalizationManager.Instance.GetLoc(namekey);
            var desckey = (string)At.GetValue(typeof(StatusEffect), effect, "m_descriptionLocKey");
            desc = LocalizationManager.Instance.GetLoc(desckey);
        }

        /// <summary>
        /// Helper to set the Name and Description localization for a StatusEffect
        /// </summary>
        public static void SetStatusLocalization(StatusEffect effect, string name, string description)
        {
            GetStatusLocalization(effect, out string oldName, out string oldDesc);

            if (string.IsNullOrEmpty(name))
            {
                name = oldName;
            }
            if (string.IsNullOrEmpty(description))
            {
                description = oldDesc;
            }

            var preset = effect.GetComponent<EffectPreset>();

            var nameKey = $"NAME_{preset.PresetID}_{effect.IdentifierName}";
            At.SetValue(nameKey, typeof(StatusEffect), effect, "m_nameLocKey");

            if (GENERAL_LOCALIZATION.ContainsKey(nameKey))
            {
                GENERAL_LOCALIZATION[nameKey] = name;
            }
            else
            {
                GENERAL_LOCALIZATION.Add(nameKey, name);
            }

            var descKey = $"DESC_{preset.PresetID}_{effect.IdentifierName}";
            At.SetValue(descKey, typeof(StatusEffect), effect, "m_descriptionLocKey");

            if (GENERAL_LOCALIZATION.ContainsKey(descKey))
            {
                GENERAL_LOCALIZATION[descKey] = description;
            }
            else
            {
                GENERAL_LOCALIZATION.Add(descKey, description);
            }
        }

        /// <summary>
        /// Use this to create or modify an Imbue Effect status.
        /// </summary>
        /// <param name="template">The SL_ImbueEffect Template for this imbue.</param>
        public static ImbueEffectPreset CreateCustomImbue(SL_ImbueEffect template)
        {
            var original = (ImbueEffectPreset)GetOrigEffectPreset(template.TargetStatusID);

            if (!original)
            {
                SL.Log("Could not find an ImbueEffectPreset with the Preset ID " + template.TargetStatusID, 1);
                return null;
            }

            ImbueEffectPreset newEffect;

            if (template.TargetStatusID == template.NewStatusID)
            {
                if (!OrigEffectPresets.ContainsKey(template.TargetStatusID))
                {
                    // instantiate and cache original
                    var cached = Instantiate(original.gameObject).GetComponent<EffectPreset>();
                    cached.gameObject.SetActive(false);
                    DontDestroyOnLoad(cached.gameObject);
                    OrigEffectPresets.Add(template.TargetStatusID, cached);
                }

                newEffect = original;
            }
            else
            {
                // instantiate original and use that as newEffect
                newEffect = Instantiate(original.gameObject).GetComponent<ImbueEffectPreset>();
                newEffect.gameObject.SetActive(false);

                // Set Preset ID
                At.SetValue(template.NewStatusID, typeof(EffectPreset), newEffect, "m_StatusEffectID");

                // Fix localization
                GetImbueLocalization(original, out string name, out string desc);
                SetImbueLocalization(newEffect, name, desc);
            }

            newEffect.gameObject.name = template.NewStatusID + "_" + (template.Name ?? newEffect.Name);

            // fix RPM_Presets dictionary
            if (!RPM_EFFECT_PRESETS.ContainsKey(template.NewStatusID))
            {
                RPM_EFFECT_PRESETS.Add(template.NewStatusID, newEffect.GetComponent<EffectPreset>());
            }
            else
            {
                //SL.Log("An imbue already exists with the ID " + template.NewStatusID + ", replacing...");
                RPM_EFFECT_PRESETS[template.NewStatusID] = newEffect;
            }

            // Always do this
            DontDestroyOnLoad(newEffect.gameObject);

            // Apply template
            if (SL.PacksLoaded)
            {
                template.ApplyTemplate();
            }
            else
            {
                SL.INTERNAL_ApplyStatuses += template.ApplyTemplate;
            }

            return newEffect;
        }

        public static void GetImbueLocalization(ImbueEffectPreset preset, out string name, out string desc)
        {
            name = preset.Name;
            desc = preset.Description;
        }

        /// <summary>
        /// Helper to set the Name and Description localization for an Imbue Preset
        /// </summary>
        public static void SetImbueLocalization(ImbueEffectPreset preset, string name, string description)
        {
            GetImbueLocalization(preset, out string oldName, out string oldDesc);
            if (string.IsNullOrEmpty(name))
            {
                name = oldName;
            }
            if (string.IsNullOrEmpty(description))
            {
                description = oldDesc;
            }

            var nameKey = $"NAME_{preset.PresetID}_{preset.Name.Trim()}";
            At.SetValue(nameKey, typeof(ImbueEffectPreset), preset, "m_imbueNameKey");

            if (GENERAL_LOCALIZATION.ContainsKey(nameKey))
            {
                GENERAL_LOCALIZATION[nameKey] = name;
            }
            else
            {
                GENERAL_LOCALIZATION.Add(nameKey, name);
            }

            var descKey = $"DESC_{preset.PresetID}_{preset.Name.Trim()}";
            At.SetValue(descKey, typeof(ImbueEffectPreset), preset, "m_imbueDescKey");

            if (GENERAL_LOCALIZATION.ContainsKey(descKey))
            {
                GENERAL_LOCALIZATION[descKey] = description;
            }
            else
            {
                GENERAL_LOCALIZATION.Add(descKey, description);
            }

            if (preset.GetComponent<StatusEffect>() is StatusEffect status)
            {
                SetStatusLocalization(status, name, description);
            }
        }

        // ======== MISC HELPERS ========



        // ================== INTERNAL ==================

        internal void Awake()
        {
            Instance = this;

            RPM_EFFECT_PRESETS = (Dictionary<int, EffectPreset>)At.GetValue(typeof(ResourcesPrefabManager), null, "EFFECTPRESET_PREFABS");
            RPM_STATUS_EFFECTS = (Dictionary<string, StatusEffect>)At.GetValue(typeof(ResourcesPrefabManager), null, "STATUSEFFECT_PREFABS");
            GENERAL_LOCALIZATION = (Dictionary<string, string>)At.GetValue(typeof(LocalizationManager), LocalizationManager.Instance, "m_generalLocalization");
        }
    }
}
