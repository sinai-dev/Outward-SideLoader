using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace SideLoader.SLPacks
{
    public class SLPackArchive : SLPack
    {
        /// <summary>
        /// Create and prepare an SLPackArchive from a zipped SLPack stream.
        /// </summary>
        /// <param name="resourceStream">The stream for the zipped SLPack, eg. from calling typeof(MyPlugin).Assembly.GetManifestResourceStream("...")</param>
        /// <param name="packName">The unique name you are giving to this SLPack.</param>
        public static SLPackArchive CreatePackFromStream(Stream resourceStream, string packName)
        {
            var zip = new ZipArchive(resourceStream);
            var archive = new SLPackArchive(packName, zip);

            return archive;
        }

        public SLPackArchive(string name, ZipArchive archive)
        {
            this.Name = name;
            this.RefZipArchive = archive;

            if (SL.s_embeddedArchivePacks.ContainsKey(Name))
            {
                SL.LogWarning("Two embedded SLPackArchives with duplicate name: " + Name + ", not loading!");
                return;
            }

            SL.s_embeddedArchivePacks.Add(Name, this);

            CachePaths();
        }

        public readonly ZipArchive RefZipArchive;

        internal Dictionary<string, List<string>> m_fileStructure = new Dictionary<string, List<string>>();

        internal Dictionary<string, ZipArchiveEntry> m_cachedEntries = new Dictionary<string, ZipArchiveEntry>();

        public override string FolderPath => "";

        private void CachePaths()
        {
            m_fileStructure = new Dictionary<string, List<string>>();
            m_cachedEntries = new Dictionary<string, ZipArchiveEntry>();

            foreach (var entry in RefZipArchive.Entries)
            {
                var fullpath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);

                if (fullpath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    fullpath = fullpath.Substring(0, fullpath.Length - 1);

                    //SL.Log("added directory to file structure: " + fullpath);

                    m_fileStructure.Add(fullpath, new List<string>());
                    continue;
                }

                //SL.Log("Caching entry " + fullpath);

                m_cachedEntries.Add(fullpath, entry);

                var splitPath = fullpath.Split(Path.DirectorySeparatorChar);

                string key = "";
                if (splitPath.Length > 1)
                {
                    for (int i = 0; i < splitPath.Length - 1; i++)
                    {
                        if (!string.IsNullOrEmpty(key))
                            key += Path.DirectorySeparatorChar;

                        key += splitPath[i];
                    }
                }

                if (!m_fileStructure.ContainsKey(key))
                    m_fileStructure.Add(key, new List<string>());

                m_fileStructure[key].Add(Path.GetFileName(fullpath));

                //SL.Log($"added to file structure under directory '{key}', file '{Path.GetFileName(fullpath)}'");
            }
        }

        public override bool DirectoryExists(string relativeDirectory)
            => m_fileStructure.ContainsKey(relativeDirectory);

        public override bool FileExists(string relativeDirectory, string file)
        {
            if (m_fileStructure.TryGetValue(relativeDirectory, out List<string> files))
                return files.Any(it => string.Equals(it, file, StringComparison.InvariantCultureIgnoreCase));

            return false;
        }

        public override string[] GetDirectories(string relativeDirectory)
        {
            // get all sub-directories inside the parent (all depth!)
            var query = m_fileStructure.Keys.Where(it => it.StartsWith(relativeDirectory));

            if (!query.Any())
                return new string[0];

            // now filter those results and only use real results.
            var list = new List<string>();

            string parentName = Path.GetFileName(relativeDirectory);

            foreach (var entry in query)
            {
                var split = entry.Split(Path.DirectorySeparatorChar);

                // Length must be at least 2 (0 = parent, 1 = subdir).
                if (split.Length < 2)
                    continue;

                var thisParent = split[split.Length - 2];

                // Make sure parent directory is the one we are searching inside.
                if (string.Equals(thisParent, parentName, StringComparison.InvariantCultureIgnoreCase))
                    list.Add(entry);
            }

            return list.ToArray();
        }

        public override string[] GetFiles(string relativeDirectory)
        {
            if (m_fileStructure.TryGetValue(relativeDirectory, out List<string> files))
                return files.ToArray();

            return new string[0];
        }

        protected internal override AssetBundle LoadAssetBundle(string relativeDirectory, string file)
        {
            if (!FileExists(relativeDirectory, file))
                return null;

            var data = ReadAllBytes(relativeDirectory, file);

            return AssetBundle.LoadFromMemory(data);
        }

        protected internal override AudioClip LoadAudioClip(string relativeDirectory, string file)
        {
            if (!FileExists(relativeDirectory, file))
                return null;

            var data = ReadAllBytes(relativeDirectory, file);

            return CustomAudio.LoadAudioClip(data, Path.GetFileNameWithoutExtension(file), this);
        }

        protected internal override Texture2D LoadTexture2D(string relativeDirectory, string file, bool mipmap = false, bool linear = false)
        {
            if (!FileExists(relativeDirectory, file))
                return null;

            var data = ReadAllBytes(relativeDirectory, file);

            return CustomTextures.LoadTexture(data, mipmap, linear);
        }

        private byte[] ReadAllBytes(string relativeDirectory, string file)
        {
            byte[] ret = null;

            var path = Path.Combine(relativeDirectory, file);

            if (m_cachedEntries.TryGetValue(path, out ZipArchiveEntry entry))
            {
                using (var stream = entry.Open())
                {
                    ret = ReadFully(stream);
                }
            }

            return ret;
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        protected internal override T ReadXmlDocument<T>(string relativeDirectory, string file)
        {
            var path = Path.Combine(relativeDirectory, file);

            if (m_cachedEntries.TryGetValue(path, out ZipArchiveEntry entry))
            {
                //SL.Log("Deserializing XML document from archive: " + file);

                object ret;
                Type baseType;

                // get the base type
                using (var stream = entry.Open())
                {
                    baseType = Serializer.GetBaseTypeOfXmlDocument(stream);
                }

                // have to re-open the handle because compressed objects do not support Seek()
                using (var stream = entry.Open())
                {
                    ret = Serializer.LoadFromXml(stream, baseType);
                    //SL.Log("Deserialized object into " + ret.GetType());
                }

                return (T)ret;
            }

            return default;
        }
    }
}
