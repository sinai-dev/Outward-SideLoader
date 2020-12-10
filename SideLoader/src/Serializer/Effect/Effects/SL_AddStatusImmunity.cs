using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.Helpers;

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

            At.SetField(new TagSourceSelector(tag), "m_statusImmunity", component as AddStatusImmunity);
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var selector = (TagSourceSelector)At.GetField("m_statusImmunity", effect as AffectNeed);
            (holder as SL_AddStatusImmunity).ImmunityTag = selector.Tag.TagName;
        }
    }
}
