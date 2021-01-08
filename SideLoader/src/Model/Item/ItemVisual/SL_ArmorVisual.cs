using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ArmorVisuals : SL_ItemVisual
    {
        public bool? HideFace;
        public bool? HideHair;

        public override void ApplyItemVisualSettings(ItemVisual itemVisual, Transform actualVisuals)
        {
            base.ApplyItemVisualSettings(itemVisual, actualVisuals);

            var armorVisuals = itemVisual as ArmorVisuals;

            if (this.HideFace != null)
                armorVisuals.HideFace = (bool)HideFace;

            if (this.HideHair != null)
                armorVisuals.HideHair = (bool)HideHair;
        }

        public override void SerializeItemVisuals(ItemVisual itemVisual)
        {
            base.SerializeItemVisuals(itemVisual);

            var armorVis = itemVisual as ArmorVisuals;

            this.Position = itemVisual.transform.position;
            this.Rotation = itemVisual.transform.rotation.eulerAngles;

            HideFace = armorVis.HideFace;
            HideHair = armorVis.HideHair;
        }
    }
}
