using HarmonyLib;
using System.Collections.Generic;

namespace SideLoader.Patches
{
    //[HarmonyPatch(typeof(StatusEffect), "UpdateTotalData")]
    //public class StatusEffect_UpdateTotalData
    //{
    //    [HarmonyPrefix]
    //    public static void Prefix(List<StatusData> ___m_statusStack, List<Effect> ___m_effectList)
    //    {
    //        SL.Log($"m_statusStack data len: {___m_statusStack[0].EffectsData.Length}. m_effectList len: {___m_effectList.Count}");
    //    }
    //}

    //[HarmonyPatch(typeof(StatusEffect), "CompileData")]
    //public class StatusEffect_CompileData
    //{
    //    [HarmonyPrefix]
    //    public static void Prefix(StatusEffect __instance, StatusData _info, ref StatusData.EffectData[] ___m_totalData,
    //        List<StatusData> ___m_statusStack, List<Effect> ___m_effectList)
    //    {
    //        if (_info.EffectsData == null || _info.EffectsData.Length != ___m_totalData.Length)
    //        {
    //            SL.Log($"Fixing StatusEffect StatusData for '{__instance.name}'");
    //            SL_StatusEffect.CompileEffectsToData(__instance);
    //            _info.EffectsData = __instance.StatusData.EffectsData;

    //            SL.Log($"m_statusStack len: {___m_statusStack.Count}. m_effectList len: {___m_effectList.Count}");
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(Effect), "OnEnable")]
    //public class Effect_OnEnable
    //{
    //    [HarmonyPrefix]
    //    public static void Prefix(Effect __instance)
    //    {
    //        if (__instance is AddStatusEffect addStatusEffect && addStatusEffect.Status is StatusEffect _old)
    //        {
    //            if (References.RPM_STATUS_EFFECTS.ContainsKey(_old.IdentifierName))
    //            {
    //                if (ResourcesPrefabManager.Instance.GetStatusEffectPrefab(_old.IdentifierName) is StatusEffect _new)
    //                    addStatusEffect.Status = _new;

    //                if (__instance is AddBoonEffect addBoon && addBoon.BoonAmplification is StatusEffect _oldAmp)
    //                {
    //                    if (ResourcesPrefabManager.Instance.GetStatusEffectPrefab(_oldAmp.IdentifierName) is StatusEffect _newAmp)
    //                        addBoon.BoonAmplification = _newAmp;
    //                }
    //            }
    //        }
    //    }
    //}
}
