namespace SideLoader
{
    public class SL_StartDuel : SL_Effect
    {
        // This class doesn't use any fields, it's a self-executing effect.

        public override void ApplyToComponent<T>(T component) { }

        public override void SerializeEffect<T>(T effect) { }
    }
}
