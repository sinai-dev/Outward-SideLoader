using Partiality.Modloader;
using UnityEngine;

namespace SideLoader_2
{
    public class SideLoaderModLoader : PartialityMod
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