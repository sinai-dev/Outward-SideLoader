using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    public class SL_PistolSkill : SL_AttackSkill
    {
        public PistolSkill.LoadoutStateCondition? AlternativeActivationRequirement;
        public PistolSkill.LoadoutStateCondition? PrimaryActivationRequirement;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var comp = item as PistolSkill;

            if (this.AlternativeActivationRequirement != null)
                comp.AlternateActivationLoadoutReq = (PistolSkill.LoadoutStateCondition)this.AlternativeActivationRequirement;

            if (this.PrimaryActivationRequirement != null)
                comp.PrimaryActivationLoadoutReq = (PistolSkill.LoadoutStateCondition)this.PrimaryActivationRequirement;

            if ((bool)At.GetField(comp, "m_isBaseFireReloadSkill") && comp.transform.Find("NormalReload") is Transform reload)
            {
                At.SetField(comp, "m_alternateAnimConditionsHolder", reload.gameObject);

                foreach (var icon in comp.m_alternateIcons)
                    At.SetField(icon, "m_conditionHolder", reload.gameObject);
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var comp = item as PistolSkill;
            var template = holder as SL_PistolSkill;

            template.AlternativeActivationRequirement = comp.AlternateActivationLoadoutReq;
            template.PrimaryActivationRequirement = comp.PrimaryActivationLoadoutReq;
        }
    }
}
