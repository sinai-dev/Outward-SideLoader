namespace SideLoader
{
    public class SL_AffectHealth : SL_Effect
    {
        public float AffectQuantity;
        public bool IsModifier;
        public float AffectQuantityAI;
        public bool InformSourceCharacter;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AffectHealth;

            comp.AffectQuantity = this.AffectQuantity;
            comp.AffectQuantityOnAI = this.AffectQuantityAI;
            comp.IsModifier = this.IsModifier;
            comp.InformSourceCharacter = this.InformSourceCharacter;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AffectHealth;

            AffectQuantity = comp.AffectQuantity;
            AffectQuantityAI = comp.AffectQuantityOnAI;
            IsModifier = comp.IsModifier;
            InformSourceCharacter = comp.InformSourceCharacter;
        }
    }
}
