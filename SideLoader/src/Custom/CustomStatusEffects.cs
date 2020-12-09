using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using HarmonyLib;
using SideLoader.Helpers;

namespace SideLoader
{
    /// <summary>
    /// SideLoader's manager class for Custom Status Effects. Contains helpful methods for creating and managing SL_StatusEffects and SL_ImbueEffects.
    /// </summary>
    public class CustomStatusEffects
    {
        /// <summary>Cached un-edited Status Effects.</summary>
        public static readonly Dictionary<string, StatusEffect> OrigStatusEffects = new Dictionary<string, StatusEffect>();

        /// <summary>Cached un-edited Effect Presets, used by Imbue Presets. For StatusEffects, use GetOrigStatusEffect.</summary>
        public static readonly Dictionary<int, EffectPreset> OrigEffectPresets = new Dictionary<int, EffectPreset>();

        // ================== HELPERS ==================

        /// <summary>
        /// Helper to get the cached ORIGINAL (not modified) EffectPreset of this PresetID.
        /// </summary>
        /// <param name="presetID">The Preset ID of the effect preset you want.</param>
        /// <returns>The EffectPreset, if found.</returns>
        public static EffectPreset GetOrigEffectPreset(int presetID)
        {
            if (OrigEffectPresets.ContainsKey(presetID))
                return OrigEffectPresets[presetID];
            else
                return ResourcesPrefabManager.Instance.GetEffectPreset(presetID);
        }

        /// <summary>
        /// Get the original Status Effect with this identifier.
        /// </summary>
        /// <param name="identifier">The identifier to get.</param>
        /// <returns>The EffectPreset, if found, otherwise null.</returns>
        public static StatusEffect GetOrigStatusEffect(string identifier)
        {
            if (OrigStatusEffects.ContainsKey(identifier))
                return OrigStatusEffects[identifier];
            else
                return ResourcesPrefabManager.Instance.GetStatusEffectPrefab(identifier);
        }

        /// <summary>
        /// Use this to create or modify a Status Effect.
        /// </summary>
        /// <param name="template">The SL_StatusEffect template.</param>
        /// <returns>The new or existing StatusEffect.</returns>
        public static StatusEffect CreateCustomStatus(SL_StatusEffect template)
        {
            var original = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(template.TargetStatusIdentifier);

            if (!original)
            {
                SL.Log($"CreateCustomStatus - Could not find any status with the Identifier '{template.TargetStatusIdentifier}' (or none was set).");
                return null;
            }

            var preset = original.GetComponent<EffectPreset>();
            if (!preset && template.NewStatusID > 0)
            {
                preset = original.gameObject.AddComponent<EffectPreset>();
                At.SetValue(template.NewStatusID, "m_StatusEffectID", preset);
            }

            StatusEffect newEffect;

            if (template.TargetStatusIdentifier == template.StatusIdentifier)
            {
                // editing orig status
                if (!OrigStatusEffects.ContainsKey(template.TargetStatusIdentifier))
                {
                    // instantiate and cache original
                    var cached = GameObject.Instantiate(original.gameObject).GetComponent<StatusEffect>();
                    cached.gameObject.SetActive(false);
                    GameObject.DontDestroyOnLoad(cached.gameObject);
                    OrigStatusEffects.Add(template.TargetStatusIdentifier, cached);
                }

                newEffect = original;
            }
            else
            {
                // instantiate original and use that as newEffect
                newEffect = GameObject.Instantiate(original.gameObject).GetComponent<StatusEffect>();
                newEffect.gameObject.SetActive(false);

                // Set Status identifier
                At.SetValue(template.StatusIdentifier, "m_identifierName", newEffect);


                if (preset)
                    At.SetValue(template.NewStatusID, "m_StatusEffectID", preset);

                // Fix localization
                GetStatusLocalization(original, out string name, out string desc);
                SetStatusLocalization(newEffect, name, desc);

                // Fix status data and stack
                At.SetValue(null, "m_statusStack", newEffect);
                At.SetValue(null, "m_amplifiedStatus", newEffect);
            }

            int presetID = newEffect.GetComponent<EffectPreset>()?.PresetID ?? -1;

            var id = "";
            if (presetID > 0)
                id += presetID + "_";
            newEffect.gameObject.name = id + newEffect.IdentifierName;

            // fix RPM_STATUS_EFFECTS dictionary
            if (!References.RPM_STATUS_EFFECTS.ContainsKey(newEffect.IdentifierName))
                References.RPM_STATUS_EFFECTS.Add(newEffect.IdentifierName, newEffect);
            else
                References.RPM_STATUS_EFFECTS[newEffect.IdentifierName] = newEffect;

            // fix RPM_Presets dictionary
            if (template.NewStatusID > 0)
            {
                if (!References.RPM_EFFECT_PRESETS.ContainsKey(template.NewStatusID))
                    References.RPM_EFFECT_PRESETS.Add(template.NewStatusID, newEffect.GetComponent<EffectPreset>());
                else
                    References.RPM_EFFECT_PRESETS[template.NewStatusID] = newEffect.GetComponent<EffectPreset>();
            }

            // Always do this
            GameObject.DontDestroyOnLoad(newEffect.gameObject);

            // Apply template
            if (SL.PacksLoaded)
                template.ApplyTemplate();
            else
                SL.INTERNAL_ApplyStatuses += template.ApplyTemplate;

            return newEffect;
        }

        /// <summary>
        /// Get the Localization for the Status Effect (name and description).
        /// </summary>
        /// <param name="effect">The Status Effect to get localization for.</param>
        /// <param name="name">The output name.</param>
        /// <param name="desc">The output description.</param>
        public static void GetStatusLocalization(StatusEffect effect, out string name, out string desc)
        {
            name = LocalizationManager.Instance.GetLoc((string)At.GetValue("m_nameLocKey", effect));
            desc = LocalizationManager.Instance.GetLoc((string)At.GetValue("m_descriptionLocKey", effect));
        }

        /// <summary>
        /// Helper to set the Name and Description localization for a StatusEffect
        /// </summary>
        public static void SetStatusLocalization(StatusEffect effect, string name, string description)
        {
            GetStatusLocalization(effect, out string oldName, out string oldDesc);

            if (string.IsNullOrEmpty(name))
                name = oldName;
            if (string.IsNullOrEmpty(description))
                description = oldDesc;

            var nameKey = $"NAME_{effect.IdentifierName}";
            At.SetValue(nameKey, "m_nameLocKey", effect);

            if (References.GENERAL_LOCALIZATION.ContainsKey(nameKey))
                References.GENERAL_LOCALIZATION[nameKey] = name;
            else
                References.GENERAL_LOCALIZATION.Add(nameKey, name);

            var descKey = $"DESC_{effect.IdentifierName}";
            At.SetValue(descKey, "m_descriptionLocKey", effect);

            if (References.GENERAL_LOCALIZATION.ContainsKey(descKey))
                References.GENERAL_LOCALIZATION[descKey] = description;
            else
                References.GENERAL_LOCALIZATION.Add(descKey, description);
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
                SL.LogError("Could not find an ImbueEffectPreset with the Preset ID " + template.TargetStatusID);
                return null;
            }

            ImbueEffectPreset newEffect;

            if (template.TargetStatusID == template.NewStatusID)
            {
                if (!OrigEffectPresets.ContainsKey(template.TargetStatusID))
                {
                    // instantiate and cache original
                    var cached = GameObject.Instantiate(original.gameObject).GetComponent<EffectPreset>();
                    cached.gameObject.SetActive(false);
                    GameObject.DontDestroyOnLoad(cached.gameObject);
                    OrigEffectPresets.Add(template.TargetStatusID, cached);
                }

                newEffect = original;
            }
            else
            {
                // instantiate original and use that as newEffect
                newEffect = GameObject.Instantiate(original.gameObject).GetComponent<ImbueEffectPreset>();
                newEffect.gameObject.SetActive(false);

                // Set Preset ID
                At.SetValue<EffectPreset>(template.NewStatusID, "m_StatusEffectID", newEffect);

                // Fix localization
                GetImbueLocalization(original, out string name, out string desc);
                SetImbueLocalization(newEffect, name, desc);
            }

            newEffect.gameObject.name = template.NewStatusID + "_" + (template.Name ?? newEffect.Name);

            // fix RPM_Presets dictionary
            if (!References.RPM_EFFECT_PRESETS.ContainsKey(template.NewStatusID))
                References.RPM_EFFECT_PRESETS.Add(template.NewStatusID, newEffect.GetComponent<EffectPreset>());
            else
                References.RPM_EFFECT_PRESETS[template.NewStatusID] = newEffect;

            // Always do this
            GameObject.DontDestroyOnLoad(newEffect.gameObject);

            // Apply template
            if (SL.PacksLoaded)
                template.ApplyTemplate();
            else
                SL.INTERNAL_ApplyStatuses += template.ApplyTemplate;

            return newEffect;
        }

        /// <summary>
        /// Helper to get the name and description for an Imbue.
        /// </summary>
        /// <param name="preset">The Imbue Preset to get localization for.</param>
        /// <param name="name">The output name.</param>
        /// <param name="desc">The output description.</param>
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
                name = oldName;

            if (string.IsNullOrEmpty(description))
                description = oldDesc;

            var nameKey = $"NAME_{preset.PresetID}_{preset.Name.Trim()}";
            At.SetValue(nameKey, "m_imbueNameKey", preset);

            if (References.GENERAL_LOCALIZATION.ContainsKey(nameKey))
                References.GENERAL_LOCALIZATION[nameKey] = name;
            else
                References.GENERAL_LOCALIZATION.Add(nameKey, name);

            var descKey = $"DESC_{preset.PresetID}_{preset.Name.Trim()}";
            At.SetValue(descKey, "m_imbueDescKey", preset);

            if (References.GENERAL_LOCALIZATION.ContainsKey(descKey))
                References.GENERAL_LOCALIZATION[descKey] = description;
            else
                References.GENERAL_LOCALIZATION.Add(descKey, description);

            if (preset.GetComponent<StatusEffect>() is StatusEffect status)
                SetStatusLocalization(status, name, description);
        }
    }
}
