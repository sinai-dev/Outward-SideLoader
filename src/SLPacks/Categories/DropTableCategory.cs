using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class DropTableCategory : SLPackTemplateCategory<SL_DropTable>
    {
        public override string FolderName => "DropTables";

        public override int LoadOrder => 25;

        public override void ApplyTemplate(IContentTemplate template)
        {
            var table = template as SL_DropTable;

            table.ApplyActualTemplate();
        }

        //public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
