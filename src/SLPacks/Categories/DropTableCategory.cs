using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class DropTableCategory : SLPackTemplateCategory<SL_DropTable>
    {
        public override string FolderName => "DropTables";

        public override int LoadOrder => 25;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var table = template as SL_DropTable;

            table.ApplyActualTemplate();
        }

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
