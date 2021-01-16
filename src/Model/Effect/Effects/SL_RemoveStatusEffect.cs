﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_RemoveStatusEffect : SL_Effect
    {
        public RemoveStatusEffect.RemoveTypes CleanseType;
        public string SelectorValue;

        /// <summary>[OBSOLETE] Use SelectorValue instead.</summary>
        public string Status_Name;
        /// <summary>[OBSOLETE] Use SelectorValue instead.</summary>
        public string Status_Tag;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as RemoveStatusEffect;

            if (!string.IsNullOrEmpty(this.Status_Name))
                this.SelectorValue = this.Status_Name;
            else if (!string.IsNullOrEmpty(this.Status_Tag))
                this.SelectorValue = this.Status_Tag;

            comp.CleanseType = this.CleanseType;

            if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusNameContains)
            {
                comp.StatusName = this.SelectorValue;
            }
            else if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusSpecific)
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.SelectorValue);
                comp.StatusEffect = status;
            }
            else if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusType && !string.IsNullOrEmpty(this.SelectorValue))
            {
                var tag = CustomItems.GetTag(this.SelectorValue);
                if (tag != Tag.None)
                {
                    comp.StatusType = new TagSourceSelector(tag);
                }
            }
            else if (this.CleanseType == RemoveStatusEffect.RemoveTypes.StatusFamily)
            {
                var family = StatusEffectFamilyLibrary.Instance.GetStatusEffect(this.SelectorValue);
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
                        SelectorValue = comp.StatusEffect.IdentifierName;
                    break;
                case RemoveStatusEffect.RemoveTypes.StatusFamily:
                    SelectorValue = comp.StatusFamily?.SelectorValue;
                    break;
                case RemoveStatusEffect.RemoveTypes.StatusType:
                    SelectorValue = comp.StatusType?.Tag.TagName;
                    break;
                case RemoveStatusEffect.RemoveTypes.StatusNameContains:
                    SelectorValue = comp.StatusName;
                    break;
            }
        }
    }
}
