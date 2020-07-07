using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_WeaponLoadoutItem : SL_WeaponLoadout
    {
        public bool? ReduceAmmunitionOnLoad;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            if (this.ReduceAmmunitionOnLoad != null)
            {
                (component as WeaponLoadoutItem).ReduceAmmunitionOnLoad = (bool)this.ReduceAmmunitionOnLoad;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            base.SerializeComponent(extension);

            this.ReduceAmmunitionOnLoad = (extension as WeaponLoadoutItem).ReduceAmmunitionOnLoad;
        }
    }
}
