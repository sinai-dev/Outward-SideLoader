using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SideLoader.SLPacks
{
    public class SLPackBundle : SLPack
    {
        public SLPackBundle(string name, SLPack parentPack, AssetBundle bundle)
        {
            this.Name = $"{parentPack.Name}.{name}";

            if (SL.s_bundlePacks.ContainsKey(Name))
            {
                SL.LogWarning("Two SLPackBundles with duplicate name: " + Name + ", not loading!");
                return;
            }

            SL.s_bundlePacks.Add(Name, this);

            this.ParentSLPack = parentPack;
            this.RefAssetBundle = bundle;

            CachePaths();
        }

        public readonly SLPack ParentSLPack;
        public readonly AssetBundle RefAssetBundle;

        internal Dictionary<string, List<string>> m_fileStructure = new Dictionary<string, List<string>>();

        internal Dictionary<string, Object> m_cachedEntries = new Dictionary<string, Object>();

        public override string FolderPath => "";

        private void CachePaths()
        {
            m_fileStructure = new Dictionary<string, List<string>>();
            m_cachedEntries = new Dictionary<string, Object>();

            foreach (var entry in RefAssetBundle.GetAllAssetNames())
            {
                var fullpath = entry.Replace('/', Path.DirectorySeparatorChar);

                var assetsString = $"assets{Path.DirectorySeparatorChar}";
                if (fullpath.StartsWith(assetsString))
                    fullpath = fullpath.Substring(assetsString.Length, fullpath.Length - assetsString.Length);

                //SL.Log("Caching entry " + fullpath);

                var obj = RefAssetBundle.LoadAsset(entry);

                m_cachedEntries.Add(fullpath, obj);

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

        public override bool DirectoryExists(string dir)
            => m_fileStructure.Keys.Any(it => it.Equals(dir, StringComparison.InvariantCultureIgnoreCase)
                                           || it.StartsWith($"{dir}{Path.DirectorySeparatorChar}", StringComparison.InvariantCultureIgnoreCase));

        public override string[] GetDirectories(string relativeDirectory)
        {
            relativeDirectory = relativeDirectory.ToLower();

            // get all sub-directories inside the parent (all depth!)
            var query = m_fileStructure.Keys.Where(it => it.StartsWith(relativeDirectory));

            if (!query.Any())
                return new string[0];

            // now filter those results and only use top-level results.
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

        public override bool FileExists(string relativeDirectory, string file)
        {
            if (m_fileStructure.TryGetValue(relativeDirectory.ToLower(), out List<string> files))
                return files.Any(it => string.Equals(it, file, StringComparison.InvariantCultureIgnoreCase));

            return false;
        }

        public override string[] GetFiles(string relativeDirectory)
        {
            if (m_fileStructure.TryGetValue(relativeDirectory.ToLower(), out List<string> files))
                return files.ToArray();

            return new string[0];
        }

        protected internal override AssetBundle LoadAssetBundle(string relativeDirectory, string file)
            => throw new NotImplementedException("An AssetBundle cannot load AssetBundles!");

        protected internal override AudioClip LoadAudioClip(string relativeDirectory, string file)
        {
            if (!FileExists(relativeDirectory, file))
                return null;

            var clip = GetAsset<AudioClip>(relativeDirectory, file);

            if (!clip)
                return null;

            return CustomAudio.LoadAudioClip(clip, Path.GetFileNameWithoutExtension(file), this);
        }

        protected internal override Texture2D LoadTexture2D(string relativeDirectory, string file, bool mipmap = false, bool linear = false)
        {
            if (!FileExists(relativeDirectory, file))
                return null;

            var orig = GetAsset<Texture2D>(relativeDirectory, file);

            return orig;

            //if (!orig)
            //    return null;

            //var data = orig.GetRawTextureData();

            //return CustomTextures.LoadTexture(data, mipmap, linear);
        }

        private T GetAsset<T>(string relativePath, string file) where T : Object
        {
            var path = Path.Combine(relativePath, file).ToLower();

            m_cachedEntries.TryGetValue(path, out Object asset);

            return (T)asset;
        }

        protected internal override T ReadXmlDocument<T>(string relativeDirectory, string file)
        {
            var path = Path.Combine(relativeDirectory, file).ToLower();

            if (m_cachedEntries.TryGetValue(path, out Object entry))
            {
                //SL.Log("Deserializing XML document from archive: " + file);

                object ret;
                Type baseType;

                var textAsset = entry as TextAsset;

                if (!textAsset)
                {
                    SL.LogWarning("Could not get '" + path + "' as TextAsset!");
                    return default;
                }

                string data = textAsset.text;

                if (string.IsNullOrEmpty(data))
                    return default;

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    baseType = Serializer.GetBaseTypeOfXmlDocument(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    ret = Serializer.LoadFromXml(stream, baseType);

                    //SL.Log("Deserialized object into " + ret.GetType());
                }

                return (T)ret;
            }

            return default;
        }
    }
}
