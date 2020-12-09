using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SLSaveManager
    {
        internal const string SAVEDATA_FOLDER = SL.SL_FOLDER + @"\_SAVEDATA";

        internal const string CHARACTERS_FOLDER = "Characters";
    
        public static string GetSaveFolderForWorldHost()
        {
            var player = CharacterManager.Instance.GetFirstLocalCharacter();

            if (!player)
                return null;

            var ret = $@"{SAVEDATA_FOLDER}\{player.UID.ToString()}";

            // Create the base folder structure for this player character
            if (!Directory.Exists(ret))
            {
                Directory.CreateDirectory(ret);
                Directory.CreateDirectory(ret + $@"\{CHARACTERS_FOLDER}");
            }

            return ret;
        }
    }
}
