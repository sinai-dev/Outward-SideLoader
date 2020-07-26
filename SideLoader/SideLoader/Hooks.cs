using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SideLoader.Hooks
{
    // All HarmonyPatches used by SideLoader are in this file.

    #region CORE SETUP PATCH

    /// <summary>
    /// SideLoader's setup is a Finalizer on ResourcesPrefabManager.Load().
    /// </summary>
    [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
    public class ResourcesPrefabManager_Load
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                SL.Log("Exception on ResourcesPrefabManager.Load!", 0);
                SL.Log(__exception.Message, 0);
                SL.Log(__exception.StackTrace, 0);
            }

            SL.Setup();

            return null;
        }
    }

    #endregion

    #region SL_ITEM PATCHES

    // fix for the recipe menu, which can break from some custom items when they are an ingredient.
    [HarmonyPatch(typeof(ItemListDisplay), "SortBySupport")]
    public class ItemListDisplay_SortBySupport
    {
        [HarmonyFinalizer]
        public static Exception Finalizer(ref int __result, Exception __exception)
        {
            if (__exception != null)
            {
                __result = -1;
            }
            return null;
        }
    }

    // fix for ItemDetailsDisplay. Shouldn't really be needed anymore, but leaving it for now.
    [HarmonyPatch(typeof(ItemDetailsDisplay), "RefreshDetail")]
    public class ItemDetailsDisplay_RefreshDetail
    {
        [HarmonyFinalizer]
        public static Exception Finalizer()
        {
            return null;
        }
    }

    #endregion

    #region SL_ITEM VISUALS PATCHES

    [HarmonyPatch(typeof(Item), "GetItemVisual", new Type[] { typeof(bool) })]
    public class Item_GetItemVisuals
    {
        [HarmonyPrefix]
        public static bool Prefix(Item __instance, bool _special, ref Transform __result)
        {
            if (CustomItemVisuals.GetItemVisualLink(__instance) is CustomItemVisuals.ItemVisualsLink link)
            {
                if (!_special)
                {
                    if (link.ItemVisuals)
                    {
                        __result = link.ItemVisuals;
                        return false;
                    }
                }
                else
                {
                    if (__instance.UseSpecialVisualFemale)
                    {
                        if (link.ItemSpecialFemaleVisuals)
                        {
                            __result = link.ItemSpecialFemaleVisuals;
                            return false;
                        }
                    }
                    else if (link.ItemSpecialVisuals)
                    {
                        __result = link.ItemSpecialVisuals;
                        return false;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Item), "ItemIcon", MethodType.Getter)]
    public class Item_ItemIcon
    {
        [HarmonyPrefix]
        public static bool Prefix(Item __instance, ref Sprite __result)
        {
            if (CustomItemVisuals.GetItemVisualLink(__instance) is CustomItemVisuals.ItemVisualsLink link
                && link.ItemIcon)
            {
                __result = link.ItemIcon;
                return false;
            }

            return true;
        }
    }

    #endregion

    #region SL_STATUSEFFECT PATCHES

    /// <summary>
    /// This patch is to fix StatusEffect prefab references on AddStatus components, for when editing an existing StatusEffect.
    /// </summary>
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

    #endregion

    #region SL_SHOOTER PATCHES

    /// <summary>
    /// Patch to re-enable the disabled blast/projectile prefabs before they are used. 
    /// We need to disable them when we clone them, but here they need to be activated again.
    /// </summary>
    [HarmonyPatch(typeof(Shooter), "Setup", new Type[] { typeof(TargetingSystem), typeof(Transform) })]
    public class ShootProjectile_Setup
    {
        [HarmonyPrefix]
        public static void Prefix(Shooter __instance)
        {
            if (__instance is ShootProjectile shootProjectile && shootProjectile.BaseProjectile is Projectile projectile && !projectile.gameObject.activeSelf)
            {
                projectile.gameObject.SetActive(true);
                EnableEffects(projectile.gameObject);

            }
            else if (__instance is ShootBlast shootBlast && shootBlast.BaseBlast is Blast blast && !blast.gameObject.activeSelf)
            {
                blast.gameObject.SetActive(true);
                EnableEffects(blast.gameObject);
            }
        }

        private static void EnableEffects(GameObject obj)
        {
            foreach (var effect in obj.GetComponentsInChildren<Effect>(true))
            {
                if (!effect.enabled)
                {
                    effect.enabled = true;
                }
            }
            foreach (var condition in obj.GetComponentsInChildren<EffectCondition>(true))
            {
                if (!condition.enabled)
                {
                    condition.enabled = true;
                }
            }
        }
    }

    #endregion

    #region SL_CHARACTER PATCHES

    // Just catches a harmless null ref exception, hiding it until I figure out a cleaner fix
    [HarmonyPatch(typeof(Character), "ProcessOnEnable")]
    public class Character_ProcessOnEnable
    {
        [HarmonyFinalizer]
        public static Exception Finalizer()
        {
            return null;
        }
    }

    // This harmony patch is to sneak into when the game applies characters.
    // I figure it's best to do it at the same time.
    [HarmonyPatch(typeof(NetworkLevelLoader), "MidLoadLevel")]
    public class NetworkLevelLoader_MidLoadLevel
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            CustomCharacters.InvokeSpawnCharacters();
        }
    }

    // Like the last patch, we sneak into when the game should have destroyed previous scene characters to cleanup there.
    [HarmonyPatch(typeof(CharacterManager), "ClearNonPersitentCharacters")]
    public class CharacterManager_ClearNonPersitentCharacters
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            CustomCharacters.CleanupCharacters();
        }
    }

    #endregion
}
