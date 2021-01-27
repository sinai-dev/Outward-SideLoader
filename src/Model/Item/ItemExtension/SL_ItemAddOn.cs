using SideLoader.SLPacks;
using System;

namespace SideLoader
{
    public class SL_ItemAddOn : SL_Deployable
    {
        public int? AddOnCompatibleItemID;
        public int? AddOnStatePrefabItemID;
        public float? SnappingRadius;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as ItemAddOn;

            SLPackManager.AddLateApplyListener(OnLateApply, comp);

            if (this.SnappingRadius != null)
                At.SetField(comp, "m_snappingRadius", (float)this.SnappingRadius);
        }

        private void OnLateApply(object[] obj)
        {
            var comp = obj[0] as ItemAddOn;

            if (!comp)
                return;

            if (this.AddOnCompatibleItemID != null)
            {
                var addOnComptaible = ResourcesPrefabManager.Instance.GetItemPrefab((int)this.AddOnCompatibleItemID);
                if (addOnComptaible)
                    At.SetField(comp, "m_addOnCompatibleItem", addOnComptaible);
            }

            if (this.AddOnStatePrefabItemID != null)
            {
                var addOnStateItem = ResourcesPrefabManager.Instance.GetItemPrefab((int)this.AddOnStatePrefabItemID);
                if (addOnStateItem)
                    comp.AddOnStateItemPrefab = addOnStateItem;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            base.SerializeComponent(extension);

            var comp = extension as ItemAddOn;

            this.SnappingRadius = comp.SnappingRadius;
            this.AddOnCompatibleItemID = comp.CompatibleItem?.ItemID ?? -1;

        }
    }
}
