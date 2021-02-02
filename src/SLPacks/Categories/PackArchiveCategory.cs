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
    public class PackArchiveCategory : SLPackAssetCategory
    {
        public override string FolderName => "PackArchives";

        public override Type BaseContainedType => typeof(SLPackArchive);

        internal static Dictionary<string, SLPackArchive> s_loadedArchives = new Dictionary<string, SLPackArchive>();

        protected internal override void InternalLoad(List<SLPack> packs, bool isHotReload)
        {
            base.InternalLoad(packs, isHotReload);
        }

        public override Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload)
        {
            var dir = pack.GetPathForCategory<PackArchiveCategory>();
            if (!Directory.Exists(dir))
                return null;

            foreach (var archivePath in Directory.GetFiles(dir))
            {
                try
                {
                    SL.Log("Loading Pack Archive zip: " + archivePath);

                    var zip = ZipFile.OpenRead(archivePath);

                    var archive = new SLPackArchive(Path.GetFileNameWithoutExtension(archivePath), pack, zip);

                    pack.PackArchives.Add(archive.Name, archive);

                    s_loadedArchives.Add(archivePath, archive);

                    //SL.Log($"Added archive '{archiveName}' to pack '{pack.Name}' (Archive count: {pack.PackArchives.Count})");
                }
                catch (Exception ex)
                {
                    SL.LogWarning("Exception loading Pack Bundle: " + archivePath);
                    SL.LogInnerException(ex);
                }
            }

            return null;
        }

        protected internal override void OnHotReload()
        {
            for (int i = s_loadedArchives.Count - 1; i >= 0; i--)
            {
                var entry = s_loadedArchives.ElementAt(i);
                entry.Value?.RefZipArchive?.Dispose();
            }

            s_loadedArchives.Clear();
        }
    }
}
