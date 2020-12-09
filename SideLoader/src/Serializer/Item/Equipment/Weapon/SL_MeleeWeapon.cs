using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_MeleeWeapon : SL_Weapon
    {
        public int? LinecastCount;
        public float? Radius;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var weapon = item as MeleeWeapon;

            if (this.LinecastCount != null)
            {
                weapon.LinecastCount = (int)this.LinecastCount;
            }
            if (this.Radius != null)
            {
                weapon.Radius = (float)this.Radius;
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var template = holder as SL_MeleeWeapon;
            var weapon = item as MeleeWeapon;

            template.LinecastCount = weapon.LinecastCount;
            template.Radius = weapon.Radius;
        }
    }
}
