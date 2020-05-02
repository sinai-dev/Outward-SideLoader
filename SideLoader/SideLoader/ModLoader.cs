using Partiality.Modloader;
using UnityEngine;

namespace SideLoader
{
    public class SideLoader : PartialityMod
    {
        private static GameObject m_obj;

        public SideLoader()
        {
            this.ModID = SL.MODNAME;
            this.Version = SL.VERSION;
            this.author = "Sinai";
            this.loadPriority = -999; // lower number = higher priority
        }

        public override void OnLoad()
        {
            base.OnLoad();

            m_obj = new GameObject(SL.MODNAME);
            Object.DontDestroyOnLoad(m_obj);
            m_obj.AddComponent<SL>();
        }
    }
}