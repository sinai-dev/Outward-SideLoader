using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SaveData
{
    public class SLSaveManager
    {
        internal const string SAVEDATA_FOLDER = SL.SL_FOLDER + @"\_SAVEDATA";

        internal const string CHARACTERS_FOLDER = "Characters";
        internal const string CUSTOM_FOLDER = "Custom";

        public static string GetSaveFolderForWorldHost()
        {
            var player = CharacterManager.Instance.GetFirstLocalCharacter();
            var host = CharacterManager.Instance.GetWorldHostCharacter();

            if (!player || !host || player != host)
                return null;

            return GetSaveFolderForCharacter(player);
        }

        public static string GetSaveFolderForCharacter(Character character)
        {
            var ret = $@"{SAVEDATA_FOLDER}\{character.UID}";

            // Create the base folder structure for this player character (does nothing if already exists)
            Directory.CreateDirectory(ret);
            Directory.CreateDirectory(ret + $@"\{CHARACTERS_FOLDER}");
            Directory.CreateDirectory(ret + $@"\{CUSTOM_FOLDER}");

            return ret;
        }
    }
}
