using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SideLoader.Helpers;

namespace SideLoader
{
    /// <summary>
    /// Used internally by SideLoader.
    /// </summary>
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

        public SL_CharacterSaveData ToSaveData() => SL_CharacterSaveData.FromSpawnInfo(this);
    }
}
