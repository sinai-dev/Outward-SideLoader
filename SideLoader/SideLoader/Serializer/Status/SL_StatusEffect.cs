using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    public class SL_StatusEffect
    {
        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom status template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore]
        public string SLPackName;
        /// <summary> [NOT SERIALIZED] The name of the folder this custom status is using for the icon.png (MyPack/StatusEffects/[SubfolderName]/icon.png).</summary>
        [XmlIgnore]
        public string SubfolderName;

        /// <summary>This is the Preset ID of the Status Effect you want to base from.</summary>
        public int TargetStatusID;
        /// <summary>The new Preset ID for your Status Effect</summary>
        public int NewStatusID;
        /// <summary>The new Status Identifier name for your Status Effect. Used by ResourcesPrefabManager.GetStatusEffect(string identifier)</summary>
        public string StatusIdentifier;

        public string Name;
        public string Description;

        public float? Lifespan;
        public float? RefreshRate;
        //public StatusEffectFamily.LengthTypes? LengthType; // need to add EffectFamily support for this
        
        public float? BuildupRecoverySpeed;
        public bool? IgnoreBuildupIfApplied;

        public bool? DisplayedInHUD;
        public bool? IsHidden;

        public List<string> Tags;

        public SL_Item.TemplateBehaviour EffectsBehaviour = SL_Item.TemplateBehaviour.OverrideEffects;
        public List<SL_EffectTransform> Effects;

        public virtual void ApplyTemplate()
        {
            var preset = ResourcesPrefabManager.Instance.GetEffectPreset(NewStatusID);

            if (!preset)
            {
                SL.Log("Could not find a StatusEffect with the PresetID " + NewStatusID, 1);
                return;
            }

            SL.Log("Applying Status Effect template, ID " + NewStatusID + ", " + Name ?? preset.name);

            var status = preset.GetComponent<StatusEffect>();

            CustomStatusEffects.SetStatusLocalization(status, Name, Description);

            if (Lifespan != null)
            {
                var data = status.StatusData;
                data.LifeSpan = (float)Lifespan;
            }

            if (RefreshRate != null)
            {
                status.RefreshRate = (float)RefreshRate;
            }

            //if (LengthType != null)
            //{
            //    status.LengthType
            //}

            if (BuildupRecoverySpeed != null)
            {
                status.BuildUpRecoverSpeed = (float)BuildupRecoverySpeed;
            }

            if (IgnoreBuildupIfApplied != null)
            {
                status.IgnoreBuildUpIfApplied = (bool)IgnoreBuildupIfApplied;
            }

            if (DisplayedInHUD != null)
            {
                status.DisplayInHud = (bool)DisplayedInHUD;
            }

            if (IsHidden != null)
            {
                status.IsHidden = (bool)IsHidden;
            }

            if (Tags != null)
            {
                var tagList = (List<Tag>)At.GetValue(typeof(StatusEffect), status, "m_tags");
                tagList.Clear();
                foreach (var tagName in Tags)
                {
                    if (CustomItems.GetTag(tagName) is Tag tag && tag != Tag.None)
                    {
                        tagList.Add(tag);
                    }
                }
            }

            // check for custom icon
            if (!string.IsNullOrEmpty(SLPackName) && !string.IsNullOrEmpty(SubfolderName) && SL.Packs[SLPackName] is SLPack pack)
            {
                var path = pack.GetSubfolderPath(SLPack.SubFolders.StatusEffects) + "\\" + SubfolderName + "\\icon.png";

                if (File.Exists(path))
                {
                    var tex = CustomTextures.LoadTexture(path, false, false);
                    var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.NONE);

                    status.OverrideIcon = sprite;
                }
            }

            if (EffectsBehaviour == SL_Item.TemplateBehaviour.DestroyEffects)
            {
                CustomItems.DestroyChildren(status.transform);
            }

            if (Effects != null)
            {
                var signature = status.transform.GetChild(0);
                if (!signature)
                {
                    signature = new GameObject($"SIGNATURE_{status.IdentifierName}").transform;
                    signature.parent = status.transform;
                    var comp = signature.gameObject.AddComponent<EffectSignature>();
                    comp.SignatureUID = new UID($"{NewStatusID}_{status.IdentifierName}");
                }

                foreach (var effectTransform in Effects)
                {
                    if (EffectsBehaviour == SL_Item.TemplateBehaviour.OverrideEffects && signature.Find(effectTransform.TransformName) is Transform existing)
                    {
                        GameObject.DestroyImmediate(existing.gameObject);
                        var newObj = new GameObject(effectTransform.TransformName);
                        newObj.transform.parent = signature.transform;
                    }

                    effectTransform.ApplyToTransform(signature);
                }

                // fix StatusData for the new effects
                CompileEffectsToData(status);
            }
        }

        // There is no opposite of Effect.SetValue (you'd think it would be Effect.CompileData, but no...), so we have to do this manually.
        // I think the StatusData is only needed for PunctualDamage and AffectX components as far as I can tell.
        private static void CompileEffectsToData(StatusEffect status)
        {
            // Get the EffectSignature component
            var signature = status.GetComponentInChildren<EffectSignature>();

            // Create a list to generate the EffectData[] array into.
            var list = new List<StatusData.EffectData>();

            // Get and Set the Effects list
            var effects = signature.GetComponentsInChildren<Effect>()?.ToList() ?? new List<Effect>();
            signature.Effects = effects;

            // Iterate over the effects
            foreach (var effect in effects)
            {
                // Create a blank holder, in the case this effect isn't supported or doesn't serialize anything.
                var data = new StatusData.EffectData()
                {
                    Data = new string[0]
                };

                var type = effect.GetType();

                if (typeof(PunctualDamage).IsAssignableFrom(type))
                {
                    var comp = effect as PunctualDamage;

                    // For PunctualDamage, Data[0] is the entire damage, and Data[1] is the impact.

                    // Each damage goes "Damage : Type", and separated by a ';' char.
                    // So we just iterate over the damage and serialize like that.
                    var dmgString = "";
                    foreach (var dmg in comp.Damages)
                    {
                        if (dmgString != "")
                            dmgString += ";";

                        dmgString += dmg.ToString(":");
                    }

                    // finally, set data
                    data.Data = new string[]
                    {
                        dmgString,
                        comp.Knockback.ToString()
                    };
                }
                // For most AffectStat components, the only thing that is serialized is the AffectQuantity.
                else if (type.GetField("AffectQuantity", At.FLAGS) is FieldInfo fi_AffectQuantity)
                {
                    data.Data = new string[]
                    {
                        fi_AffectQuantity.GetValue(effect)?.ToString()
                    };
                }
                // AffectMana uses "Value" instead of AffectQuantity for some reason...
                else if (type.GetField("Value", At.FLAGS) is FieldInfo fi_Value)
                {
                    data.Data = new string[]
                    {
                        fi_Value.GetValue(effect)?.ToString()
                    };
                }
                else // otherwise I need to add support for this effect (maybe).
                {
                    SL.Log("[StatusEffect] Unsupported effect: " + type, 1);
                }

                list.Add(data);
            }

            // Finally, set the EffectsData[] array.
            status.StatusData.EffectsData = list.ToArray();

            // Not sure if this is needed or not, but I'm doing it to be extra safe.
            At.SetValue(status.StatusData.EffectsData, typeof(StatusEffect), status, "m_totalData");
        }

        public static SL_StatusEffect ParseStatusEffect(StatusEffect status)
        {
            var preset = status.GetComponent<EffectPreset>();
            if (!preset)
            {
                SL.Log("This StatusEffect does not have an EffectPreset component, we can't serialize this yet sorry!", 1);
                return null;
            }

            var template = new SL_StatusEffect()
            {
                TargetStatusID = preset.PresetID,
                StatusIdentifier = status.IdentifierName,
                IgnoreBuildupIfApplied = status.IgnoreBuildUpIfApplied,
                BuildupRecoverySpeed = status.BuildUpRecoverSpeed,
                DisplayedInHUD = status.DisplayInHud,
                IsHidden = status.IsHidden,
                //LengthType = status.LengthType,
                Lifespan = status.StatusData.LifeSpan,
                RefreshRate = status.RefreshRate
            };

            CustomStatusEffects.GetStatusLocalization(status, out template.Name, out template.Description);

            template.Tags = new List<string>();
            status.InitTags();
            var tags = (List<Tag>)At.GetValue(typeof(StatusEffect), status, "m_tags");
            foreach (var tag in tags)
            {
                template.Tags.Add(tag.TagName);
            }

            // For existing StatusEffects, the StatusData contains the real values, so we need to SetValue to each Effect.
            var statusData = status.StatusData.EffectsData;
            var components = status.GetComponentsInChildren<Effect>();
            for (int i = 0; i < components.Length; i++)
            {
                var comp = components[i];
                if (comp && comp.Signature.Length > 0)
                {
                    comp.SetValue(statusData[i].Data);
                }
            }

            template.Effects = new List<SL_EffectTransform>();
            var signature = status.transform.GetChild(0);
            if (signature)
            {
                foreach (Transform child in signature.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0) // || effectsChild.EffectConditions.Count > 0)
                    {
                        template.Effects.Add(effectsChild);
                    }
                }
            }

            return template;
        }
    }
}
