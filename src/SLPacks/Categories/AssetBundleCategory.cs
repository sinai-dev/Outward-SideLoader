using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SideLoader.SLPacks.Categories
{
    public class AssetBundleCategory : SLPackAssetCategory
    {
        public override string FolderName => "AssetBundles";

        public override Type BaseContainedType => typeof(AssetBundle);

        public override Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload)
        {
            var dict = new Dictionary<string, object>();

            // AssetBundle does not use hot reload at the moment.
            if (isHotReload)
                return dict;

            var dirPath = pack.GetPathForCategory<AssetBundleCategory>();

            if (!Directory.Exists(dirPath))
                return dict;

            var fileQuery = Directory.GetFiles(dirPath)
                                     .Where(x => !x.EndsWith(".meta")
                                                 && !x.EndsWith(".manifest"));

            foreach (var bundlePath in fileQuery)
            {
                try
                {
                    var bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (bundle is AssetBundle)
                    {
                        string name = Path.GetFileName(bundlePath);

                        pack.AssetBundles.Add(name, bundle);

                        dict.Add(bundlePath, bundle);
                        SL.Log("Loaded assetbundle " + name);
                    }
                    else
                        throw new Exception($"Unknown error (Bundle '{Path.GetFileName(bundlePath)}' was null)");
                }
                catch (Exception e)
                {
                    SL.LogError("Error loading asset bundle! Message: " + e.Message + "\r\nStack: " + e.StackTrace);
                }
            }

            return dict;
        }

        protected internal override void OnHotReload()
        {
        }
    }
}
