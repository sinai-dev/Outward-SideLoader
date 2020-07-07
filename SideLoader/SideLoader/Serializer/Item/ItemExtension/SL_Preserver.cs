using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Preserver : SL_ItemExtension
    {
        public override bool AddToChild => true;
        public override string ChildToAddTo => "Content";

        public bool? NullifyPerish;
        public List<SL_PreservedElement> PreservedElements;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Preserver;

            if (this.NullifyPerish != null)
            {
                comp.NullifyPerishing = (bool)this.NullifyPerish;
            }

            if (this.PreservedElements != null)
            {
                var list = new List<Preserver.PreservedElement>();

                foreach (var ele in this.PreservedElements)
                {
                    list.Add(new Preserver.PreservedElement
                    {
                        Preservation = ele.Preservation,
                        Tag = new TagSourceSelector(CustomTags.GetTag(ele.PreservedItemTag)),
                    });
                }

                At.SetValue(list, typeof(Preserver), comp, "m_preservedElements");
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as Preserver;

            this.NullifyPerish = comp.NullifyPerishing;

            var list = (List<Preserver.PreservedElement>)At.GetValue(typeof(Preserver), comp, "m_preservedElements");
            if (list != null)
            {
                this.PreservedElements = new List<SL_PreservedElement>();
                foreach (var ele in list)
                {
                    this.PreservedElements.Add(new SL_PreservedElement
                    {
                        Preservation = ele.Preservation,
                        PreservedItemTag = ele.Tag.Tag.TagName
                    });
                }
            }
        }

        [SL_Serialized]
        public class SL_PreservedElement
        {
            public float Preservation;
            public string PreservedItemTag;
        }
    }
}
