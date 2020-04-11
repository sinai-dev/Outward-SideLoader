using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
 
// ****************************************************************** //
/*
 * This class is not yet finished or used.
*/
// ****************************************************************** //

namespace SideLoader
{
    public class SL_Material
    {
        public string Name { get; private set; }

        public string ShaderName;

        public List<ShaderProperty> Properties = new List<ShaderProperty>();

        public static SL_Material ParseMaterial(Material mat)
        {
            var holder = new SL_Material()
            {
                Name = mat.name,
                ShaderName = mat.shader.name
            };

            return holder;
        }

        public class ShaderProperty
        {
            public string Key;
            public object Value;
        }
    }
}
