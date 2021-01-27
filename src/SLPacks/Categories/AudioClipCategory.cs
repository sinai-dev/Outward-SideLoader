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

            if (!Directory.Exists(dirPath))
                return dict;

            foreach (var clipPath in Directory.GetFiles(dirPath, "*.wav"))
            {
                SLPlugin.Instance.StartCoroutine(CustomAudio.LoadClip(clipPath, pack, dict));
            }

            return dict;
        }
    }
}
