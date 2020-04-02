using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Partiality.Modloader;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace SideLoader_2
{
    public class ModLoader : PartialityMod
    {
        public override void OnEnable()
        {
            base.OnEnable();

            var obj = new GameObject(SL.MODNAME);
            GameObject.DontDestroyOnLoad(obj);

            obj.AddComponent<SL>();
        }
    }
}