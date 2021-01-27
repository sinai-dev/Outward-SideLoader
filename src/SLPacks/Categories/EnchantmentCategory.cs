using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class EnchantmentCategory : SLPackTemplateCategory<SL_EnchantmentRecipe>
    {
        public override string FolderName => "Enchantments";

        public override int LoadOrder => 20;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var enchant = template as SL_EnchantmentRecipe;
            enchant.ApplyActualTemplate();
        }

        public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
