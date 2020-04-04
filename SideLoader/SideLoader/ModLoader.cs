using Partiality.Modloader;
using UnityEngine;

namespace SideLoader
{
    public class SideLoaderModLoader : PartialityMod
    {
        private static GameObject m_obj;

        public override void OnLoad()
        {
            base.OnLoad();

            m_obj = new GameObject(SL.MODNAME);            
            GameObject.DontDestroyOnLoad(m_obj);
            m_obj.AddComponent<SL>();
        }
    }
}