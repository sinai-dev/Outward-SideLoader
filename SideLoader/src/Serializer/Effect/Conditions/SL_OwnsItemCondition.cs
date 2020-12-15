using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_OwnsItemCondition : SL_EffectCondition
    {
        public int ReqItemID;
        public int ReqAmount;

        public override void ApplyToComponent<T>(T component)
        {
            var item = ResourcesPrefabManager.Instance.GetItemPrefab(ReqItemID);

            if (!item)
            {
                SL.Log("SL_OwnsItemCondition: Could not find an Item with the ID " + this.ReqItemID + "!");
                return;
            }

            (component as OwnsItemCondition).ReqItem = item;
            (component as OwnsItemCondition).MinAmount = this.ReqAmount;
        }

        public override void SerializeEffect<T>(T component)
        {
            ReqItemID = (component as OwnsItemCondition).ReqItem.ItemID;
            ReqAmount = (component as OwnsItemCondition).MinAmount;
        }
    }
}
