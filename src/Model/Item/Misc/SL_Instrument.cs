using UnityEngine;

namespace SideLoader
{
    public class SL_Instrument : SL_Item
    {
        public float? PeriodicTime;
        public Vector2? PulseSpeed;
        public float? StrikeTime;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var comp = item as Instrument;

            if (this.PeriodicTime != null)
                comp.PeriodicTime = (float)this.PeriodicTime;

            if (this.PulseSpeed != null)
                comp.PulseSpeed = (Vector2)this.PulseSpeed;

            if (this.StrikeTime != null)
                comp.StrikeTime = (float)this.StrikeTime;
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var comp = item as Instrument;

            PeriodicTime = comp.PeriodicTime;
            PulseSpeed = comp.PulseSpeed;
            StrikeTime = comp.StrikeTime;
        }
    }
}
