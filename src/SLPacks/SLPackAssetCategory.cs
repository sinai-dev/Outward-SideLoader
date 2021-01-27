using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.SLPacks
{
    public abstract class SLPackAssetCategory : SLPackCategory
    {
        public override int LoadOrder => 0;

        internal override Dictionary<string, object> InternalLoad(SLPack pack, bool isHotReload)
        {
            return LoadContent(pack, isHotReload);
        }

        public abstract Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload);
    }
}
