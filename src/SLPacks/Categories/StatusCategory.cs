using SideLoader.Model;
using SideLoader.Model.Status;
using UnityEngine;

namespace SideLoader.SLPacks.Categories
{
    public class StatusCategory : SLPackTemplateCategory<SL_StatusBase>, ITemplateCategory
    {
        public override string FolderName => "StatusEffects";

        public override int LoadOrder => (int)SLPackManager.LoadOrder.Status;

        public override void ApplyTemplate(ContentTemplate template)
        {
            var status = (SL_StatusBase)template;

            status.ApplyActualTemplate();
        }

        protected internal override void OnHotReload()
        {
            base.OnHotReload();

            foreach (var status in AllCurrentTemplates)
            {
                if (status is SL_StatusEffect statusEffect)
                {
                    if (statusEffect.StatusIdentifier != statusEffect.TargetStatusIdentifier)
                    {
                        if (status.CurrentPrefab)
                            GameObject.Destroy(status.CurrentPrefab);

                        if (References.RPM_STATUS_EFFECTS.ContainsKey(statusEffect.StatusIdentifier))
                            References.RPM_STATUS_EFFECTS.Remove(statusEffect.StatusIdentifier);
                    }
                }
                else if (status is SL_ImbueEffect imbueEffect)
                {
                    if (imbueEffect.NewStatusID != imbueEffect.TargetStatusID)
                    {
                        if (status.CurrentPrefab)
                            GameObject.Destroy(status.CurrentPrefab);

                        if (References.RPM_EFFECT_PRESETS.ContainsKey(imbueEffect.NewStatusID))
                            References.RPM_EFFECT_PRESETS.Remove(imbueEffect.NewStatusID);
                    }
                }
            }
        }
    }
}
