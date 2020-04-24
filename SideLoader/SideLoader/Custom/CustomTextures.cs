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

namespace SideLoader
{
    public class CustomTextures : MonoBehaviour
    {
        public static CustomTextures Instance;

        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        // helper enum for certain types of icon borders that Nine Dots use
        public enum SpriteBorderTypes
        {
            NONE,
            ItemIcon,
            SkillTreeIcon,
        }

        /// <summary>
        /// Nine Dots' Shader layer names.
        /// </summary>
        public enum Layer
        {
            _MainTex,
            _NormTex,
            _GenTex,
            _SpecColorTex,
            _EmissionTex
        }

        /// <summary>
        /// Suffixes used on most filenames for textures that use Nine Dots Shader. Use CustomTextures.SuffixToShaderDict to get the shader layer name for the suffix.
        /// </summary>
        public enum Suffix
        {
            _d,   // _MainTex
            _n,   // _NormTex
            _g,   // _GenTex
            _s,   // _SpecColorTex
            _i    // _EmissionTex
        }

        internal void Awake()
        {
            Instance = this;

            QualitySettings.masterTextureLimit = 0;

            SL.OnPacksLoaded += ReplaceActiveTextures;
            SL.OnSceneLoaded += ReplaceActiveTextures;
        }

        public static Texture2D LoadTexture(string filePath, bool isNormal = false)
        {
            return LoadTextureInternal(filePath, isNormal);
        }

        /// <summary> Simple helper for loading a Texture2D from a .png filepath </summary>
        private static Texture2D LoadTextureInternal(string filePath, bool isNormal)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            SL.Log("Loading texture map " + name + ", isnormal: " + isNormal);

            if (File.Exists(filePath))
            {
                Texture2D tex;
                var fileData = File.ReadAllBytes(filePath);

                if (isNormal)
                {
                    tex = new Texture2D(1, 1, TextureFormat.DXT1, false, true);
                }
                else
                {
                    tex = new Texture2D(1, 1, TextureFormat.DXT1, false);
                }

                tex.LoadImage(fileData);
                tex.filterMode = FilterMode.Point;
                return tex;
            }
            else
            {
                SL.Log("Could not find " + filePath, 1);
                return null;
            }
        }

        /// <summary> Helper for creating a generic sprite with no border, from a Texture2D. Use CustomTextures.LoadTexture() to load a tex from a filepath. </summary>
        public static Sprite CreateSprite(Texture2D texture)
        {
            return CreateSprite(texture, SpriteBorderTypes.NONE);
        }

        /// <summary> Create a sprite with the appropriate border for the type. Use CustomTextures.LoadTexture() to load a tex from a filepath.</summary>
        public static Sprite CreateSprite(Texture2D tex, SpriteBorderTypes borderType)
        {
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;

            Vector4 offset = Vector4.zero;
            switch (borderType)
            {
                case SpriteBorderTypes.ItemIcon:
                    offset = new Vector4(1, 2, 2, 3); break;
                case SpriteBorderTypes.SkillTreeIcon:
                    offset = new Vector4(1, 1, 1, 2); break;
                default: break;
            }

            var rect = new Rect(
                offset.x,
                offset.z,
                tex.width - offset.y,
                tex.height - offset.w);

            return Sprite.Create(tex, rect, Vector2.zero, 100f, 1, SpriteMeshType.Tight);
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

        // ============= Internal Functions ===============

        public static void ReplaceActiveTextures()
        {
            if (Textures.Count < 1)
            {
                return;
            }

            float start = Time.realtimeSinceStartup;
            SL.Log("Replacing active textures.");

            // ============ Materials ============

            var list = Resources.FindObjectsOfTypeAll<Material>();

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
                    if (m.HasProperty(layer) && m.GetTexture(layer) is Texture tex && Textures.ContainsKey(tex.name))
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
    }
}