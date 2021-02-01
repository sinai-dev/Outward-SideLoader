using SideLoader.Model;
using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public abstract class SL_ItemSource : ContentTemplate
    {
        #region Content Template

        public override ITemplateCategory PackCategory => SLPackManager.GetCategoryInstance<ItemSourceCategory>();
        public override string DefaultTemplateName => "Untitled ItemSource";

        #endregion

        public string IdentifierName = "";

        public override void ApplyActualTemplate()
        {
            SL.Log("Registering SL_ItemSource '" + this.IdentifierName + "' (" + this.GetType() + ")");
        }
    }
}
