namespace SideLoader
{
    [SL_Serialized]
    public class SL_ItemQty
    {
        public override string ToString()
                => $"{Quantity}x {ResourcesPrefabManager.Instance.GetItemPrefab(ItemID)?.Name ?? "<not found>"}";

        public int ItemID;
        public int Quantity = 1;
    }
}
