using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks
{
    public abstract class SLPackCategory
    {
        public abstract string FolderName { get; }

        public abstract Type BaseContainedType { get; }

        public abstract int LoadOrder { get; }

        public virtual bool HasLateContent => false;

        public virtual void ApplyLateContent(SLPack pack, bool isHotReload)
            => throw new NotImplementedException("This SLPackCategory does not have late content.");

        internal abstract Dictionary<string, object> InternalLoad(SLPack pack, bool isHotReload);
    }
}
