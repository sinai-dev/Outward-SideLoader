using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

namespace SideLoader
{
    public class SL_AddStatusEffectRandom : SL_AddStatusEffect
    {
        public List<string> StatusIdentifiers;
        public int ForceID;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as AddStatusEffectRandom;

            List<StatusEffect> list = new List<StatusEffect>();
            if (this.StatusIdentifiers != null)
            {
                foreach (var name in this.StatusIdentifiers)
                {
                    if (ResourcesPrefabManager.Instance.GetStatusEffectPrefab(name) is StatusEffect status)
                    {
                        list.Add(status);
                    }
                }

                comp.Statuses = list.ToArray();
            }

            comp.ForceID = this.ForceID;

            comp.VfxSystems = comp.transform.root.GetComponentsInChildren<VFXSystem>();
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            var template = holder as SL_AddStatusEffectRandom;
            var comp = effect as AddStatusEffectRandom;

            template.StatusIdentifiers = new List<string>();
            foreach (var status in comp.Statuses)
            {
                template.StatusIdentifiers.Add(status.IdentifierName);
            }
            template.ForceID = comp.ForceID;
        }
    }
}
