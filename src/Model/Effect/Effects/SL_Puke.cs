namespace SideLoader
{
    public class SL_Puke : SL_Effect
    {
        public int ChanceToTrigger;

        public override void ApplyToComponent<T>(T component)
        {
            (component as Puke).ChancesToTrigger = this.ChanceToTrigger;
        }

        public override void SerializeEffect<T>(T effect)
        {
            ChanceToTrigger = (effect as Puke).ChancesToTrigger;
        }
    }
}
