using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace SideLoader
{
    public class SL_Material
    {
        [XmlIgnore]
        public string Name { get; private set; }

        public string ShaderName;

        public List<string> Keywords = new List<string>();
        public List<ShaderProperty> Properties = new List<ShaderProperty>();
        public List<TextureConfig> TextureConfigs = new List<TextureConfig>(); 

        public void ApplyToMaterial(Material mat)
        {
            SL.Log("SL_Material applying to " + mat.name);

            if (!string.IsNullOrEmpty(this.ShaderName) && Shader.Find(this.ShaderName) is Shader shader)
            {
                SL.Log("Set material shader to " + shader.name);
                mat.shader = shader;
            }

            if (Keywords != null && Keywords.Count > 0)
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

            foreach (var prop in this.Properties)
            {
                if (mat.HasProperty(prop.Name))
                {
                    try
                    {
                        if (prop is FloatProp fProp)
                        {
                            mat.SetFloat(prop.Name, fProp.Value);
                        }
                        else if (prop is ColorProp cProp)
                        {
                            mat.SetColor(prop.Name, cProp.Value);
                        }
                        else if (prop is VectorProp vProp)
                        {
                            mat.SetVector(prop.Name, vProp.Value);
                        }
                        else
                        {
                            SL.Log("Cannot set ShaderProp of type: " + prop.GetType());
                        }
                    }
                    catch (Exception e)
                    {
                        SL.Log("Unhandled exception setting shader property " + prop.Name + "\r\n"
                            + "Message: " + e.Message + "\r\n"
                            + "Stack: " + e.StackTrace);
                    }
                }
                else
                {
                    SL.Log("Trying to set ShaderProperty " + prop.Name + " but this material does not have such a property!");
                }
            }
        }

        public void ApplyTextureSettings(Material mat)
        {
            var dict = TextureConfigsToDict();

            foreach (var texName in mat.GetTexturePropertyNames())
            {
                if (dict.ContainsKey(texName))
                {
                    var cfg = dict[texName];
                    var tex = mat.GetTexture(texName);

                    tex.mipMapBias = cfg.MipMapBias;
                }
            }
        }

        public Dictionary<string, TextureConfig> TextureConfigsToDict()
        {
            var dict = new Dictionary<string, TextureConfig>();

            foreach (var cfg in this.TextureConfigs)
            {
                dict.Add(cfg.TextureName, cfg);
            }

            return dict;
        }

        public static SL_Material ParseMaterial(Material mat)
        {
            var holder = new SL_Material()
            {
                Name = mat.name,
                ShaderName = mat.shader.name,
                Properties = CustomTextures.GetProperties(mat),
                Keywords = mat.shaderKeywords.ToList(),
            };

            foreach (var texName in mat.GetTexturePropertyNames())
            {
                if (mat.GetTexture(texName) is Texture2D tex)
                {
                    holder.TextureConfigs.Add(new TextureConfig()
                    {
                        TextureName = texName,
                        MipMapBias = tex.mipMapBias,
                        UseMipMap = (tex.mipmapCount > 0),
                    });
                }
            }

            return holder;
        }

        public class TextureConfig
        {
            public string TextureName;
            public bool UseMipMap = true;
            public float MipMapBias = 0;
        }

        public abstract class ShaderProperty
        {
            public string Name;
        }

        public class FloatProp : ShaderProperty
        {
            public float Value;
        }

        public class ColorProp : ShaderProperty
        {
            public Color Value;
        }

        public class VectorProp : ShaderProperty
        {
            public Vector4 Value;
        }
    }
}
