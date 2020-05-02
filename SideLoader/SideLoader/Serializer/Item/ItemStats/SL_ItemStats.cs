using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ItemStats
    {
        public int? BaseValue;
        public float? RawWeight;
        public int? MaxDurability;

        public void ApplyToItem(ItemStats stats)
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

            if (this is SL_EquipmentStats equipStatsHolder)
            {
                equipStatsHolder.ApplyToItem(stats as EquipmentStats);
            }
        }

        public static SL_ItemStats ParseItemStats(ItemStats stats)
        {
            var itemStatsHolder = new SL_ItemStats
            {
                BaseValue = stats.BaseValue,
                MaxDurability = stats.MaxDurability,
                RawWeight = stats.RawWeight
            };

            // todo equipmentstats, weaponstats

            return itemStatsHolder;
        }
    }
}
