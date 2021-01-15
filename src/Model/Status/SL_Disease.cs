using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SL_Disease : SL_StatusEffect
    {
        public int? AutoHealTime;
        public bool? CanDegenerate;
        public float? DegenerateTime;
        public Diseases? DiseaseType;
        public int? SleepHealTime;

        internal override void ApplyTemplate(StatusEffect status)
        {
            base.ApplyTemplate(status);

            var comp = status as Disease;

            if (this.AutoHealTime != null)
                At.SetField(comp, "m_autoHealTime", (int)this.AutoHealTime);

            if (this.CanDegenerate != null)
                At.SetField(comp, "m_canDegenerate", (bool)this.CanDegenerate);

            if (this.DegenerateTime != null)
                At.SetField(comp, "m_degenerateTime", (float)this.DegenerateTime);

            if (this.DiseaseType != null)
                At.SetField(comp, "m_diseasesType", (Diseases)this.DiseaseType);

            if (this.SleepHealTime != null)
                At.SetField(comp, "m_straightSleepHealTime", (float)this.SleepHealTime);
        }

        public override void SerializeStatus(StatusEffect status)
        {
            base.SerializeStatus(status);

            var comp = status as Disease;

            this.AutoHealTime = (int)At.GetField(comp, "m_autoHealTime");
            this.CanDegenerate = (bool)At.GetField(comp, "m_canDegenerate");
            this.DegenerateTime = (float)At.GetField(comp, "m_degenerateTime");
            this.DiseaseType = (Diseases)At.GetField(comp, "m_diseasesType");
            this.SleepHealTime = comp.StraightSleepHealTime;
        }
    }
}
