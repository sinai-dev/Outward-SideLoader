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

        public Dictionary<string, Texture2D> TextureData = new Dictionary<string, Texture2D>();

        internal void Awake()
        {
            Instance = this;
        }

        // This should result in the best possible sprite quality, as far as I understand.
        // These are the values thats Nine Dots use, at least according to the uTiny Ripper output.
        public static Sprite CreateSprite(Texture2D texture)
        {
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 24.92582f, 1, SpriteMeshType.Tight);
        }

        public IEnumerator ReplaceActiveTextures()
        {
            SideLoader.Log("Replacing Materials..");
            float start = Time.time;

            // ============ materials ============
            var list = Resources.FindObjectsOfTypeAll<Material>()
                        .Where(x => x.mainTexture != null && TextureData.ContainsKey(x.mainTexture.name))
                        .ToList();

            SideLoader.Log(string.Format("Found {0} materials to replace.", list.Count));

            int i = 0;
            foreach (Material m in list)
            {
                string name = m.mainTexture.name;
                i++; SideLoader.Log(string.Format(" - Replacing material {0} of {1}: {2}", i, list.Count, name));

                // set maintexture (diffuse map)
                m.mainTexture = TextureData[name];

                // ======= set other shader material layers =======     
                if (name.EndsWith("_d")) { name = name.Substring(0, name.Length - 2); } // try remove the _d suffix, if its there

                // check each shader material suffix name
                foreach (KeyValuePair<string, string> entry in TextureSuffixes)
                {
                    if (entry.Key == "_d") { continue; } // already set MainTex

                    if (TextureData.ContainsKey(name + entry.Key))
                    {
                        SideLoader.Log(" - Setting " + entry.Value + " for " + m.name);
                        m.SetTexture(entry.Value, TextureData[name + entry.Key]);
                    }
                }

                yield return null;
            }

            // ========= sprites =========

            SideLoader.Log("Replacing PrefabManager icons...");
            if (At.GetValue(typeof(ResourcesPrefabManager), null, "ITEM_PREFABS") is Dictionary<string, Item> dict)
            {
                foreach (Item item in dict.Values
                    .Where(x => 
                    x.ItemID >= 2000000 
                    && x.ItemIcon != null 
                    && x.ItemIcon.texture != null 
                    && TextureData.ContainsKey(x.ItemIcon.texture.name)))
                {
                    string name = item.ItemIcon.texture.name;
                    SideLoader.Log(string.Format(" - Replacing item icon: {0}", name));

                    var tex = TextureData[name];
                    var sprite = CreateSprite(tex);
                    At.SetValue(sprite, typeof(Item), item, "m_itemIcon");
                }
            }

            // ==============================================

            SideLoader.Log("Active assets replaced. Time: " + (Time.time - start), 0);
            SL.Instance.Loading = false;
        }

        public static readonly Dictionary<string, string> TextureSuffixes = new Dictionary<string, string>()
        {
            { "_d", "_MainTex" },
            { "_n", "_NormTex" },
            { "_g", "_GenTex" },
            { "_sc", "_SpecColorTex" },
            { "_i", "_EmissionTex" },
        };

        public IEnumerator LoadTextures()
        {
            SideLoader.Log("Reading Texture2D data...");
            float start = Time.time;

            foreach (string filepath in SL.Instance.FilePaths[ResourceTypes.Texture])
            {
                Texture2D texture2D = LoadPNG(filepath);

                string texname = Path.GetFileNameWithoutExtension(filepath);
                if (TextureData.ContainsKey(texname))
                {
                    SideLoader.Log("Adding " + texname + " to texture data dictionary, but a key with this name already exists! Overwriting...");
                    TextureData[texname] = texture2D;
                }
                else
                {
                    TextureData.Add(texname, texture2D);
                }

                SideLoader.Log(" - Texture loaded: " + texname + ", from " + filepath);

                yield return null;
            }

            SL.Instance.Loading = false;
            SideLoader.Log("Textures loaded. Time: " + (Time.time - start), 0);
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
    }
}
