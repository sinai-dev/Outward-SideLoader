namespace SideLoader
{
    public class SL_AffectStamina : SL_Effect
    {
        public float AffectQuantity;

        public override void ApplyToComponent<T>(T component)
        {
            (component as AffectStamina).AffectQuantity = this.AffectQuantity;
        }

        public override void SerializeEffect<T>(T effect)
        {
            AffectQuantity = (effect as AffectStamina).AffectQuantity;
        }
    }
}
