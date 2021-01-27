namespace SideLoader
{
    public class SL_AffectMana : SL_Effect
    {
        public AffectMana.AffectTypes AffectType;
        public float AffectQuantity;
        public bool IsModifier;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AffectMana;

            comp.Value = this.AffectQuantity;
            comp.IsModifier = this.IsModifier;
            comp.AffectType = this.AffectType;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AffectMana;

            AffectQuantity = comp.Value;
            IsModifier = comp.IsModifier;
            AffectType = comp.AffectType;
        }
    }
}
