using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Armor : SL_Equipment
    {
        public Armor.ArmorClass? Class;
        public EquipmentSoundMaterials? GearSoundMaterial;
        public EquipmentSoundMaterials? ImpactSoundMaterial;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var armor = item as Armor;

            if (this.Class != null)
            {
                armor.Class = (Armor.ArmorClass)this.Class;
            }
            if (this.GearSoundMaterial != null)
            {
                armor.GearSoundMaterial = (EquipmentSoundMaterials)GearSoundMaterial;
            }
            if (this.ImpactSoundMaterial != null)
            {
                armor.ImpactSoundMaterial = (EquipmentSoundMaterials)ImpactSoundMaterial;
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var armor = item as Armor;

            Class = armor.Class;
            GearSoundMaterial = armor.GearSoundMaterial;
            ImpactSoundMaterial = armor.ImpactSoundMaterial;
        }
    }
}
