using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader_2
{
    public class SL_Textures : MonoBehaviour
    {
        public static SL_Textures Instance;

        public static Dictionary<string, Texture2D> TextureReplacements = new Dictionary<string, Texture2D>();

        /// <summary> Helper for the Texture suffixes. Keys are the suffixes (eg. _d), Values are the Layer name (eg. _MainTex) </summary>
        public static readonly Dictionary<string, string> SuffixToShaderLayer = new Dictionary<string, string>()
        {
            { "_d", "_MainTex" },
            { "_n", "_NormTex" },
            { "_g", "_GenTex" },
            { "_sc", "_SpecColorTex" },
            { "_i", "_EmissionTex" },
        };

        internal void Awake()
        {
            Instance = this;

            SideLoader.OnPacksLoaded += ReplaceActiveTextures;
            SideLoader.OnSceneLoaded += ReplaceActiveTextures;
        }

        /// <summary> Simple helper for loading a Texture2D from a .png filepath </summary>
        public static Texture2D LoadTexture(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(1,1);
                tex.LoadImage(fileData);
            }
            return tex;
        }

        /// <summary> Helper for creating a Sprite from a Texture2D, with the values that Nine Dots use for UI Sprites.</summary>
        public static Sprite CreateSprite(Texture2D texture)
        {
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 24.92582f, 1, SpriteMeshType.Tight);
        }

        public static void ReplaceActiveTextures()
        {
            // ============ materials ============

            var list = Resources.FindObjectsOfTypeAll<Material>()
                .Where(x => x.mainTexture != null && TextureReplacements.ContainsKey(x.mainTexture.name))
                .ToList();

            if (list.Count < 1)
            {
                return;
            }

            SideLoader.Log("Replacing active textures..");
            float start = Time.time;

            int i = 0;
            foreach (Material m in list)
            {
                string name = m.mainTexture.name;
                i++;
                SideLoader.Log(string.Format(" - Replacing material {0} of {1}: {2}", i, list.Count, name));

                // set maintexture (diffuse map)
                m.mainTexture = TextureReplacements[name];

                // ======= set other shader material layers =======     
                if (name.EndsWith("_d")) { name = name.Substring(0, name.Length - 2); } // try remove the _d suffix, if its there

                // check each shader material suffix name
                foreach (KeyValuePair<string, string> entry in SuffixToShaderLayer)
                {
                    if (entry.Key == "_d") { continue; } // already set MainTex

                    if (TextureReplacements.ContainsKey(name + entry.Key))
                    {
                        SideLoader.Log(" - Setting " + entry.Value + " for " + m.name);
                        m.SetTexture(entry.Value, TextureReplacements[name + entry.Key]);
                    }
                }
            }

            // todo UI.Image replacement?

            //// ============ item icon sprites ============

            //if (At.GetValue(typeof(ResourcesPrefabManager), null, "ITEM_PREFABS") is Dictionary<string, Item> dict)
            //{
            //    foreach (Item item in dict.Values
            //        .Where(x =>
            //        x.ItemID > 2000000
            //        && x.ItemIcon != null
            //        && x.ItemIcon.texture != null
            //        && TextureReplacements.ContainsKey(x.ItemIcon.texture.name)))
            //    {
            //        string name = item.ItemIcon.texture.name;
            //        SideLoader.Log(string.Format(" - Replacing item icon: {0}", name));

            //        var tex = TextureReplacements[name];
            //        var sprite = CreateSprite(tex);
            //        At.SetValue(sprite, typeof(Item), item, "m_itemIcon");
            //    }
            //}
        }
    }
}
