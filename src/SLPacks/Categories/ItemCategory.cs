using SideLoader.Model;
using UnityEngine;

namespace SideLoader.SLPacks.Categories
{
    public class ItemCategory : SLPackTemplateCategory<SL_Item>
    {
        public override string FolderName => "Items";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.Item;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var item = template as SL_Item;

            item.ApplyActualTemplate();
        }

        protected internal override void OnHotReload()
        {
            base.OnHotReload();

            foreach (var item in AllCurrentTemplates)
            {
                if (item.New_ItemID != item.Target_ItemID)
                {
                    if (item.CurrentPrefab)
                        GameObject.Destroy(item.CurrentPrefab);

                    if (References.RPM_ITEM_PREFABS.ContainsKey(item.New_ItemID.ToString()))
                        References.RPM_ITEM_PREFABS.Remove(item.New_ItemID.ToString());
                }
            }

            SL_Skill.s_customSkills.Clear();
        }
    }
}
