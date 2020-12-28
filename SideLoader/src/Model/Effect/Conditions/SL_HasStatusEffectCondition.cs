using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_HasStatusEffectEffectCondition : SL_EffectCondition
    {
        public float DiseaseAge;
        public bool CheckOwner;

        // StatusSpecific, StatusFamily, StatusType
        public StatusEffectSelector.Types StatusSelectorType;

        public string SelectorValue;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as HasStatusEffectEffectCondition;

            comp.Inverse = this.Invert;
            comp.CheckOwner = this.CheckOwner;
            comp.DiseaseAge = this.DiseaseAge;

            comp.StatusEffect = new StatusEffectSelector()
            {
                Type = this.StatusSelectorType,
            };

            var selector = comp.StatusEffect;
            switch (this.StatusSelectorType)
            {
                case StatusEffectSelector.Types.StatusFamily:
                    selector.StatusFamily = new StatusEffectFamilySelector() { SelectorValue = this.SelectorValue };
                    break;

                case StatusEffectSelector.Types.StatusSpecific:
                    selector.StatusEffect = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.SelectorValue);
                    break;

                case StatusEffectSelector.Types.StatusType:
                    selector.StatusType = new TagSourceSelector(CustomTags.GetTag(this.SelectorValue));
                    break;
            }
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as HasStatusEffectEffectCondition;

            Invert = comp.Inverse;
            DiseaseAge = comp.DiseaseAge;
            CheckOwner = comp.CheckOwner;

            var selector = comp.StatusEffect;

            StatusSelectorType = selector.Type;

            switch (selector.Type)
            {
                case StatusEffectSelector.Types.StatusFamily:
                    SelectorValue = StatusEffectFamilyLibrary.Instance.GetStatusEffect(selector.StatusFamily.SelectorValue).UID;
                    break;

                case StatusEffectSelector.Types.StatusSpecific:
                    SelectorValue = selector.StatusEffect?.IdentifierName;
                    break;

                case StatusEffectSelector.Types.StatusType:
                    SelectorValue = selector.StatusType.Tag.TagName;
                    break;
            }
        }
    }
}
