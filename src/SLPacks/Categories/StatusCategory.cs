using SideLoader.Model;
using SideLoader.Model.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class StatusCategory : SLPackTemplateCategory<SL_StatusBase>, ITemplateCategory
    {
        public override string FolderName => "StatusEffects";

        public override int LoadOrder => 10;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
			var status = (SL_StatusBase)template;

			status.ApplyActualTemplate();
		}

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
