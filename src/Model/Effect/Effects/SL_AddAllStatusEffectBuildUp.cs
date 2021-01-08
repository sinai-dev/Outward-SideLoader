using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SL_AddAllStatusEffectBuildUp : SL_Effect
    {
        public float BuildUpValue;
        public bool NoDealer;
        public bool AffectController;
        public float BuildUpBonus;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AddAllStatusEffectBuildUp;

            comp.BuildUpValue = this.BuildUpValue;
            comp.NoDealer = this.NoDealer;
            comp.AffectController = this.AffectController;
            At.SetField(comp, "m_buildUpBonus", this.BuildUpBonus);
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AddAllStatusEffectBuildUp;

            BuildUpValue = comp.BuildUpValue;
            NoDealer = comp.NoDealer;
            AffectController = comp.AffectController;
            BuildUpBonus = (float)At.GetField(comp, "m_buildUpBonus");
        }
    }
}
