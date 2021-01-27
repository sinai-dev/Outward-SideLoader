using SideLoader.SLPacks.Categories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SL_LevelAttackSkill : SL_AttackSkill
    {
        public string WatchedStatusIdentifier;
        public SL_SkillStage[] Stages;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var comp = item as LevelAttackSkill;

            if (this.WatchedStatusIdentifier != null)
            {
                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(this.WatchedStatusIdentifier);
                if (status)
                    comp.WatchedStatus = status;
                else
                    SL.LogWarning("SL_LevelAttackSkill: could not find a status by the name of '" + this.WatchedStatusIdentifier + "'");
            }

            var stages = At.GetField(comp, "m_skillStages") as LevelAttackSkill.SkillStage[];

            int newMax = this.Stages?.Length ?? stages.Length;

            if (this.Stages != null)
            {
                if (stages.Length != newMax)
                    Array.Resize(ref stages, newMax);

                for (int i = 0; i < stages.Length; i++)
                {
                    var stage = stages[i];
                    stage.StageDefaultName = Stages[i].Name;
                    stage.StageLocKey = null;
                    stage.StageAnim = Stages[i].Animation;
                    if (!stage.StageIcon)
                        stage.StageIcon = comp.ItemIcon;
                }

            }

            // check for custom level icons
            if (!string.IsNullOrEmpty(SLPackName) && !string.IsNullOrEmpty(SubfolderName) && SL.Packs[SLPackName] is SLPack pack)
            {
                var dir = $@"{pack.GetPathForCategory<ItemCategory>()}\{SubfolderName}\Textures";
                for (int i = 0; i < newMax; i++)
                {
                    var path = dir + $@"\icon{i + 2}.png";

                    if (File.Exists(path))
                    {
                        var tex = CustomTextures.LoadTexture(path, false, false);
                        var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.NONE);

                        stages[i].StageIcon = sprite;
                    }
                }
            }

            At.SetField(comp, "m_skillStages", stages);
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var comp = item as LevelAttackSkill;

            WatchedStatusIdentifier = comp.WatchedStatus?.IdentifierName;

            var stages = (LevelAttackSkill.SkillStage[])At.GetField(comp, "m_skillStages");
            if (stages != null)
            {
                this.Stages = new SL_SkillStage[stages.Length];
                for (int i = 0; i < stages.Length; i++)
                {
                    var stage = stages[i];
                    this.Stages[i] = new SL_SkillStage 
                    {
                        Name = stage.GetName(), 
                        Animation = stage.StageAnim 
                    };
                }
            }
        }
    }

    [SL_Serialized]
    public class SL_SkillStage
    {
        public string Name;
        public Character.SpellCastType Animation;
    }
}
