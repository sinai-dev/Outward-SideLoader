using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_ItemDrop
    {
        public override string ToString()
                   => $"{MinQty}-{MaxQty}x {ResourcesPrefabManager.Instance.GetItemPrefab(DroppedItemID)?.Name ?? "<not found>"}";

        public int MinQty = 1;
        public int MaxQty = 1;

        public int DroppedItemID;

        public void GenerateDrop(Transform container)
		{
			if (!container)
				return;

			var itemContainer = container.GetComponent<ItemContainer>();
			if (!itemContainer)
            {
				SL.LogWarning("Generating an SL_ItemDrop but the container does not have an ItemContainer component: " + container.GetGameObjectPath());
				return;
			}

			var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(this.DroppedItemID);
			if (!prefab)
            {
				SL.LogWarning("Generating an SL_ItemDrop but the DroppedItemID is invalid: " + this.DroppedItemID);
				return;
            }

			var qtyToAdd = Random.Range(this.MinQty, this.MaxQty + 1);
			if (qtyToAdd <= 0)
				return;

			//SL.Log("Adding " + qtyToAdd + " of " + prefab.Name);

			if (prefab is Currency)
            {
				itemContainer.AddSilver(qtyToAdd);
            }
			else
            {
				var item = ItemManager.Instance.GenerateItemNetwork(prefab.ItemID);
				item.ChangeParent(container);

				qtyToAdd--;

				if (qtyToAdd >= 1)
                {
					if (!prefab.HasMultipleUses)
                    {
						for (int i = 0; i < qtyToAdd; i++)
                        {
							item = ItemManager.Instance.GenerateItemNetwork(DroppedItemID);
							item.ChangeParent(container);
						}
                    }
					else
                    {
						if (qtyToAdd > item.MaxStackAmount)
						{
							item.RemainingAmount = item.MaxStackAmount;
							for (qtyToAdd -= item.RemainingAmount; qtyToAdd > 0; qtyToAdd -= item.RemainingAmount)
							{
								item = ItemManager.Instance.GenerateItemNetwork(DroppedItemID);
								item.ChangeParent(container);
								item.RemainingAmount = Mathf.Min(qtyToAdd, item.MaxStackAmount);
							}
							return;
						}
						item.RemainingAmount = qtyToAdd;
					}
                }
            }
		}
    }
}
