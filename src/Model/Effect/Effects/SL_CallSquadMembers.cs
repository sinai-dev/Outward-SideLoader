namespace SideLoader
{
    public class SL_CallSquadMembers : SL_Effect
    {
        // This class uses no fields, it's a self-executing effect.

        public override void ApplyToComponent<T>(T component) { }

        public override void SerializeEffect<T>(T effect) { }
    }
}
