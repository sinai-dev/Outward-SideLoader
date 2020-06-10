using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_ItemStats
    {
        public int? BaseValue;
        public float? RawWeight;
        public int? MaxDurability;

        public virtual void ApplyToItem(ItemStats stats)
        {
            //set base value
            if (this.BaseValue != null)
            {
                At.SetValue((int)this.BaseValue, typeof(ItemStats), stats, "m_baseValue");
            }

            //set raw weight
            if (this.RawWeight != null)
            {
                At.SetValue((float)this.RawWeight, typeof(ItemStats), stats, "m_rawWeight");
            }

            //max durability
            if (this.MaxDurability != null)
            {
                At.SetValue((int)this.MaxDurability, typeof(ItemStats), stats, "m_baseMaxDurability");
                stats.StartingDurability = (int)this.MaxDurability;
            }
        }

        public static SL_ItemStats ParseItemStats(ItemStats stats)
        {
            var type = Serializer.GetSLType(stats.GetType());

            var holder = (SL_ItemStats)Activator.CreateInstance(type);

            holder.SerializeStats(stats, holder);

            return holder;
        }
    
        public virtual void SerializeStats(ItemStats stats, SL_ItemStats holder)
        {
            holder.BaseValue = stats.BaseValue;
            holder.MaxDurability = stats.MaxDurability;
            holder.RawWeight = stats.RawWeight;
        }
    }
}
