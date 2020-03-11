using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader_2
{
    public class SLPack
    {
        public string Name { get; private set; }
        public bool Registered { get; private set; } = false;

        public Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();
        public Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        public Dictionary<string, ItemHolder> CustomItems = new Dictionary<string, ItemHolder>();
        public Dictionary<string, RecipeHolder> CustomRecipes = new Dictionary<string, RecipeHolder>();
        public Dictionary<string, Texture2D> Texture2D = new Dictionary<string, Texture2D>();

        public delegate void PackRegistered();
        public event PackRegistered OnPackRegistered;

        public void LoadFromFolder(string dir)
        {
            Name = Path.GetFileName(dir);


        }

        public void ApplyPack()
        {


            Registered = true;
            OnPackRegistered?.Invoke();
        }

        ///<summary>The expected subfolder names inside a SLPack</summary>
        public static class Folders
        {
            public static string Audio = "Audio";
            public static string AssetBundles = "AssetBundles";
            public static string CustomItems = "CustomItems";
            public static string CustomRecipes = "CustomRecipes";
            public static string Texture2D = "Texture2D";
        }
    }
}
