using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader.Helpers;

namespace SideLoader
{
    public class SL_DeployableTrap : SL_Item
    {
        public override bool ShouldApplyLate => true;

        public bool? OneTimeUse;
        public SL_TrapEffectRecipe[] TrapRecipeEffects;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var trap = item as DeployableTrap;

            if (this.OneTimeUse != null)
                At.SetField(trap, "m_oneTimeUse", (bool)this.OneTimeUse);

            if (this.TrapRecipeEffects != null)
            {
                var list = new List<TrapEffectRecipe>();
                foreach (var holder in this.TrapRecipeEffects)
                    list.Add(holder.Apply());
                At.SetField(trap, "m_trapRecipes", list.ToArray());
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var trap = item as DeployableTrap;

            OneTimeUse = (bool)At.GetField(trap, "m_oneTimeUse");

            var recipes = (TrapEffectRecipe[])At.GetField(trap, "m_trapRecipes");
            if (recipes != null)
            {
                var list = new List<SL_TrapEffectRecipe>();
                foreach (var recipe in recipes)
                {
                    var dmRecipe = new SL_TrapEffectRecipe();
                    dmRecipe.Serialize(recipe);
                    list.Add(dmRecipe);
                }
                this.TrapRecipeEffects = list.ToArray();
            }
        }
    }
}
