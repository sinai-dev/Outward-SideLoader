using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader_2
{
    public class SL_AssetBundles : MonoBehaviour
    {
        public static SL_AssetBundles Instance;

        internal void Awake()
        {
            Instance = this;
        }

        public static AssetBundle LoadAssetBundle(string filepath)
        {
            try
            {
                return AssetBundle.LoadFromFile(filepath);
            }
            catch (Exception e)
            {
                SideLoader.Log(string.Format("Error loading bundle: {0}\r\nMessage: {1}\r\nStack Trace: {2}", filepath, e.Message, e.StackTrace), 1);
                return null;
            }
        }
    }
}
