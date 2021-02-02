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

                if (pack.PackArchives != null && pack.PackArchives.Count > 0)
                {
                    foreach (var archive in pack.PackArchives.Values)
                        LoadContent(archive, isHotReload);
                }

                if (pack.PackBundles != null && pack.PackBundles.Count > 0)
                {
                    SL.Log("Checking pack bundles for category '" + this.GetType().Name + "' in pack " + pack.Name);
                    foreach (var archive in pack.PackBundles.Values)
                        LoadContent(archive, isHotReload);
                }
            }
        }

        public abstract Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload);
    }
}
