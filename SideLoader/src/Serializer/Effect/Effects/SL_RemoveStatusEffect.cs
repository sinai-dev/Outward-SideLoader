using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_RemoveStatusEffect : SL_Effect
    {
        public string Status_Name = "";
        public string Status_Tag = "";
        public RemoveStatusEffect.RemoveTypes CleanseType;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as RemoveStatusEffect;

            comp.CleanseType = this.CleanseType;

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusNameContains)
            {
                comp.StatusName = this.Status_Name;
            }

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusSpecific)
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.Status_Name);
                comp.StatusEffect = status;
            }

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusType && !string.IsNullOrEmpty(this.Status_Tag))
            {
                var tag = CustomItems.GetTag(this.Status_Tag);
                if (tag != Tag.None)
                {
                    comp.StatusType = new TagSourceSelector(tag);
                }
            }

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusFamily)
            {
                var family = StatusEffectFamilyLibrary.Instance.GetStatusEffect(this.Status_Name);
                if (family != null)
                {
                    comp.StatusFamily = new StatusEffectFamilySelector()
                    {
                        SelectorValue = family.UID,
                    };
                }
            }
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as RemoveStatusEffect;

            CleanseType = comp.CleanseType;

            switch (CleanseType)
            {
                case RemoveStatusEffect.RemoveTypes.StatusSpecific:
                    if (comp.StatusEffect)
                        Status_Name = comp.StatusEffect.IdentifierName;
                    break;
                case RemoveStatusEffect.RemoveTypes.StatusFamily:
                        Status_Name = comp.StatusFamily?.SelectorValue;
                    break;
                case RemoveStatusEffect.RemoveTypes.StatusType:
                    Status_Name = comp.StatusType?.Tag.TagName;
                    break;
                case RemoveStatusEffect.RemoveTypes.StatusNameContains:
                    Status_Name = comp.StatusName;
                    break;
            }
        }
    }
}
