using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Threading;
using UnityEditor;

namespace SideLoader_2
{
    public class CustomTextures : MonoBehaviour
    {
        public static CustomTextures Instance;

        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();        

        internal void Awake()
        {
            Instance = this;

            QualitySettings.masterTextureLimit = 0;

            SL.OnPacksLoaded += ReplaceActiveTextures;
            SL.OnSceneLoaded += ReplaceActiveTextures;
        }

        public static void ReplaceActiveTextures()
        {
            float start = Time.realtimeSinceStartup;
            SL.Log("Replacing active textures.");

            if (Textures.Count < 1)
            {
                return;
            }

            // ============ Materials ============

            var list = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.mainTexture != null);

            var layers = new string[]
            {
                Layer._MainTex.ToString(),
                Layer._NormTex.ToString(),
                Layer._GenTex.ToString(),
                Layer._SpecColorTex.ToString(),
                Layer._EmissionTex.ToString(),
            };

            foreach (Material m in list)
            {
                foreach (var layer in layers)
                {
                    if (m.GetTexture(layer) is Texture tex && Textures.ContainsKey(tex.name))
                    {
                        SL.Log("Replacing layer " + layer + " on material " + m.name);
                        m.SetTexture(layer, Textures[tex.name]);
                    }
                }
            }

            // ============ UI.Image ============ //

            // note: this works, but im not sure if its desirable right now. maybe we have another folder for Sprite. 
            // you might accidentally replace menu with generic names.
            // also, this doesnt work for item icons.

            //var images = Resources.FindObjectsOfTypeAll<Image>().Where(x => x.sprite != null && x.sprite.texture != null);

            //foreach (Image i in images)
            //{
            //    if (Textures.ContainsKey(i.sprite.texture.name))
            //    {
            //        SL.Log("Replacing sprite for " + i.name);
            //        i.sprite = CreateSprite(Textures[i.sprite.texture.name]);
            //    }
            //}

            var time = Math.Round(1000f * (Time.realtimeSinceStartup - start), 2);

            SL.Log("Finished replacing textures, took " + time + " ms");
        }

        /// <summary> Simple helper for loading a Texture2D from a .png filepath </summary>
        public static Texture2D LoadTexture(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(1, 1);
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

        public static void SaveTextureAsPNG(Texture2D _tex, string dir, string name)
        {
            var savepath = dir + @"\" + name + ".png";
            try
            {
                byte[] data = _tex.EncodeToPNG();
                File.WriteAllBytes(savepath, data);
            }
            catch
            {
                var origFilter = _tex.filterMode;

                _tex.filterMode = FilterMode.Point;
                RenderTexture rt = RenderTexture.GetTemporary(_tex.width, _tex.height);
                rt.filterMode = FilterMode.Point;
                RenderTexture.active = rt;
                Graphics.Blit(_tex, rt);
                Texture2D _newTex = new Texture2D(_tex.width, _tex.height);
                _newTex.ReadPixels(new Rect(0, 0, _tex.width, _tex.height), 0, 0);
                _newTex.Apply();
                RenderTexture.active = null;

                _tex.filterMode = origFilter;

                byte[] data = _newTex.EncodeToPNG();
                File.WriteAllBytes(savepath, data);
            }
        }

        public enum Layer
        {
            _MainTex,
            _NormTex,
            _GenTex,
            _SpecColorTex,
            _EmissionTex
        }

        public enum Suffix
        {
            _d,   // _MainTex
            _n,   // _NormTex
            _g,   // _GenTex
            _s,   // _SpecColorTex
            _i    // _EmissionTex
        }

        public static string GetSuffix(Layer layer)
        {
            return ((Suffix)(int)layer).ToString();
        }

        /// <summary> Helper for the Texture suffixes. Keys are the suffixes (eg. Suffixes._d), Values are the Layer name (eg. Layers._MainTex)
        /// Eg. SuffixToShaderLayer[Suffixes._d] would return Layers._MainTex</summary>
        public static readonly Dictionary<Suffix, Layer> SuffixToShaderLayer = new Dictionary<Suffix, Layer>()
        {
            { Suffix._d, Layer._MainTex },
            { Suffix._n, Layer._NormTex },
            { Suffix._g, Layer._GenTex },
            { Suffix._s, Layer._SpecColorTex },
            { Suffix._i, Layer._EmissionTex },
        };
    }
}