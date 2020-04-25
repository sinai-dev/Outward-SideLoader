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
            (component as RemoveStatusEffect).CleanseType = this.CleanseType;

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusNameContains)
            {
                (component as RemoveStatusEffect).StatusName = this.Status_Name;
            }

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusSpecific)
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.Status_Name);
                (component as RemoveStatusEffect).StatusEffect = status;
            }

            if (!string.IsNullOrEmpty(this.Status_Tag))
            {
                var tag = CustomItems.GetTag(this.Status_Tag);
                if (tag != null && tag != Tag.None)
                {
                    (component as RemoveStatusEffect).StatusType = new TagSourceSelector(tag);
                }
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_RemoveStatusEffect).Status_Name = (effect as RemoveStatusEffect).StatusName;
            (holder as SL_RemoveStatusEffect).Status_Tag = (effect as RemoveStatusEffect).StatusType?.Tag.TagName;
            (holder as SL_RemoveStatusEffect).CleanseType = (effect as RemoveStatusEffect).CleanseType;

            //if (removeStatusEffect.StatusFamily != null
            //    && StatusEffectFamilyLibrary.Instance.GetStatusEffect(removeStatusEffect.StatusFamily.SelectorValue) is StatusEffectFamily statusFamily)
            //{
            //    removeStatusEffectHolder.StatusEffect_Family = statusFamily.Name;
            //}
        }
    }
}
