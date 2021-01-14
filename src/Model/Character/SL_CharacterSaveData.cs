using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using SideLoader.Helpers;
using System.Collections;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_CharacterSaveData
    {
        public CharSaveType SaveType;
        public string CharacterUID;
        public string TemplateUID;

        public string FollowTargetUID;

        public Vector3 Forward;
        public Vector3 Position;

        public float Health;
        public string[] StatusData;

        public string ExtraRPCData;

        public void ApplyToCharacter(Character character)
        {
            if (!CustomCharacters.Templates.TryGetValue(this.TemplateUID, out SL_Character template))
            {
                SL.LogWarning($"Trying to apply an SL_CharacterSaveData to a Character, but could not get any template with the UID '{this.TemplateUID}'");
                return;
            }

            SLPlugin.Instance.StartCoroutine(ApplyCoroutine(character, template));
        }

        internal IEnumerator ApplyCoroutine(Character character, SL_Character template)
        {
            yield return new WaitForSeconds(0.5f);

            if (!string.IsNullOrEmpty(FollowTargetUID))
            {
                var followTarget = CharacterManager.Instance.GetCharacter(FollowTargetUID);
                var aisWander = character.GetComponentInChildren<AISWander>();
                if (followTarget && aisWander)
                    aisWander.FollowTransform = followTarget.transform;
            }

            character.transform.position = this.Position;
            character.transform.forward = this.Forward;

            if (character.GetComponent<CharacterStats>() is CharacterStats stats)
                stats.SetHealth(this.Health);

            if (this.StatusData != null)
            {
                var statusMgr = character.GetComponentInChildren<StatusEffectManager>(true);
                if (statusMgr)
                {
                    foreach (var statusData in this.StatusData)
                    {
                        var data = statusData.Split('|');

                        var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(data[0]);
                        if (!status)
                            continue;

                        var dealer = CharacterManager.Instance.GetCharacter(data[1]);
                        var effect = statusMgr.AddStatusEffect(status, dealer);

                        var remaining = float.Parse(data[2]);
                        At.SetField(effect, "m_remainingTime", remaining);
                        if (effect.StatusData != null)
                            At.SetField(effect.StatusData, "m_remainingLifespan", remaining);
                    }
                }
            }

            template.INTERNAL_OnSaveApplied(character, this.ExtraRPCData);
        }
    }
}
