using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Ephemeral : SL_ItemExtension
    {
        public float? Lifespan;
        public float? LifeSpanSkillKnowledge;
        public int? SkillToKnowID;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Ephemeral;

            if (this.Lifespan != null)
                comp.LifeSpan = (float)this.Lifespan;

            if (this.SkillToKnowID != null)
                comp.SkillToKnowID = (int)this.SkillToKnowID;

            if (this.LifeSpanSkillKnowledge != null)
                comp.LifeSpanSkillKnowledge = (float)this.LifeSpanSkillKnowledge;
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as Ephemeral;

            this.Lifespan = comp.LifeSpan;
            this.SkillToKnowID = comp.SkillToKnowID;
            this.LifeSpanSkillKnowledge = comp.LifeSpanSkillKnowledge;
        }
    }
}
