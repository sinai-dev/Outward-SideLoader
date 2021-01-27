﻿using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks.Categories
{
    public class ItemCategory : SLPackTemplateCategory<SL_Item>
    {
        public override string FolderName => "Items";

        public override int LoadOrder => 15;

        public override void ApplyTemplate(IContentTemplate template, SLPack pack)
        {
            var item = template as SL_Item;

            item.ApplyActualTemplate();
        }

        public override bool ShouldApplyLate(IContentTemplate template) => (template as SL_Item).ShouldApplyLate;
    }
}