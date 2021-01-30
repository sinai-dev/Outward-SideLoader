namespace SideLoader
{
    public class SL_LeechStatus : SL_StatusEffect
    {
        public float? LeechRatio;
        public float? LeechFXRatio;

        internal override void Internal_ApplyTemplate(StatusEffect status)
        {
            base.Internal_ApplyTemplate(status);

            var comp = status as LeechStatus;

            comp.LeechFXRatio = (float)this.LeechFXRatio;
            comp.LeechRatio = (float)this.LeechRatio;
        }

        public override void SerializeStatus(StatusEffect status)
        {
            base.SerializeStatus(status);

            var comp = status as LeechStatus;

            this.LeechRatio = comp.LeechRatio;
            this.LeechFXRatio = comp.LeechFXRatio;
        }
    }
}
