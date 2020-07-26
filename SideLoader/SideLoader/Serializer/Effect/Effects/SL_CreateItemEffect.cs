using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_CreateItemEffect : SL_Effect
    {
        public int ItemToCreate;
        public int Quantity = 1;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as CreateItemEffect;

            if (ResourcesPrefabManager.Instance.GetItemPrefab(this.ItemToCreate) is Item item)
            {
                comp.ItemToCreate = item;
                comp.Quantity = this.Quantity;
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var template = holder as SL_CreateItemEffect;
            var comp = effect as CreateItemEffect;

            template.ItemToCreate = comp.ItemToCreate?.ItemID ?? -1;
            template.Quantity = comp.Quantity;
        }
    }
}
