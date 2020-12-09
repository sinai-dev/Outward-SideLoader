using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_CharacterSaveData
    {
        public CharSaveType SaveType;
        public string CharacterUID;
        public string TemplateUID;

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

            if (this.SaveType == CharSaveType.Follower)
                character.transform.position = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
            else
                character.transform.position = this.Position;

            character.transform.forward = this.Forward;

            character.GetComponent<CharacterStats>().SetHealth(this.Health);

            if (this.StatusData != null)
            {
                var statusMgr = character.GetComponentInChildren<StatusEffectManager>(true);

                foreach (var statusData in this.StatusData)
                {
                    var data = statusData.Split('|');

                    var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(data[0]);
                    if (!status)
                        continue;

                    var dealer = CharacterManager.Instance.GetCharacter(data[1]);
                    var effect = statusMgr.AddStatusEffect(status, dealer);

                    var remaining = float.Parse(data[2]);
                    At.SetValue(remaining, "m_remainingTime", effect);
                    if (effect.StatusData != null)
                        At.SetValue(remaining, "m_remainingLifespan", effect.StatusData);
                }
            }

            template.INTERNAL_OnSaveApplied(character, this.ExtraRPCData);
        }
    }
}
