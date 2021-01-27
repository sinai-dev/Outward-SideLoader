namespace SideLoader
{
    public class SL_HasStatusLevelCondition : SL_EffectCondition
    {
        public string StatusIdentifier;
        public int CompareLevel;
        public bool CheckOwner;
        public AICondition.NumericCompare ComparisonType;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as HasStatusLevelCondition;

            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusIdentifier);

            if (!status)
            {
                SL.Log("SL_HasStatusEffectLevelCondition: Could not find a Status Effect with the identifier '" + this.StatusIdentifier + "'!");
                return;
            }

            comp.StatusEffect = status;
            comp.CheckOwner = this.CheckOwner;
            comp.CompareLevel = this.CompareLevel;
            comp.ComparaisonType = this.ComparisonType;
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as HasStatusLevelCondition;

            StatusIdentifier = comp.StatusEffect?.IdentifierName;
            CheckOwner = comp.CheckOwner;
            CompareLevel = comp.CompareLevel;
            ComparisonType = comp.ComparaisonType;
        }
    }
}
