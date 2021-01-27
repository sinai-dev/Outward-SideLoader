namespace SideLoader
{
    public class SL_AchievementOnEffect : SL_Effect
    {
        public AchievementManager.Achievement UnlockedAchievement;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AchievementOnEffect).UnlockedAchievement = this.UnlockedAchievement;
        }

        public override void SerializeEffect<T>(T effect)
        {
            UnlockedAchievement = (effect as AchievementOnEffect).UnlockedAchievement;
        }
    }
}
