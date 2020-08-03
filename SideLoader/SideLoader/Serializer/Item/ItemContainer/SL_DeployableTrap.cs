using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            {
                At.SetValue((bool)this.OneTimeUse, typeof(DeployableTrap), trap, "m_oneTimeUse");
            }

            if (this.TrapRecipeEffects != null)
            {
                var list = new List<TrapEffectRecipe>();
                foreach (var holder in this.TrapRecipeEffects)
                {
                    list.Add(holder.Apply());
                }
                At.SetValue(list.ToArray(), typeof(DeployableTrap), trap, "m_trapRecipes");
            }
        }

        public override void SerializeItem(Item item, SL_Item holder)
        {
            base.SerializeItem(item, holder);

            var trap = item as DeployableTrap;

            var template = holder as SL_DeployableTrap;

            template.OneTimeUse = (bool)At.GetValue(typeof(DeployableTrap), trap, "m_oneTimeUse");

            var recipes = (TrapEffectRecipe[])At.GetValue(typeof(DeployableTrap), trap, "m_trapRecipes");
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
