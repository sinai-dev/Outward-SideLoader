using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Patches
{
    [HarmonyPatch(typeof(Effect), "OnEnable")]
    public class Effect_OnEnable
    {
        [HarmonyPrefix]
        public static void Prefix(Effect __instance)
        {
            if (__instance is AddStatusEffect addStatusEffect && addStatusEffect.Status is StatusEffect _old)
            {
                if (References.RPM_STATUS_EFFECTS.ContainsKey(_old.IdentifierName))
                {
                    if (ResourcesPrefabManager.Instance.GetStatusEffectPrefab(_old.IdentifierName) is StatusEffect _new)
                        addStatusEffect.Status = _new;

                    if (__instance is AddBoonEffect addBoon && addBoon.BoonAmplification is StatusEffect _oldAmp)
                    {
                        if (ResourcesPrefabManager.Instance.GetStatusEffectPrefab(_oldAmp.IdentifierName) is StatusEffect _newAmp)
                            addBoon.BoonAmplification = _newAmp;
                    }
                }
            }
        }
    }
}
