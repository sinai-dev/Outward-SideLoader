using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_TrinketSkill : SL_Skill
    {
        public List<int> CompatibleItemIDs;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            if (this.CompatibleItemIDs != null)
            {
                var list = new List<Item>();
                foreach (var id in this.CompatibleItemIDs)
                {
                    if (ResourcesPrefabManager.Instance.GetItemPrefab(id) is Item compatItem)
                    {
                        list.Add(compatItem);
                    }
                }

                (item as TrinketSkill).CompatibleItems = list.ToArray();
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var skill = item as TrinketSkill;

            CompatibleItemIDs = new List<int>();
            foreach (var compatItem in skill.CompatibleItems)
            {
                CompatibleItemIDs.Add(compatItem.ItemID);
            }
        }
    }
}
