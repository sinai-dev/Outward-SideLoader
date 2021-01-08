using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Perishable : SL_ItemExtension
    {
        public float? BaseDepletionRate;
        public bool? DisableInInventory;
        public bool? DontPerishInWorld;
        public bool? DontPerishSkipTime;
        public float? OverrideUpdateRate;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Perishable;

            if (this.BaseDepletionRate != null)
            {
                comp.SetDurabilityDepletion((float)this.BaseDepletionRate);
            }
            if (this.DisableInInventory != null)
            {
                comp.DisableInInventory = (bool)this.DisableInInventory;
            }
            if (this.DontPerishInWorld != null)
            {
                comp.DontPerishInWorld = (bool)this.DontPerishInWorld;
            }
            if (this.DontPerishSkipTime != null)
            {
                comp.DontPerishSkipTime = (bool)this.DontPerishSkipTime;
            }
            if (this.OverrideUpdateRate != null)
            {
                comp.OverrideUpdateRate = (float)this.OverrideUpdateRate;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as Perishable;

            this.BaseDepletionRate = comp.DepletionRate;
            this.DisableInInventory = comp.DisableInInventory;
            this.DontPerishInWorld = comp.DontPerishInWorld;
            this.DontPerishSkipTime = comp.DontPerishSkipTime;
            this.OverrideUpdateRate = comp.OverrideUpdateRate;
        }
    }
}
