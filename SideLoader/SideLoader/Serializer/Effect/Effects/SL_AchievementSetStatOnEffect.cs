using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_AchievementSetStatOnEffect : SL_Effect
    {
        public AchievementManager.AchievementStat StatToChange;
        public int IncreaseAmount = 1;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AchievementSetStatOnEffect;

            comp.StatToChange = this.StatToChange;
            comp.IncreaseAmount = this.IncreaseAmount;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var template = holder as SL_AchievementSetStatOnEffect;
            var comp = effect as AchievementSetStatOnEffect;

            template.StatToChange = comp.StatToChange;
            template.IncreaseAmount = comp.IncreaseAmount;
        }
    }
}
