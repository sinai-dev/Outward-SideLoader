using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SideLoader
{
    public class SL_LevelStatusEffect : SL_StatusEffect
    {
        public int? MaxLevel;

        internal override void ApplyTemplate(StatusEffect status)
        {
            base.ApplyTemplate(status);

            var comp = status as LevelStatusEffect;

            int origMax = (int)At.GetField(comp, "m_maxLevel");
            int newMax = MaxLevel ?? origMax;

            At.SetField(comp, "m_maxLevel", newMax);

            Sprite[] origIcons = new Sprite[origMax - 1];

            // prepare the level data array and get current icons
            if (comp.StatusLevelData == null)
                comp.StatusLevelData = new LevelStatusEffect.LevelData[newMax - 1];
            else if (comp.StatusLevelData.Length > 0)
            {
                comp.StatusLevelData = comp.StatusLevelData.OrderBy(it => it.LevelIndex).ToArray();

                for (int i = 0; i < origMax - 1; i++)
                    origIcons[i] = comp.StatusLevelData[i].Icon;
            }

            if (origMax != newMax)
                Array.Resize(ref comp.StatusLevelData, newMax - 1);

            // set the level datas
            for (int i = 0; i < newMax - 1; i++)
            {
                if (comp.StatusLevelData[i] == null)
                    comp.StatusLevelData[i] = new LevelStatusEffect.LevelData();

                var level = comp.StatusLevelData[i];
                level.LevelIndex = i;
                level.StatusData = new StatusData(comp.StatusData)
                {
                    EffectsData = GenerateEffectsData(comp.StatusEffectSignature.Effects, i + 2)
                };
            }

            // check for custom level icons
            if (!string.IsNullOrEmpty(SLPackName) && !string.IsNullOrEmpty(SubfolderName) && SL.Packs[SLPackName] is SLPack pack)
            {
                var dir = $@"{pack.GetSubfolderPath(SLPack.SubFolders.StatusEffects)}\{SubfolderName}";
                for (int i = 0; i < newMax - 1; i++)
                {
                    var path = dir + $@"\icon{i + 2}.png";

                    if (File.Exists(path))
                    {
                        var tex = CustomTextures.LoadTexture(path, false, false);
                        var sprite = CustomTextures.CreateSprite(tex, CustomTextures.SpriteBorderTypes.NONE);

                        //list.Add(sprite);
                        comp.StatusLevelData[i].Icon = sprite;
                    }
                }
            }

            // ensure all levels at least have some icon
            for (int i = 0; i < newMax - 1; i++)
            {
                if (!comp.StatusLevelData[i].Icon)
                    comp.StatusLevelData[i].Icon = comp.StatusIcon;
            }
        }

        public override void SerializeStatus(StatusEffect status)
        {
            base.SerializeStatus(status);

            this.MaxLevel = (int)At.GetField(status as LevelStatusEffect, "m_maxLevel");
        }
    }
}
