using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using HarmonyLib;
using Discord;

namespace SideLoader
{
    public class CustomStatusEffects
    {
        /// <summary>Cached un-edited Effect Presets, used for all Status and Imbues which have a Preset ID.</summary>
        public static readonly Dictionary<int, EffectPreset> OrigEffectPresets = new Dictionary<int, EffectPreset>();
        /// <summary>Cached un-edited Status Effects. <b>Only used for StatusEffects which do not have a Preset ID.</b></summary>
        public static readonly Dictionary<string, StatusEffect> OrigStatusEffects = new Dictionary<string, StatusEffect>();

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

        public static StatusEffect GetOrigStatusEffect(string identifier, bool checkPresets = true)
        {
            if (OrigStatusEffects.ContainsKey(identifier))
            {
                return OrigStatusEffects[identifier];
            }
            else
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(identifier);
                if (checkPresets)
                {
                    // check if it was backed up from a Preset ID
                    if (status?.GetComponent<EffectPreset>() is EffectPreset preset
                        && GetOrigEffectPreset(preset.PresetID)?.GetComponent<StatusEffect>() is StatusEffect presetStatus)
                    {
                        return presetStatus;
                    }
                }
                return status;
            }
        }

        /// <summary>
        /// Use this to create or modify a Status Effect.
        /// </summary>
        /// <param name="template">The SL_StatusEffect template.</param>
        public static StatusEffect CreateCustomStatus(SL_StatusEffect template)
        {
            var preset = GetOrigEffectPreset(template.TargetStatusID);
            var original = preset?.GetComponent<StatusEffect>();

            if (!original)
            {
                bool found = false;
                if (!string.IsNullOrEmpty(template.TargetStatusIdentifier))
                {
                    var status = GetOrigStatusEffect(template.TargetStatusIdentifier, false);
                    if (status)
                    {
                        found = true;
                        original = status;
                        template.CloneByIdentifier = true;
                    }
                }
                if (!found)
                {
                    SL.Log("Could not find a Status Effect with the Preset ID " + template.TargetStatusID, 1);
                    return null;
                }
            }

            StatusEffect newEffect;

            bool editingOrig;
            if (template.CloneByIdentifier)
            {
                editingOrig = template.StatusIdentifier == template.TargetStatusIdentifier;
            }
            else
            {
                editingOrig = template.NewStatusID == template.TargetStatusID;
            }

            if (editingOrig)
            {
                if (template.CloneByIdentifier)
                {
                    if (!OrigStatusEffects.ContainsKey(template.TargetStatusIdentifier))
                    {
                        // instantiate and cache original
                        var cached = GameObject.Instantiate(original.gameObject).GetComponent<StatusEffect>();
                        cached.gameObject.SetActive(false);
                        GameObject.DontDestroyOnLoad(cached.gameObject);
                        OrigStatusEffects.Add(template.TargetStatusIdentifier, cached);
                    }
                }
                else
                {
                    if (!OrigEffectPresets.ContainsKey(template.TargetStatusID))
                    {
                        // instantiate and cache original
                        var cached = GameObject.Instantiate(original.gameObject).GetComponent<EffectPreset>();
                        cached.gameObject.SetActive(false);
                        GameObject.DontDestroyOnLoad(cached.gameObject);
                        OrigEffectPresets.Add(template.TargetStatusID, cached);
                    }
                }

                newEffect = original;
            }
            else
            {
                // instantiate original and use that as newEffect
                newEffect = GameObject.Instantiate(original.gameObject).GetComponent<StatusEffect>();
                newEffect.gameObject.SetActive(false);

                // Set Status identifier
                At.SetValue(template.StatusIdentifier, typeof(StatusEffect), newEffect, "m_identifierName");

                if (preset)
                {
                    // Set Preset ID
                    At.SetValue(template.NewStatusID, typeof(EffectPreset), preset, "m_StatusEffectID");
                }

                // Fix localization
                GetStatusLocalization(original, out string name, out string desc);
                SetStatusLocalization(newEffect, name, desc);

                // Fix status data and stack
                At.SetValue<List<StatusData>>(null, typeof(StatusEffect), newEffect, "m_statusStack");
                //At.SetValue<StatusData.EffectData[]>(null, typeof(StatusEffect), newEffect, "m_totalData");
            }

            int presetID = newEffect.GetComponent<EffectPreset>()?.PresetID ?? -1;

            var id = "";
            if (presetID > 0)
            {
                id += presetID + "_";
            }
            newEffect.gameObject.name = id + newEffect.IdentifierName;

            // fix RPM_STATUS_EFFECTS dictionary
            if (!References.RPM_STATUS_EFFECTS.ContainsKey(newEffect.IdentifierName))
            {
                References.RPM_STATUS_EFFECTS.Add(newEffect.IdentifierName, newEffect);
            }
            else
            {
                References.RPM_STATUS_EFFECTS[newEffect.IdentifierName] = newEffect;
            }

            // fix RPM_Presets dictionary
            if (template.NewStatusID > 0)
            {
                if (!References.RPM_EFFECT_PRESETS.ContainsKey(template.NewStatusID))
                {
                    References.RPM_EFFECT_PRESETS.Add(template.NewStatusID, newEffect.GetComponent<EffectPreset>());
                }
                else
                {
                    //SL.Log("A Status Effect already exists with the Identifier " + template.StatusIdentifier + ", replacing with " + template.Name);
                    References.RPM_EFFECT_PRESETS[template.NewStatusID] = newEffect.GetComponent<EffectPreset>();
                }
            }

            // Always do this
            GameObject.DontDestroyOnLoad(newEffect.gameObject);

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

            var nameKey = $"NAME_{effect.IdentifierName}";
            At.SetValue(nameKey, typeof(StatusEffect), effect, "m_nameLocKey");

            if (References.GENERAL_LOCALIZATION.ContainsKey(nameKey))
            {
                References.GENERAL_LOCALIZATION[nameKey] = name;
            }
            else
            {
                References.GENERAL_LOCALIZATION.Add(nameKey, name);
            }

            var descKey = $"DESC_{effect.IdentifierName}";
            At.SetValue(descKey, typeof(StatusEffect), effect, "m_descriptionLocKey");

            if (References.GENERAL_LOCALIZATION.ContainsKey(descKey))
            {
                References.GENERAL_LOCALIZATION[descKey] = description;
            }
            else
            {
                References.GENERAL_LOCALIZATION.Add(descKey, description);
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
                At.SetValue(template.NewStatusID, typeof(EffectPreset), newEffect, "m_StatusEffectID");

                // Fix localization
                GetImbueLocalization(original, out string name, out string desc);
                SetImbueLocalization(newEffect, name, desc);
            }

            newEffect.gameObject.name = template.NewStatusID + "_" + (template.Name ?? newEffect.Name);

            // fix RPM_Presets dictionary
            if (!References.RPM_EFFECT_PRESETS.ContainsKey(template.NewStatusID))
            {
                References.RPM_EFFECT_PRESETS.Add(template.NewStatusID, newEffect.GetComponent<EffectPreset>());
            }
            else
            {
                //SL.Log("An imbue already exists with the ID " + template.NewStatusID + ", replacing...");
                References.RPM_EFFECT_PRESETS[template.NewStatusID] = newEffect;
            }

            // Always do this
            GameObject.DontDestroyOnLoad(newEffect.gameObject);

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

            if (References.GENERAL_LOCALIZATION.ContainsKey(nameKey))
            {
                References.GENERAL_LOCALIZATION[nameKey] = name;
            }
            else
            {
                References.GENERAL_LOCALIZATION.Add(nameKey, name);
            }

            var descKey = $"DESC_{preset.PresetID}_{preset.Name.Trim()}";
            At.SetValue(descKey, typeof(ImbueEffectPreset), preset, "m_imbueDescKey");

            if (References.GENERAL_LOCALIZATION.ContainsKey(descKey))
            {
                References.GENERAL_LOCALIZATION[descKey] = description;
            }
            else
            {
                References.GENERAL_LOCALIZATION.Add(descKey, description);
            }

            if (preset.GetComponent<StatusEffect>() is StatusEffect status)
            {
                SetStatusLocalization(status, name, description);
            }
        }
    }
}
