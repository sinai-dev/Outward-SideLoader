namespace SideLoader
{
    public class SL_WeaponDamageStatusOnKill : SL_WeaponDamage
    {
        public string StatusIdentifier;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            if (!string.IsNullOrEmpty(this.StatusIdentifier) && ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.StatusIdentifier) is StatusEffect status)
                (component as WeaponDamageStatusOnKill).Status = status;
        }

        public override void SerializeEffect<T>(T effect)
        {
            StatusIdentifier = (effect as WeaponDamageStatusOnKill).Status?.IdentifierName;
        }
    }
}
