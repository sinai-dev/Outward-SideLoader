namespace SideLoader
{
    public class SL_InstrumentTriggerBubble : SL_Effect
    {
        public float Range;

        public override void ApplyToComponent<T>(T component)
        {
            (component as InstrumentTriggerBubble).Range = this.Range;
        }

        public override void SerializeEffect<T>(T effect)
        {
            this.Range = (effect as InstrumentTriggerBubble).Range;
        }
    }
}
