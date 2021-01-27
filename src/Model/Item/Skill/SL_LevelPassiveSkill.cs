namespace SideLoader
{
    public class SL_LevelPassiveSkill : SL_Skill
    {
        public string WatchedStatusIdentifier;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            if (this.WatchedStatusIdentifier != null)
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.WatchedStatusIdentifier);
                if (status)
                    (item as LevelPassiveSkill).WatchedStatusEffect = status;
                else
                    SL.LogWarning("SL_LevelPassiveSkill - could not find any status with identifier '" + this.WatchedStatusIdentifier + "'");
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            this.WatchedStatusIdentifier = (item as LevelPassiveSkill).WatchedStatusEffect?.IdentifierName;
        }
    }
}
