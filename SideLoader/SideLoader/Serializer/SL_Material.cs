using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;
 
// ****************************************************************** //
/*
 * This class is not yet finished or used.
*/
// ****************************************************************** //

namespace SideLoader
{
    public class SL_Material
    {
        [XmlIgnore]
        public string Name { get; private set; }

        public string ShaderName;

        public List<ShaderProperty> Properties = new List<ShaderProperty>();

        public List<string> Keywords = new List<string>();

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
                            SL.Log("Set float property " + prop.Name);
                        }
                        else if (prop is ColorProp cProp)
                        {
                            mat.SetColor(prop.Name, cProp.Value);
                            SL.Log("Set color property " + prop.Name);
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

        public static SL_Material ParseMaterial(Material mat)
        {
            var holder = new SL_Material()
            {
                Name = mat.name,
                ShaderName = mat.shader.name,
                Properties = new List<ShaderProperty>(),
                Keywords = mat.shaderKeywords.ToList(),
            };

            return holder;
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
    }
}
