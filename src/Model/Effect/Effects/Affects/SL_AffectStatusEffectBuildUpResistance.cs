namespace SideLoader
{
    public class SL_AffectStatusEffectBuildUpResistance : SL_Effect
    {
        public string StatusEffectIdentifier;
        public float Value;
        public float Duration;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as AffectStatusEffectBuildUpResistance;

            if (!string.IsNullOrEmpty(this.StatusEffectIdentifier)
                && ResourcesPrefabManager.Instance.GetStatusEffectPrefab(StatusEffectIdentifier) is StatusEffect status)
            {
                comp.StatusEffect = status;
            }
            else
                SL.LogWarning("SL_AffectStatusEffectBuildUpResistance: Could not find any StatusEffect with the identifier: " + StatusEffectIdentifier);

            comp.Value = Value;
            comp.Duration = Duration;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AffectStatusEffectBuildUpResistance;

            StatusEffectIdentifier = comp.StatusEffect?.IdentifierName;
            Value = comp.Value;
            Duration = comp.Duration;
        }
    }
}
