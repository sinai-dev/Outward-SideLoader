namespace SideLoader
{
    public class SL_UnloadWeapon : SL_Effect
    {
        public Weapon.WeaponSlot WeaponSlot;

        public override void ApplyToComponent<T>(T component)
        {
            (component as UnloadWeapon).WeaponSlot = this.WeaponSlot;
        }

        public override void SerializeEffect<T>(T effect)
        {
            WeaponSlot = (effect as UnloadWeapon).WeaponSlot;
        }
    }
}
