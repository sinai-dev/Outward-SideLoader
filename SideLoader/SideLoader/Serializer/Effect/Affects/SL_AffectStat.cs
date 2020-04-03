using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AffectStat : SL_Effect
    {
        public string Stat_Tag;
        public string Stat_ID;
        public float AffectQuantity;
        public bool IsModifier;

        public new void ApplyToTransform(Transform t)
        {
            var tag = TagSourceManager.Instance.GetTag(Stat_ID);

            if (tag == null || tag == Tag.None)
            {
                SL.Log("AffectStat: could not find tag of ID " + (this.Stat_ID ?? ""), 0);
                return;
            }

            var component = t.gameObject.AddComponent<AffectStat>();

            component.AffectedStat = new TagSourceSelector(tag);
            component.Value = this.AffectQuantity;
            component.IsModifier = this.IsModifier;
        }

        public static SL_AffectStat ParseAffectStat(AffectStat affectStat, SL_Effect _effectHolder)
        {
            var affectStatHolder = new SL_AffectStat
            {
                Stat_Tag = affectStat.AffectedStat.Tag.TagName,
                Stat_ID = affectStat.AffectedStat.Tag.UID.ToString(),
                AffectQuantity = affectStat.Value,
                IsModifier = affectStat.IsModifier
            };

            At.InheritBaseValues(affectStatHolder, _effectHolder);

            return affectStatHolder;
        }

    }
}
