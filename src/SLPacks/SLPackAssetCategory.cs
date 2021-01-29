using System.Collections.Generic;

namespace SideLoader.SLPacks
{
    public abstract class SLPackAssetCategory : SLPackCategory
    {
        public override int LoadOrder => 0;

        protected internal override void InternalLoad(List<SLPack> packs, bool isHotReload)
        {
            foreach (var pack in packs)
            {
                var dict = LoadContent(pack, isHotReload);
                if (dict != null)
                    this.AddToSLPackDictionary(pack, dict);
            }
        }

        public abstract Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload);
    }
}
