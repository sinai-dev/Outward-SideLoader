using Partiality.Modloader;
using UnityEngine;

namespace SideLoader
{
    public class SideLoaderModLoader : PartialityMod
    {
        private static GameObject m_obj = null;

        public override void OnEnable()
        {
            base.OnEnable();
            if (m_obj != null)
            {
                m_obj = new GameObject(SL.MODNAME);
                GameObject.DontDestroyOnLoad(m_obj);
                m_obj.AddComponent<SL>();
            }
        }
    }
}