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
using UnityEngine.Experimental.Rendering;

namespace SideLoader
{
    public class CustomTextures
    {
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        // helper enum for certain types of icon borders that Nine Dots use
        public enum SpriteBorderTypes
        {
            NONE,
            ItemIcon,
            SkillTreeIcon,
        }

        /// <summary>
        /// Handles how different types of Textures are loaded with Texture2D.LoadImage.
        /// If it's not a Normal (bump map) or GenTex, just use Default.
        /// </summary>
        public enum TextureType
        {
            Default,
            Normal,
            GenTex
        }

        public static void Init()
        {
            QualitySettings.masterTextureLimit = 0;

            SL.OnPacksLoaded += ReplaceActiveTextures;
            SL.OnSceneLoaded += ReplaceActiveTextures;
        }

        /// <summary>
        /// Simple helper for loading a Texture2D from a .png filepath
        /// </summary>
        /// <param name="filePath">The full or relative filepath</param>
        /// <param name="mipmap">Do you want mipmaps for this texture?</param>
        /// <param name="linear">Is this linear or sRGB? (Normal or non-normal)</param>
        /// <returns>The Texture2D (or null if there was an error)</returns>
        public static Texture2D LoadTexture(string filePath, bool mipmap, bool linear)
        {
            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, mipmap, linear);

                try
                {
                    tex.LoadImage(fileData);
                }
                catch (Exception e)
                {
                    SL.Log("Error loading texture! Message: " + e.Message + "\r\nStack: " + e.StackTrace);
                }

                tex.filterMode = FilterMode.Bilinear;

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

        public static void SaveIconAsPNG(Sprite icon, string dir, string name = "icon")
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            SaveTextureAsPNG(icon.texture, dir, name, false);
        }

        public static void SaveTextureAsPNG(Texture2D _tex, string dir, string name, bool normal)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            byte[] data;
            var savepath = dir + @"\" + name + ".png";

            try
            {
                if (normal)
                {
                    _tex = DTXnmToRGBA(_tex);
                    _tex.Apply(false, false);
                }

                data = _tex.EncodeToPNG();

                if (data == null)
                {
                    throw new Exception();
                }
            }
            catch
            {
                var origFilter = _tex.filterMode;
                _tex.filterMode = FilterMode.Point;

                RenderTexture rt = RenderTexture.GetTemporary(_tex.width, _tex.height);
                rt.filterMode = FilterMode.Point;
                RenderTexture.active = rt;
                Graphics.Blit(_tex, rt);

                Texture2D _newTex = new Texture2D(_tex.width, _tex.height, TextureFormat.RGBA32, false);
                _newTex.ReadPixels(new Rect(0, 0, _tex.width, _tex.height), 0, 0);

                if (normal)
                {
                    _newTex = DTXnmToRGBA(_newTex);
                }

                _newTex.Apply(false, false);

                RenderTexture.active = null;
                _tex.filterMode = origFilter;

                data = _newTex.EncodeToPNG();
            }

            File.WriteAllBytes(savepath, data);
        }

        // Converts DTXnm-format Normal Map to RGBA-format Normal Map.
        private static Texture2D DTXnmToRGBA(Texture2D tex)
        {
            Color[] colors = tex.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            { 
                Color c = colors[i];

                c.r = c.a * 2 - 1;  // red <- alpha (x <- w)
                c.g = c.g * 2 - 1;  // green is always the same (y)

                Vector2 rg = new Vector2(c.r, c.g); //this is the xy vector
                c.b = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(rg, rg))); //recalculate the blue channel (z)

                colors[i] = new Color(
                    (c.r * 0.5f) + 0.5f,
                    (c.g * 0.5f) + 0.25f, 
                    (c.b * 0.5f) + 0.5f
                );
            }

            var newtex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            newtex.SetPixels(colors); //apply pixels to the texture

            return newtex;
        }


        // =========== Shader Properties Helpers ===========

        public enum ShaderPropType
        {
            Color,
            Vector,
            Float
        }

        public static List<SL_Material.ShaderProperty> GetProperties(Material m)
        {
            var list = new List<SL_Material.ShaderProperty>();

            if (ShaderPropertyDicts.ContainsKey(m.shader.name))
            {
                var dict = ShaderPropertyDicts[m.shader.name];

                foreach (var entry in dict)
                {
                    switch (entry.Value)
                    {
                        case ShaderPropType.Color:
                            list.Add(new SL_Material.ColorProp()
                            {
                                Name = entry.Key,
                                Value = m.GetColor(entry.Key)
                            });
                            break;
                        case ShaderPropType.Float:
                            list.Add(new SL_Material.FloatProp()
                            {
                                Name = entry.Key,
                                Value = m.GetFloat(entry.Key)
                            });
                            break;
                        case ShaderPropType.Vector:
                            list.Add(new SL_Material.VectorProp()
                            {
                                Name = entry.Key,
                                Value = m.GetVector(entry.Key)
                            });
                            break;
                    }
                }
            }
            else
            {
                SL.Log("Shader GetProperties not supported: " + m.shader.name, 0);
            }

            return list;
        }

        private static readonly Dictionary<string, Dictionary<string, ShaderPropType>> ShaderPropertyDicts = new Dictionary<string, Dictionary<string, ShaderPropType>>()
        {
            { "Custom/Main Set/Main Standard",      CustomMainSetMainStandard },
            { "Custom/Distort/DistortTextureSpec",  CustomDistortDistortTextureSpec }
        };

        /// <summary>
        /// Properties on Nine Dots' "Custom/Main Set/Main Standard" shader.
        /// </summary>
        public static Dictionary<string, ShaderPropType> CustomMainSetMainStandard = new Dictionary<string, ShaderPropType>
        {
            { "_Color",                 ShaderPropType.Color },
            { "_Cutoff",                ShaderPropType.Float },
            { "_Dither",                ShaderPropType.Float },
            { "_DoubleFaced",           ShaderPropType.Float },
            { "_NormStr",               ShaderPropType.Float },
            { "_SpecColor",             ShaderPropType.Color },
            { "_SmoothMin",             ShaderPropType.Float },
            { "_SmoothMax",             ShaderPropType.Float },
            { "_OccStr",                ShaderPropType.Float },
            { "_EmissionColor",         ShaderPropType.Color },
            { "_EmitAnimSettings",      ShaderPropType.Vector },
            { "_EmitScroll",            ShaderPropType.Float },
            { "_EmitPulse",             ShaderPropType.Float },
            { "_DetColor",              ShaderPropType.Color },
            { "_DetTiling",             ShaderPropType.Vector },
            { "_DetNormStr",            ShaderPropType.Float },
            { "_VPRTexColor",           ShaderPropType.Color },
            { "_VPRTexSettings",        ShaderPropType.Vector },
            { "_VPRSpecColor",          ShaderPropType.Color },
            { "_VPRNormStr",            ShaderPropType.Float },
            { "_VPRUnderAuto",          ShaderPropType.Float },
            { "_VPRTiling",             ShaderPropType.Float },
            { "_AutoTexColor",          ShaderPropType.Color },
            { "_AutoTexSettings",       ShaderPropType.Vector },
            { "_AutoTexHideEmission",   ShaderPropType.Float },
            { "_AutoSpecColor",         ShaderPropType.Color },
            { "_AutoNormStr",           ShaderPropType.Float },
            { "_AutoTexTiling",         ShaderPropType.Float },
            { "_SnowEnabled",           ShaderPropType.Float }
        };

        /// <summary>
        /// Properties on Nine Dots' "Custom/Distort/DistortTextureSpec" shader.
        /// </summary>
        public static Dictionary<string, ShaderPropType> CustomDistortDistortTextureSpec = new Dictionary<string, ShaderPropType>
        {
            { "_Color",             ShaderPropType.Color },
            { "_SpecColor",         ShaderPropType.Color },
            { "_NormalStrength",    ShaderPropType.Float },
            { "_Speed",             ShaderPropType.Float },
            { "_Scale",             ShaderPropType.Float },
            { "_MaskPow",           ShaderPropType.Float },
        };

        // ============= GLOBAL TEXTURE REPLACEMENT ===============

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

            foreach (Material m in list)
            {
                var texNames = m.GetTexturePropertyNames();

                foreach (var layer in texNames)
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

            var images = Resources.FindObjectsOfTypeAll<Image>().Where(x => x.sprite != null && x.sprite.texture != null);

            foreach (Image i in images)
            {
                if (Textures.ContainsKey(i.sprite.texture.name))
                {
                    SL.Log("Replacing sprite for " + i.name);
                    i.sprite = CreateSprite(Textures[i.sprite.texture.name]);
                }
            }

            var time = Math.Round(1000f * (Time.realtimeSinceStartup - start), 2);

            SL.Log("Finished replacing textures, took " + time + " ms");
        }
    }
}