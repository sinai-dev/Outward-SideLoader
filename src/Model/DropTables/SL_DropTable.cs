using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_DropTable : IContentTemplate<string> 
    {
        #region IContentTemplate

        public string TargetID => this.UID;
        public string AppliedID => this.UID;

        public string SerializedSLPackName 
        {
            get => m_serializedSLPackName;
            set => m_serializedSLPackName = value;
        }
        
        public string SerializedFilename 
        { 
            get => m_serializedFilename; 
            set => m_serializedFilename = value;
        }

        public string SerializedSubfolderName
        {
            get => null;
            set { }
        }

        public bool IsCreatingNewID => true;
        public bool DoesTargetExist => true;

        public bool CanParseContent => false;

        public SLPack.SubFolders SLPackCategory => SLPack.SubFolders.DropTables;

        public bool TemplateAllowedInSubfolder => false;

        public string DefaultTemplateName => "Untitled Droptable";

        public void CreateContent() => Prepare();

        public IContentTemplate ParseToTemplate(object content) => throw new NotImplementedException();
        public object GetContentFromID(object id) => throw new NotImplementedException();

        #endregion

        // ~~~~~~ User-Defined ~~~~~~

        public string UID = "";

        public List<SL_ItemDrop> GuaranteedDrops = new List<SL_ItemDrop>();

        public List<SL_RandomDropGenerator> RandomTables = new List<SL_RandomDropGenerator>();

        internal string m_serializedSLPackName;
        internal string m_serializedFilename;

        public void Prepare()
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

        // ~~~~~~ Internal ~~~~~~

        internal static readonly Dictionary<string, SL_DropTable> s_registeredTables = new Dictionary<string, SL_DropTable>();

        public void GenerateDrops(Transform container)
        {
            if (!container)
            {
                SL.LogWarning($"Trying to generate drops from '{UID}' but target container is null!");
                return;
            }

            if (this.GuaranteedDrops != null)
            {
                //SL.Log("Generating Guaranteed drops...");

                foreach (var drop in this.GuaranteedDrops)
                    drop.GenerateDrop(container);
            }

            if (this.RandomTables != null)
            {
                //SL.Log("Generating Random Tables...");

                foreach (var table in this.RandomTables)
                    table.GenerateDrops(container);
            }
        }
    }
}
