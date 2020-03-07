using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Localizer;
using System.IO;
//using OModAPI;

namespace SideLoader_2
{
    public class SL_Items : MonoBehaviour
    {
        public static SL_Items Instance;

        public Dictionary<int, Item> LoadedCustomItems = new Dictionary<int, Item>();

        internal void Awake()
        {
            Instance = this;
        }

        public IEnumerator LoadItemXMLs()
        {
            yield return null;
        }

        
    }
}
