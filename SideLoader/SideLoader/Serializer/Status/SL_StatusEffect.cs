using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_StatusEffect
    {
        /// <summary> [NOT SERIALIZED] The name of the SLPack this custom status template comes from (or is using).
        /// If defining from C#, you can set this to the name of the pack you want to load assets from.</summary>
        [XmlIgnore] public string SLPackName;
        /// <summary> [NOT SERIALIZED] The name of the folder this custom status is using for the icon.png (MyPack/StatusEffects/[SubfolderName]/icon.png).</summary>
        [XmlIgnore] public string SubfolderName;

        /// <summary> The StatusEffect you would like to clone from. Can also use TargetStatusID (checks for a Preset ID), but this takes priority.</summary>
        public string TargetStatusIdentifier;
        /// <summary>[Optional] Used if SideLoader could not find anything with your TargetStatusIdentifier.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TargetStatusID;
        /// <summary>Internal C# override, so that TargetStatusID is not included in generated XML templates.</summary>
        public bool ShouldSerializeTargetStatusID() { return false; }

        /// <summary>The new Preset ID for your Status Effect.</summary>
        public int NewStatusID;
        /// <summary>The new Status Identifier name for your Status Effect. Used by ResourcesPrefabManager.GetStatusEffect(string identifier)</summary>
        public string StatusIdentifier;

        public string Name;
        public string Description;

        public float? Lifespan;
        public float? RefreshRate;
        public StatusEffectFamily.LengthTypes? LengthType;

        public string AmplifiedStatusIdentifier;

        public float? BuildupRecoverySpeed;
        public bool? IgnoreBuildupIfApplied;

        public bool? DisplayedInHUD;
        public bool? IsHidden;
        public bool? IsMalusEffect;

        public string[] Tags;

        public EffectBehaviours EffectBehaviour = EffectBehaviours.OverrideEffects;
        public SL_EffectTransform[] Effects;

        public virtual void ApplyTemplate()
        {
            StatusEffect status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusIdentifier);
            if (!status)
            {
                SL.Log("Could not find a StatusEffect with the Identifier: " + StatusIdentifier, 1);
                return;
            }

            SL.Log("Applying Status Effect template: " + Name ?? status.name);

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

            if (IsMalusEffect != null)
            {
                status.IsMalusEffect = (bool)this.IsMalusEffect;
            }

            if (!string.IsNullOrEmpty(this.AmplifiedStatusIdentifier))
            {
                var amp = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(AmplifiedStatusIdentifier);
                if (amp)
                {
                    At.SetValue(amp, typeof(StatusEffect), status, "m_amplifiedStatus");
                }
                else
                {
                    SL.Log("StatusEffect.ApplyTemplate - could not find AmplifiedStatusIdentifier " + this.AmplifiedStatusIdentifier);
                }
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

            if (EffectBehaviour == EffectBehaviours.DestroyEffects)
            {
                SL.DestroyChildren(status.transform);
            }

            // setup family and length type
            StatusEffectFamily.LengthTypes lengthType = status.EffectFamily?.LengthType ?? StatusEffectFamily.LengthTypes.Short;
            if (LengthType != null)
            {
                lengthType = (StatusEffectFamily.LengthTypes)LengthType;
            }

            bool editingOrig = this.StatusIdentifier == this.TargetStatusIdentifier;
            if (!editingOrig)
            {
                var family = new StatusEffectFamily
                {
                    Name = this.StatusIdentifier + "_FAMILY",
                    LengthType = lengthType,
                    MaxStackCount = 1,
                    StackBehavior = StatusEffectFamily.StackBehaviors.IndependantUnique
                };

                At.SetValue(StatusEffect.FamilyModes.Bind, typeof(StatusEffect), status, "m_familyMode");
                At.SetValue(family, typeof(StatusEffect), status, "m_bindFamily");
            }
            else
            {
                if (status.EffectFamily != null)
                {
                    status.EffectFamily.LengthType = lengthType;
                }
            }

            // setup signature and finalize

            Transform signature;
            if (status.transform.childCount < 1)
            {
                signature = new GameObject($"SIGNATURE_{status.IdentifierName}").transform;
                signature.parent = status.transform;
                var comp = signature.gameObject.AddComponent<EffectSignature>();
                comp.SignatureUID = new UID($"{NewStatusID}_{status.IdentifierName}");
            }
            else
            {
                signature = status.transform.GetChild(0);
            }

            if (Effects != null)
            {
                if (signature)
                {
                    SL_EffectTransform.ApplyTransformList(signature, Effects, EffectBehaviour);
                }
                else
                {
                    SL.Log("Could not get effect signature!", 1);
                }
            }

            // fix StatusData for the new effects
            CompileEffectsToData(status);
        }

        // There is no opposite of Effect.SetValue (you'd think it would be Effect.CompileData, but no...), so we have to do this manually.
        // I think the StatusData is only needed for PunctualDamage and AffectX components as far as I can tell.
        public static void CompileEffectsToData(StatusEffect status)
        {
            // Get the EffectSignature component
            var signature = status.GetComponentInChildren<EffectSignature>();

            // Create a list to generate the EffectData[] array into.
            var list = new List<StatusData.EffectData>();

            // Get and Set the Effects list
            var effects = signature.GetComponentsInChildren<Effect>()?.ToList() ?? new List<Effect>();
            signature.Effects = effects;

            status.StatusData.EffectSignature = signature;

            // Iterate over the effects
            foreach (var effect in effects)
            {
                // Create a blank holder, in the case this effect isn't supported or doesn't serialize anything.
                var data = new StatusData.EffectData()
                {
                    Data = new string[]
                    {
                        "0"
                    }
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
                    //SL.Log("[StatusEffect] Unsupported effect: " + type, 1);
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

            var template = new SL_StatusEffect()
            {
                TargetStatusID = preset?.PresetID ?? -1,
                TargetStatusIdentifier = status.IdentifierName,
                StatusIdentifier = status.IdentifierName,
                IgnoreBuildupIfApplied = status.IgnoreBuildUpIfApplied,
                BuildupRecoverySpeed = status.BuildUpRecoverSpeed,
                DisplayedInHUD = status.DisplayInHud,
                IsHidden = status.IsHidden,
                LengthType = status.LengthType,
                Lifespan = status.StatusData.LifeSpan,
                RefreshRate = status.RefreshRate,
                AmplifiedStatusIdentifier = status.AmplifiedStatus?.IdentifierName ?? "",
            };

            CustomStatusEffects.GetStatusLocalization(status, out template.Name, out template.Description);

            var tagList = new List<string>();
            status.InitTags();
            var tags = (List<Tag>)At.GetValue(typeof(StatusEffect), status, "m_tags");
            foreach (var tag in tags)
            {
                tagList.Add(tag.TagName);
            }
            template.Tags = tagList.ToArray();

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

            var effects = new List<SL_EffectTransform>();
            var signature = status.transform.GetChild(0);
            if (signature)
            {
                foreach (Transform child in signature.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.HasContent)
                    {
                        effects.Add(effectsChild);
                    }
                }
            }
            template.Effects = effects.ToArray();

            return template;
        }
    }
}
