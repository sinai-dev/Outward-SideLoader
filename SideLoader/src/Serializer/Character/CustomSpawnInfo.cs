using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SideLoader.Helpers;

namespace SideLoader
{
    public struct CustomSpawnInfo
    {
        public SL_Character Template;
        public Character ActiveCharacter;
        public string ExtraRPCData;

        public CustomSpawnInfo(Character character, string templateUID, string extraRPCData)
        {
            this.ActiveCharacter = character;
            this.ExtraRPCData = extraRPCData;
            CustomCharacters.Templates.TryGetValue(templateUID, out this.Template);
        }

        public SL_CharacterSaveData ToSaveData()
        {
            // should probably debug this if it happens
            if (this.Template == null || !this.ActiveCharacter)
            {
                SL.LogWarning("Trying to save a CustomSpawnInfo, but template or activeCharacter is null!"); 
                return null;
            }

            var character = this.ActiveCharacter;
            var template = this.Template;

            // capture the save data in an instance
            var data = new SL_CharacterSaveData()
            {
                SaveType = template.SaveType,
                CharacterUID = character.UID,
                TemplateUID = template.UID,
                Forward = character.transform.forward,
                Position = character.transform.position,
                ExtraRPCData = this.ExtraRPCData,
            };

            try 
            {
                data.Health = character.Health;

                if (character.StatusEffectMngr)
                {
                    var statuses = character.StatusEffectMngr.Statuses.ToArray().Where(it => !string.IsNullOrEmpty(it.IdentifierName));
                    data.StatusData = new string[statuses.Count()];

                    int i = 0;
                    foreach (var status in statuses)
                    {
                        var sourceChar = (UID)At.GetField("m_sourceCharacterUID", status)?.ToString();
                        data.StatusData[i] = $"{status.IdentifierName}|{sourceChar}|{status.RemainingLifespan}";
                        i++;
                    }
                }

            } catch { }

            return data;
        }
    }
}
