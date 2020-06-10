using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectStat : SL_Effect
    {
        public string Stat_Tag = "";
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            var tag = CustomItems.GetTag(Stat_Tag);

            if (tag == Tag.None)
            {
                SL.Log("AffectStat: could not find tag of ID " + (this.Stat_Tag ?? ""), 0);
                return;
            }

            (component as AffectStat).AffectedStat = new TagSourceSelector(tag);
            (component as AffectStat).Value = this.AffectQuantity;
            (component as AffectStat).IsModifier = this.IsModifier;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_AffectStat).Stat_Tag = (effect as AffectStat).AffectedStat.Tag.TagName;
            (holder as SL_AffectStat).AffectQuantity = (effect as AffectStat).Value;
            (holder as SL_AffectStat).IsModifier = (effect as AffectStat).IsModifier;
        }
    }
}
