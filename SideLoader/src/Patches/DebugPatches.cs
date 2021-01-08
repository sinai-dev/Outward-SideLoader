using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader.Patches
{
    [HarmonyPatch(typeof(DT_SkillProficiencyCheats), nameof(DT_SkillProficiencyCheats.Show))]
    public class DT_SkillProfiencyCheats_Show
    {
        [HarmonyPrefix]
        public static void Prefix(ref List<Skill> ___m_allSkills)
        {
            if (___m_allSkills == null)
                ___m_allSkills = ResourcesPrefabManager.Instance.EDITOR_GetPlayerSkillPrefabs();

            foreach (var skill in SL.s_customSkills)
            {
                if (!___m_allSkills.Contains(skill.Value))
                {
                    //SL.LogWarning("Adding custom skill to F3 menu: " + skill.Value.Name);
                    ___m_allSkills.Add(skill.Value);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(DT_SkillProficiencyCheats __instance, List<DT_SkillDisplayCheat> ___m_skillDisplays,
            DT_SkillDisplayCheat ___m_skillDisplayTemplate, CharacterUI ___m_characterUI, List<int> ___m_knownSkillDisplay,
            List<int> ___m_unknownSkillDisplay, RectTransform ___m_knownSkillHolder, RectTransform ___m_unknownSkillHolder)
        {
            foreach (var skill in SL.s_customSkills)
            {
                if (!skill.Value)
                    continue;

                var query = ___m_skillDisplays.Where(it => it.RefSkill == skill.Value);

                if (!query.Any())
                {
                    //SL.LogWarning("Adding custom skill display to F3 menu: " + skill.Value.Name);

                    var display = UnityEngine.Object.Instantiate(___m_skillDisplayTemplate);
                    display.SetReferencedSkill(skill.Value);
                    display.SetCharacterUI(___m_characterUI);
                    ___m_skillDisplays.Add(display);

                    if (__instance.LocalCharacter.Inventory.SkillKnowledge.IsItemLearned(skill.Value.ItemID))
                    {
                        ___m_knownSkillDisplay.Add(___m_skillDisplays.IndexOf(display));
                        display.transform.SetParent(___m_knownSkillHolder);
                        display.transform.ResetLocal();
                    }
                    else
                    {
                        ___m_unknownSkillDisplay.Add(___m_skillDisplays.IndexOf(display));
                        display.transform.SetParent(___m_unknownSkillHolder);
                        display.transform.ResetLocal();
                    }

                    display.gameObject.SetActive(true);
                }
                else
                {
                    foreach (var result in query)
                        result.gameObject.SetActive(true);
                }
            }
        }
    }
}
