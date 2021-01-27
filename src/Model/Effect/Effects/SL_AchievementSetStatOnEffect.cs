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

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AchievementSetStatOnEffect;

            StatToChange = comp.StatToChange;
            IncreaseAmount = comp.IncreaseAmount;
        }
    }
}
