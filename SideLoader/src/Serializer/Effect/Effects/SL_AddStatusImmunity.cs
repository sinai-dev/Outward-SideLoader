using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                SL.Log("Could not find a tag with the name " + ImmunityTag + "!");
                return;
            }

            At.SetValue(new TagSourceSelector(tag), typeof(AddStatusImmunity), component, "m_statusImmunity");
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var selector = (TagSourceSelector)At.GetValue(typeof(AddStatusImmunity), effect, "m_statusImmunity");
            (holder as SL_AddStatusImmunity).ImmunityTag = selector.Tag.TagName;
        }
    }
}
