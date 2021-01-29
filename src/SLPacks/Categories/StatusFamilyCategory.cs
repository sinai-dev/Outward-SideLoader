using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class StatusFamilyCategory : SLPackTemplateCategory<SL_StatusEffectFamily>
    {
        public override string FolderName => "StatusFamilies";

        public override int LoadOrder => 5;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var family = template as SL_StatusEffectFamily;
            family.ApplyActualTemplate();
        }

        //public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
