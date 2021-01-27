using System.Collections.Generic;

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
