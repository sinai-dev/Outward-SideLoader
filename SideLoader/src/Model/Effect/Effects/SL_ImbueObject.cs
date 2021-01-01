using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SL_ImbueObject : SL_Effect
    {
        public float Lifespan;
        public int ImbueEffect_Preset_ID;

        public override void ApplyToComponent<T>(T component)
        {
            var preset = ResourcesPrefabManager.Instance.GetEffectPreset(this.ImbueEffect_Preset_ID);

            if (!preset)
            {
                SL.Log($"{this.GetType().Name}: Could not find imbue effect preset of ID '{this.ImbueEffect_Preset_ID}'");
                return;
            }

            var comp = component as ImbueObject;

            comp.SetLifespanImbue(this.Lifespan);
            comp.ImbuedEffect = preset as ImbueEffectPreset;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as ImbueObject;

            ImbueEffect_Preset_ID = comp.ImbuedEffect?.PresetID ?? -1;
            Lifespan = comp.LifespanImbue;
        }
    }
}
