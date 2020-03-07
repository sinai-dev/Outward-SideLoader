using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dataminer
{
    public class TrapEffectHolder
    {
        public string Name;
        public string Description;

        public List<string> CompatibleItems = new List<string>();
        public List<string> CompatibleTags = new List<string>();

        public List<EffectHolder> NormalEffects = new List<EffectHolder>();
        public List<EffectHolder> ImprovedEffects = new List<EffectHolder>();

        public static TrapEffectHolder ParseTrapEffect(TrapEffectRecipe recipe)
        {
            var trapEffectHolder = new TrapEffectHolder
            {
                Name = recipe.Name,
                Description = recipe.Description
            };

            if (recipe.CompatibleTags != null)
            {
                foreach (TagSourceSelector tag in recipe.CompatibleTags)
                {
                    trapEffectHolder.CompatibleTags.Add(tag.Tag.TagName);
                }
            }

            if (At.GetValue(typeof(TrapEffectRecipe), recipe, "m_compatibleItems") is Item[] items)
            {
                foreach (Item item in items)
                {
                    trapEffectHolder.CompatibleItems.Add(item.Name);
                }
            }

            if (recipe.TrapEffectsPrefab is Transform effectsPrefab)
            {
                foreach (Effect effect in effectsPrefab.GetComponentsInChildren<Effect>())
                {
                    var effectHolder = EffectHolder.ParseEffect(effect);
                    trapEffectHolder.NormalEffects.Add(effectHolder);
                }
            }

            if (recipe.HiddenTrapEffectsPrefab is Transform hiddenPrefab)
            {
                foreach (Effect effect in hiddenPrefab.GetComponentsInChildren<Effect>())
                {
                    var effectHolder = EffectHolder.ParseEffect(effect);
                    trapEffectHolder.ImprovedEffects.Add(effectHolder);
                }
            }

            return trapEffectHolder;
        }
    }
}
