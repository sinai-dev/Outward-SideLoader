namespace SideLoader
{
    public class SL_AddStatusImmunity : SL_Effect
    {
        public string ImmunityTag;

        public override void ApplyToComponent<T>(T component)
        {
            var tag = CustomTags.GetTag(ImmunityTag, false);

            if (tag == Tag.None)
            {
                SL.LogWarning($"{this.GetType().Name}: Could not find a tag with the name '{ImmunityTag}'!");
                return;
            }

            At.SetField(component as AddStatusImmunity, "m_statusImmunity", new TagSourceSelector(tag));
        }

        public override void SerializeEffect<T>(T effect)
        {
            var selector = (TagSourceSelector)At.GetField(effect as AffectNeed, "m_statusImmunity");
            ImmunityTag = selector.Tag.TagName;
        }
    }
}
