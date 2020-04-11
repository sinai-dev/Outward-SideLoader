using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_RemoveStatusEffect : SL_Effect
    {
        //public string StatusEffect;
        public string Status_Name = "";
        //public string StatusEffect_Family;
        public string Status_Tag = "";

        public RemoveStatusEffect.RemoveTypes CleanseType;

        public new void ApplyToTransform(Transform t)
        {
            var component = t.gameObject.AddComponent<RemoveStatusEffect>();

            component.CleanseType = this.CleanseType;
            
            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusNameContains)
            {
                component.StatusName = this.Status_Name;
            }

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusSpecific)
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.Status_Name);
                component.StatusEffect = status;
            }

            if (!string.IsNullOrEmpty(this.Status_Tag))
            {
                var tag = CustomItems.GetTag(this.Status_Tag);
                if (tag != null && tag != Tag.None)
                {
                    component.StatusType = new TagSourceSelector(tag);
                }
            }
        }

        public static SL_RemoveStatusEffect ParseRemoveStatusEffect(RemoveStatusEffect removeStatusEffect, SL_Effect _effectHolder)
        {
            var removeStatusEffectHolder = new SL_RemoveStatusEffect
            {
               //StatusEffect = removeStatusEffect.StatusEffect ? removeStatusEffect.StatusEffect.IdentifierName : null,
               Status_Tag = removeStatusEffect.StatusName,
               Status_Name = removeStatusEffect.StatusType.Tag.TagName,
               CleanseType = removeStatusEffect.CleanseType
            };

            At.InheritBaseValues(removeStatusEffectHolder, _effectHolder);

            //if (removeStatusEffect.StatusFamily != null
            //    && StatusEffectFamilyLibrary.Instance.GetStatusEffect(removeStatusEffect.StatusFamily.SelectorValue) is StatusEffectFamily statusFamily)
            //{
            //    removeStatusEffectHolder.StatusEffect_Family = statusFamily.Name;
            //}

            return removeStatusEffectHolder;
        }
    }
}
