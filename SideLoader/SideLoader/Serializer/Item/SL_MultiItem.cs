using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_MultiItem
    {
        public List<SL_Item> Items = new List<SL_Item>();

        public void ApplyTemplates()
        {
            foreach (var template in this.Items)
            {
                template.Apply();
            }
        }

        public static List<SL_Item> ParseItems(List<Item> Items)
        {
            var list = new List<SL_Item>();
            foreach (var item in Items)
            {
                var template = SL_Item.ParseItemToTemplate(item);
                list.Add(template);
            }
            return list;
        }
    }
}
