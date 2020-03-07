using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dataminer
{
    public class ItemStatsHolder
    {
        public static ItemStatsHolder ParseItemStats(ItemStats stats)
        {
            var itemStatsHolder = new ItemStatsHolder
            {
                BaseValue = stats.BaseValue,
                MaxDurability = stats.MaxDurability,
                RawWeight = stats.RawWeight
            };

            // todo equipmentstats, weaponstats

            return itemStatsHolder;
        }

        public int BaseValue;
        public float RawWeight;
        public int MaxDurability;
    }
}
