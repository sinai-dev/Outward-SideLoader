using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class CharacterCategory : SLPackTemplateCategory<SL_Character>
    {
        public override string FolderName => "Characters";

        public override int LoadOrder => 25;

        public override void ApplyTemplate(IContentTemplate template)
        {
            var character = template as SL_Character;
            character.ApplyActualTemplate();

            if (!string.IsNullOrEmpty(character.SerializedSLPackName))
            {
                var pack = SL.GetSLPack(character.SerializedSLPackName);
                if (pack.CharacterTemplates.ContainsKey(character.UID))
                    SL.LogWarning("Loaded a dupliate UID SL_Character! UID: " + character.UID);
                else
                    pack.CharacterTemplates.Add(character.UID, character);
            }
        }

        //public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
