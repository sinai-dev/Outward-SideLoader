using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_RunicBlade : SL_Effect
    {
        public int WeaponID;
        public int GreaterWeaponID;

        public int PrefixImbueID;
        public int PrefixGreaterImbueID;

        public float SummonLifespan;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as RunicBlade;

            comp.SummonLifeSpan = this.SummonLifespan;

            if (ResourcesPrefabManager.Instance.GetItemPrefab(this.WeaponID) is Weapon weapon)
            {
                comp.RunicBladePrefab = weapon;
            }
            else
            {
                SL.Log("SL_RunicBlade: Could not get an Item with ID '" + WeaponID + "'!");
                return;
            }

            if (ResourcesPrefabManager.Instance.GetItemPrefab(this.GreaterWeaponID) is Weapon greaterWeapon)
            {
                comp.RunicGreatBladePrefab = greaterWeapon;
            }
            else
            {
                SL.Log("SL_RunicBlade: Could not get an Item with ID '" + GreaterWeaponID + "'!");
                return;
            }

            if (ResourcesPrefabManager.Instance.GetEffectPreset(PrefixImbueID) is ImbueEffectPreset imbue)
            {
                comp.ImbueAmplifierRunicBlade = imbue;
            }
            else
            {
                SL.Log("SL_RunicBlade: Could not get an imbue with the ID '" + PrefixImbueID + "'!");
                return;
            }

            if (ResourcesPrefabManager.Instance.GetEffectPreset(PrefixGreaterImbueID) is ImbueEffectPreset greatImbue)
            {
                comp.ImbueAmplifierGreatRunicBlade = greatImbue;
            }
            else
            {
                SL.Log("SL_RunicBlade: Could not get an imbue with the ID '" + PrefixGreaterImbueID + "'!");
                return;
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var template = holder as SL_RunicBlade;
            var comp = effect as RunicBlade;

            template.SummonLifespan = comp.SummonLifeSpan;
            template.WeaponID = comp.RunicBladePrefab.ItemID;
            template.GreaterWeaponID = comp.RunicGreatBladePrefab.ItemID;
            template.PrefixImbueID = comp.ImbueAmplifierRunicBlade.PresetID;
            template.PrefixGreaterImbueID = comp.ImbueAmplifierGreatRunicBlade.PresetID;
        }
    }
}
