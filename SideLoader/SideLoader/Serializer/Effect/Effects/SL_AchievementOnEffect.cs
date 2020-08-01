using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_AchievementOnEffect : SL_Effect
    {
        public AchievementManager.Achievement UnlockedAchievement;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AchievementOnEffect).UnlockedAchievement = this.UnlockedAchievement;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AchievementOnEffect).UnlockedAchievement = (effect as AchievementOnEffect).UnlockedAchievement;
        }
    }
}
