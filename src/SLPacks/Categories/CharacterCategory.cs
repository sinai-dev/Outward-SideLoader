using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class CharacterCategory : SLPackTemplateCategory<SL_Character>
    {
        public override string FolderName => "Characters";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.IndependantLast;

        public override void ApplyTemplate(ContentTemplate template)
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

        protected internal override void OnHotReload()
        {
            CustomCharacters.Templates.Clear();
        }
    }
}
