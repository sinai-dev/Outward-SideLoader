using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class AffectStatHolder : EffectHolder
    {
        public string AffectedStat;
        public string AffectedStat_TagUID;
        public float AffectQuantity;
        public bool IsModifier;

        public static AffectStatHolder ParseAffectStat(AffectStat affectStat, EffectHolder _effectHolder)
        {
            var affectStatHolder = new AffectStatHolder
            {
                AffectedStat = affectStat.AffectedStat.Tag.TagName,
                AffectedStat_TagUID = affectStat.AffectedStat.Tag.UID.ToString(),
                AffectQuantity = affectStat.Value,
                IsModifier = affectStat.IsModifier
            };

            At.InheritBaseValues(affectStatHolder, _effectHolder);

            return affectStatHolder;
        }

    }
}
