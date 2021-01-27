namespace SideLoader
{
    public class SL_RemoveImbueEffects : SL_Effect
    {
        public Weapon.WeaponSlot AffectSlot;

        public override void ApplyToComponent<T>(T component)
        {
            (component as RemoveImbueEffects).AffectSlot = this.AffectSlot;
        }

        public override void SerializeEffect<T>(T effect)
        {
            AffectSlot = (effect as RemoveImbueEffects).AffectSlot;
        }
    }
}
