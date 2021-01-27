namespace SideLoader
{
    public class SL_AddStatusEffectBuildUp : SL_Effect
    {
        /// <summary>
        /// Must use a Status Identifier, not the actual name of the status effect.
        /// </summary>
        public string StatusEffect = "";
        /// <summary>
        /// The effect build-up value, between 0 and 100.
        /// </summary>
        public float Buildup;

        public float BuildUpMultiplier = 1.0f;
        public bool BypassCounter;
        public bool AffectController;

        public override void ApplyToComponent<T>(T component)
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusEffect);

            if (!status)
            {
                SL.LogWarning($"{this.GetType().Name}: Could not find any effect with the identifier '{this.StatusEffect}'");
                return;
            }

            var comp = component as AddStatusEffectBuildUp;

            comp.Status = status;
            comp.BuildUpValue = this.Buildup;
            comp.BypassCounter = this.BypassCounter;
            comp.BuildUpMultiplier = this.BuildUpMultiplier;
            comp.AffectController = this.AffectController;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as AddStatusEffectBuildUp;

            if (comp.Status)
            {
                StatusEffect = comp.Status.IdentifierName;
                Buildup = comp.BuildUpValue;
                BuildUpMultiplier = comp.BuildUpMultiplier;
                BypassCounter = comp.BypassCounter;
                this.AffectController = comp.AffectController;
            }
        }
    }
}
