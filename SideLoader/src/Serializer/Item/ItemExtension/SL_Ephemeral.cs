using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Ephemeral : SL_ItemExtension
    {
        public float? Lifespan;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Ephemeral;

            if (this.Lifespan != null)
            {
                comp.LifeSpan = (float)this.Lifespan;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            this.Lifespan = (extension as Ephemeral).LifeSpan;
        }
    }
}
