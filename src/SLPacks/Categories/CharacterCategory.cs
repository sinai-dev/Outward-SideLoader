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
        }

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
