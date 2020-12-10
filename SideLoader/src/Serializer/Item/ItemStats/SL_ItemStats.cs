using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.Helpers;
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
                At.SetField((int)this.BaseValue, "m_baseValue", stats);
            }

            //set raw weight
            if (this.RawWeight != null)
            {
                At.SetField((float)this.RawWeight, "m_rawWeight", stats);
            }

            //max durability
            if (this.MaxDurability != null)
            {
                At.SetField((int)this.MaxDurability, "m_baseMaxDurability", stats);
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
