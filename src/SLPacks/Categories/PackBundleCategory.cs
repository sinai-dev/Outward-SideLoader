using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader.SLPacks.Categories
{
    public class PackBundleCategory : SLPackAssetCategory
    {
        public override string FolderName => "PackBundles";

        public override Type BaseContainedType => typeof(SLPackBundle);

        internal static Dictionary<string, SLPackBundle> s_loadedBundles = new Dictionary<string, SLPackBundle>();

        protected internal override void InternalLoad(List<SLPack> packs, bool isHotReload)
        {
            base.InternalLoad(packs, isHotReload);
        }

        public override Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload)
        {
            var dir = pack.GetPathForCategory<PackBundleCategory>();
            if (!Directory.Exists(dir))
                return null;

            foreach (var bundlePath in Directory.GetFiles(dir))
            {
                try
                {
                    SL.Log("Loading Pack AssetBundle: " + bundlePath);

                    var assetbundle = AssetBundle.LoadFromFile(bundlePath);

                    var bundlePack = new SLPackBundle(Path.GetFileName(bundlePath), pack, assetbundle);

                    pack.PackBundles.Add(bundlePack.Name, bundlePack);

                    s_loadedBundles.Add(bundlePath, bundlePack);

                    //SL.Log($"Added bundle '{bundlePack.Name}' to pack '{pack.Name}' (Bundle count: {pack.PackBundles.Count})");
                }
                catch (Exception ex)
                {
                    SL.LogWarning("Exception loading Pack Bundle: " + bundlePath);
                    SL.LogInnerException(ex);
                }
            }

            return null;
        }

        protected internal override void OnHotReload()
        {
            for (int i = s_loadedBundles.Count - 1; i >= 0; i--)
            {
                var entry = s_loadedBundles.ElementAt(i);
                if (entry.Value?.RefAssetBundle)
                {
                    entry.Value.RefAssetBundle.Unload(true);
                }
            }

            s_loadedBundles.Clear();
        }
    }
}
