using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader
{
    /// <summary>Wrapper for serializing a UnityEngine.Material object.</summary>
    [SL_Serialized]
    public class SL_Material
    {
        /// <summary>The name of the Material (readonly).</summary>
        [XmlIgnore]
        public string Name { get; private set; }

        /// <summary>The Shader to use for the Material. Should be the same value use by Shader.Find()</summary>
        public string ShaderName;

        /// <summary>Shader Keywords to enable.</summary>
        public string[] Keywords;
        /// <summary>List of Shader Properties to set.</summary>
        public ShaderProperty[] Properties;
        /// <summary>List of TextureConfigs to apply.</summary>
        public TextureConfig[] TextureConfigs; 

        /// <summary>Applies this SL_Material template to the provided Material.</summary>
        /// <param name="mat">The material to apply to.</param>
        public void ApplyToMaterial(Material mat)
        {
            SL.Log("SL_Material applying to " + mat.name);

            if (!string.IsNullOrEmpty(this.ShaderName) && Shader.Find(this.ShaderName) is Shader shader)
            {
                SL.Log("Set material shader to " + shader.name);
                mat.shader = shader;
            }

            if (Keywords != null)
            {
                mat.shaderKeywords = new string[0];
                foreach (var keyword in this.Keywords)
                {
                    try
                    {
                        mat.EnableKeyword(keyword);
                    }
                    catch { }
                }
            }

            if (this.Properties != null)
            {
                foreach (var prop in this.Properties)
                {
                    if (mat.HasProperty(prop.Name))
                    {
                        try
                        {
                            if (prop is FloatProp fProp)
                                mat.SetFloat(prop.Name, fProp.Value);
                            else if (prop is ColorProp cProp)
                                mat.SetColor(prop.Name, cProp.Value);
                            else if (prop is VectorProp vProp)
                                mat.SetVector(prop.Name, vProp.Value);
                            else
                                SL.Log("Cannot set ShaderProp of type: " + prop.GetType());
                        }
                        catch (Exception e)
                        {
                            SL.Log("Unhandled exception setting shader property " + prop.Name + "\r\n"
                                + "Message: " + e.Message + "\r\n"
                                + "Stack: " + e.StackTrace);
                        }
                    }
                    else
                        SL.Log("Trying to set ShaderProperty " + prop.Name + " but this material does not have such a property!");
                }
            }
        }

        /// <summary>
        /// Apply the TextureConfigs to the provided Material.
        /// </summary>
        /// <param name="mat">The material to apply to.</param>
        public void ApplyTextureSettings(Material mat)
        {
            var dict = TextureConfigsToDict();

            foreach (var texName in mat.GetTexturePropertyNames())
            {
                if (dict.ContainsKey(texName) && mat.GetTexture(texName) is Texture2D tex)
                {
                    var cfg = dict[texName];

                    tex.mipMapBias = cfg.MipMapBias;
                }
            }
        }

        /// <summary>
        /// Converts the TextureConfigs list into a Dictionary (key: Texture name).
        /// </summary>
        /// <returns>The completed dictionary.</returns>
        public Dictionary<string, TextureConfig> TextureConfigsToDict()
        {
            var dict = new Dictionary<string, TextureConfig>();

            if (this.TextureConfigs != null)
            {
                foreach (var cfg in this.TextureConfigs)
                {
                    dict.Add(cfg.TextureName, cfg);
                }
            }

            return dict;
        }

        /// <summary>
        /// Serializes a Material into a SL_Material.
        /// </summary>
        /// <param name="mat">The material to serialize.</param>
        /// <returns>Serialized SL_Material.</returns>
        public static SL_Material ParseMaterial(Material mat)
        {
            var holder = new SL_Material()
            {
                Name = mat.name,
                ShaderName = mat.shader.name,
                Properties = CustomTextures.GetProperties(mat).ToArray(),
                Keywords = mat.shaderKeywords,
            };

            var list = new List<TextureConfig>();
            foreach (var texName in mat.GetTexturePropertyNames())
            {
                if (mat.GetTexture(texName) is Texture2D tex)
                {
                    list.Add(new TextureConfig()
                    {
                        TextureName = texName,
                        MipMapBias = tex.mipMapBias,
                        UseMipMap = (tex.mipmapCount > 0),
                    });
                }
            }
            holder.TextureConfigs = list.ToArray();

            return holder;
        }

        /// <summary>
        /// Container class for setting config values to a Texture on a Material.
        /// </summary>
        [SL_Serialized]
        public class TextureConfig
        {
            /// <summary>The name of the Texture to apply to (shader layer name).</summary>
            public string TextureName;
            /// <summary>Whether or not to use MipMap on the texture.</summary>
            public bool UseMipMap = true;
            /// <summary>If using MipMap, the bias level.</summary>
            public float MipMapBias = 0;
        }

        /// <summary>Abstract wrapper used to serialize Shader Properties.</summary>
        [SL_Serialized]
        public abstract class ShaderProperty
        {
            /// <summary>Name of the Property.</summary>
            public string Name;
        }

        public class FloatProp : ShaderProperty
        {
            /// <summary>Float value to set.</summary>
            public float Value;
        }

        public class ColorProp : ShaderProperty
        {
            /// <summary>UnityEngine.Color value to set.</summary>
            public Color Value;
        }

        public class VectorProp : ShaderProperty
        {
            /// <summary>UnityEngine.Vector4 value to set.</summary>
            public Vector4 Value;
        }
    }
}
