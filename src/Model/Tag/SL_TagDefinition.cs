using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_TagDefinition
    {
        public string TagName = "";
        public string OptionalTagUID = "";

        public override string ToString() => TagName ?? "";

        public void CreateTag()
        {
            if (CustomTags.GetTag(this.TagName, false) != Tag.None)
            {
                SL.LogWarning($"Creating Tag '{this.TagName}', but it already exists!");
                return;
            };

            var tag = CustomTags.CreateTag(TagName);

            if (!string.IsNullOrEmpty(this.OptionalTagUID))
                At.SetField(tag, "m_uid", new UID(this.OptionalTagUID));
        }
    }
}
