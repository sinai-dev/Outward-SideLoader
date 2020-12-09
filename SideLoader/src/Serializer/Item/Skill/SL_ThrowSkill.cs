using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ThrowSkill : SL_AttackSkill
    {
        public List<int> ThrowableItemIDs;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            if (this.ThrowableItemIDs != null)
            {
                var list = new List<Item>();
                foreach (var id in this.ThrowableItemIDs)
                {
                    if (ResourcesPrefabManager.Instance.GetItemPrefab(id) is Item throwable)
                    {
                        list.Add(throwable);
                    }
                }

                (item as ThrowSkill).ThrowableItems = list.ToArray();
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_ThrowSkill;
            var skill = item as ThrowSkill;

            template.ThrowableItemIDs = new List<int>();
            foreach (var throwable in skill.ThrowableItems)
            {
                template.ThrowableItemIDs.Add(throwable.ItemID);
            }
        }
    }
}
