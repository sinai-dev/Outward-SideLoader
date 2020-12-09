using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_ImbueEffectORCondition : SL_EffectCondition
    {
        public List<int> ImbuePresetIDs;
        public bool AnyImbue;
        public Weapon.WeaponSlot WeaponToCheck;

        public override void ApplyToComponent<T>(T component)
        {
            bool anyValid = false;
            var list = new List<ImbueEffectPreset>();

            foreach (var id in this.ImbuePresetIDs)
            {
                var preset = ResourcesPrefabManager.Instance.GetEffectPreset(id) as ImbueEffectPreset;

                if (preset)
                {
                    anyValid = true;
                    list.Add(preset);
                }
            }

            if (!anyValid && !AnyImbue)
            {
                SL.Log("SL_ImbueEffectORCondition : Could not find any valid Imbue Preset ID!");
                return;
            }

            var comp = component as ImbueEffectORCondition;

            comp.ImbueEffectPresets = list.ToArray();
            comp.AnyImbue = this.AnyImbue;
            comp.WeaponToCheck = this.WeaponToCheck;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var comp = component as ImbueEffectORCondition;
            var holder = template as SL_ImbueEffectORCondition;

            holder.AnyImbue = comp.AnyImbue;
            holder.WeaponToCheck = comp.WeaponToCheck;

            if (comp.ImbueEffectPresets != null)
            {
                holder.ImbuePresetIDs = new List<int>();
                foreach (var imbue in comp.ImbueEffectPresets)
                {
                    holder.ImbuePresetIDs.Add(imbue.PresetID);
                }
            }
        }
    }
}
