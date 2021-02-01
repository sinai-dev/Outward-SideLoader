using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class ItemSourceCategory : SLPackTemplateCategory<SL_ItemSource>
    {
        public override string FolderName => "ItemSources";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.IndependantLast;

        public override void ApplyTemplate(ContentTemplate template)
        {
            template.ApplyActualTemplate();
        }

        protected internal override void OnHotReload()
        {
            SL_ItemSpawn.s_registeredSpawnSources.Clear();
            SL_DropTableAddition.s_registeredDropTableSources.Clear();
        }
    }
}
