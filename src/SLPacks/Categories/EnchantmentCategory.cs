﻿using SideLoader.Model;

namespace SideLoader.SLPacks.Categories
{
    public class EnchantmentCategory : SLPackTemplateCategory<SL_EnchantmentRecipe>
    {
        public override string FolderName => "Enchantments";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.Recipe;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var enchant = template as SL_EnchantmentRecipe;
            enchant.ApplyActualTemplate();
        }

        //public override bool ShouldApplyLate(IContentTemplate template) => false;
    }
}
