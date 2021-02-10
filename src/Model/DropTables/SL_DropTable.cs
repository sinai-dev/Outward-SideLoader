using SideLoader.Model;
using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_DropTable : ContentTemplate
    {
        #region IContentTemplate

        public override ITemplateCategory PackCategory => SLPackManager.GetCategoryInstance<DropTableCategory>();
        public override string DefaultTemplateName => "Untitled Droptable";

        public override void ApplyActualTemplate() => Internal_ApplyTemplate();

        #endregion

        // ~~~~~~ User-Defined ~~~~~~

        public string UID = "";

        public List<SL_ItemDrop> GuaranteedDrops = new List<SL_ItemDrop>();

        public List<SL_RandomDropGenerator> RandomTables = new List<SL_RandomDropGenerator>();

        // ~~~~~~ Internal ~~~~~~

        /// <summary>
        /// Internal dictionary, only use this as a reference, do not modify it directly unless you're sure you know what you're doing.
        /// </summary>
        public static readonly Dictionary<string, SL_DropTable> s_registeredTables = new Dictionary<string, SL_DropTable>();

        public override void ApplyTemplate()
        {
            base.ApplyTemplate();
        }

        internal void Internal_ApplyTemplate()
        {
            if (string.IsNullOrEmpty(this.UID))
            {
                SL.LogWarning("Cannot prepare an SL_DropTable with a null or empty UID!");
                return;
            }

            if (s_registeredTables.ContainsKey(this.UID))
            {
                SL.LogWarning("Trying to register an SL_DropTable but one already exists with this UID: " + this.UID);
                return;
            }

            s_registeredTables.Add(this.UID, this);
            SL.Log("Registered SL_DropTable '" + this.UID + "'");
        }

        public void GenerateDrops(Transform container)
        {
            if (!container)
            {
                SL.LogWarning($"Trying to generate drops from '{UID}' but target container is null!");
                return;
            }

            if (this.GuaranteedDrops != null && GuaranteedDrops.Count > 0)
            {
                //SL.Log("Generating Guaranteed drops...");

                foreach (var drop in this.GuaranteedDrops)
                    drop.GenerateDrop(container);
            }

            if (this.RandomTables != null && RandomTables.Count > 0)
            {
                //SL.Log("Generating Random Tables...");

                foreach (var table in this.RandomTables)
                    table.GenerateDrops(container);
            }
        }
    }
}
