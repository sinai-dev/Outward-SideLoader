using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class CharacterCategory : SLPackTemplateCategory<SL_Character>
    {
        public override string FolderName => "Characters";

        public override int LoadOrder => 25;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var character = template as SL_Character;
            character.ApplyActualTemplate();
            
            if (pack.CharacterTemplates.ContainsKey(character.UID))
                SL.LogWarning("Loaded a dupliate UID SL_Character! UID: " + character.UID);
            else
                pack.CharacterTemplates.Add(character.UID, character);
        }

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
