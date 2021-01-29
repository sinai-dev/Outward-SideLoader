using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class DropTableCategory : SLPackTemplateCategory<SL_DropTable>
    {
        public override string FolderName => "DropTables";

        public override int LoadOrder => 25;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var table = template as SL_DropTable;

            table.ApplyActualTemplate();
        }

        protected internal override void OnHotReload()
        {
            SL_DropTable.s_registeredTables.Clear();
        }
    }
}
