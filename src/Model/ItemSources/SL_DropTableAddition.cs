using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    public class SL_DropTableAddition : SL_ItemSource
    {
        internal static readonly HashSet<SL_DropTableAddition> s_registeredDropTableSources = new HashSet<SL_DropTableAddition>();

        public List<string> SelectorTargets = new List<string>();

        public List<string> DropTableUIDsToAdd = new List<string>();

        public override void ApplyActualTemplate()
        {
            base.ApplyActualTemplate();

            s_registeredDropTableSources.Add(this);
        }

        public bool IsTargeting(string targetUID)
        {
            if (SelectorTargets == null)
                return false;

            return SelectorTargets.Any(it => it == targetUID);
        }

        public void GenerateItems(Transform container)
        {
            if (this.DropTableUIDsToAdd == null)
            {
                SL.LogWarning($"Trying to generate drops from an SL_DropTableAddition '{IdentifierName}', but the DropTableUIDsToAdd is null!");
                return;
            }

            foreach (string tableUID in this.DropTableUIDsToAdd)
            {
                SL_DropTable.s_registeredTables.TryGetValue(tableUID, out SL_DropTable table);

                if (table == null)
                {
                    SL.LogWarning($"SL_DropTableAddition: Could not find any SL_DropTable with UID '{tableUID}'!");
                    continue;
                }

                //SL.Log("Generating from '" + table.UID + "'");
                table.GenerateDrops(container);
            }
        }
    }
}
