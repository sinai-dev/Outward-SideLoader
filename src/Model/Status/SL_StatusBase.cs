using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader.Model.Status
{
    [SL_Serialized]
    public abstract class SL_StatusBase
    {
        internal string m_serializedFilename;

        public virtual void ExportIcons(Component comp, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }
    }
}
