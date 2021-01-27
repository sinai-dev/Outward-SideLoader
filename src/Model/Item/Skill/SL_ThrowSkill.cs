using SideLoader.SLPacks;
using System;
using System.Collections.Generic;

namespace SideLoader
{
    public class SL_ThrowSkill : SL_AttackSkill
    {
        public List<int> ThrowableItemIDs;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);
        }

        public override void LateApply(Skill skill)
        {
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

                (skill as ThrowSkill).ThrowableItems = list.ToArray();
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var skill = item as ThrowSkill;

            ThrowableItemIDs = new List<int>();
            foreach (var throwable in skill.ThrowableItems)
            {
                ThrowableItemIDs.Add(throwable.ItemID);
            }
        }
    }
}
