using SideLoader.Model;
using SideLoader.Model.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class StatusFamilyCategory : SLPackTemplateCategory<SL_StatusEffectFamily>
    {
        public override string FolderName => "StatusFamilies";

        public override int LoadOrder => 5;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var family = template as SL_StatusEffectFamily;
            family.Apply();
        }

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
