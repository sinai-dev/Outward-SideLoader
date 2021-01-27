using System.Linq;

namespace SideLoader
{
    public class SL_AffectStat : SL_Effect
    {
        public string Stat_Tag = "";
        public float AffectQuantity;
        public bool IsModifier;
        public string[] Tags;

        public override void ApplyToComponent<T>(T component)
        {
            var tag = CustomItems.GetTag(Stat_Tag);

            if (tag == Tag.None)
            {
                SL.Log("AffectStat: could not find tag of ID " + (this.Stat_Tag ?? ""));
                return;
            }

            var comp = component as AffectStat;

            comp.AffectedStat = new TagSourceSelector(tag);
            comp.Value = this.AffectQuantity;
            comp.IsModifier = this.IsModifier;

            if (this.Tags != null)
            {
                comp.Tags = this.Tags
                                .Select(it => new TagSourceSelector(CustomTags.GetTag(it)))
                                .ToArray();
            }
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AffectStat;
            this.Stat_Tag = comp.AffectedStat.Tag.TagName;
            this.AffectQuantity = comp.Value;
            this.IsModifier = comp.IsModifier;

            if (comp.Tags != null)
                this.Tags = comp.Tags
                           .Select(it => it.Tag.TagName)
                           .ToArray();
        }
    }
}
