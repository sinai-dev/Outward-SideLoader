using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SideLoader.SLPacks.Categories
{
    public class AudioClipCategory : SLPackAssetCategory
    {
        public override string FolderName => "AudioClip";

        public override Type BaseContainedType => typeof(AudioClip);

        public override Dictionary<string, object> LoadContent(SLPack pack, bool isHotReload)
        {
            var dict = new Dictionary<string, object>();

            var dirPath = pack.GetPathForCategory<AudioClipCategory>();

            if (!pack.DirectoryExists(dirPath))
                return dict;

            foreach (var clipPath in pack.GetFiles(dirPath, ".wav"))
            {
                SL.Log("Loading audio clip from '" + clipPath + "'");

                var clip = pack.LoadAudioClip(dirPath, Path.GetFileName(clipPath));

                if (clip != null)
                    dict.Add(clipPath, clip);
            }

            return dict;
        }

        protected internal override void OnHotReload()
        {
        }
    }
}
