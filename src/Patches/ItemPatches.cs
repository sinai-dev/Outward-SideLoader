using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SideLoader.Patches
{
    #region Start

    [HarmonyPatch(typeof(Item), "Start")]
    public class Item_Start
    {
        public static void Postfix(Item __instance, ref string ___m_toLogString)
        {
            ___m_toLogString = Serializer.ReplaceInvalidChars(___m_toLogString);

            if (!SL.PacksLoaded)
                return;

            if (SL_Item.s_initCallbacks.TryGetValue(__instance.ItemID, out List<Action<Item>> invocationList))
            {
                foreach (var listener in invocationList)
                {
                    try
                    {
                        listener.Invoke(__instance);
                    }
                    catch (Exception e)
                    {
                        SL.LogWarning($"Exception invoking callback for Item.Start() on {__instance.Name}");
                        SL.LogInnerException(e);
                    }
                }
            }
        }
    }

    #endregion

    #region Localization

    [HarmonyPatch(typeof(LocalizationManager), "StartLoading")]
    public class LocalizationManager_StartLoading
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (CustomItems.s_customLocalizations.Any())
            {
                for (int i = CustomItems.s_customLocalizations.Count - 1; i >= 0; i--)
                {
                    var entry = CustomItems.s_customLocalizations.ElementAt(i);

                    if (!(ResourcesPrefabManager.Instance.GetItemPrefab(entry.Key) is Item item))
                        continue;

                    CustomItems.SetNameAndDescription(item, entry.Value.Name, entry.Value.Desc);
                }
            }
        }
    }

    #endregion

    #region Item Visuals

    [HarmonyPatch(typeof(Item), "GetItemVisual", new Type[] { typeof(bool) })]
    public class Item_GetItemVisuals
    {
        [HarmonyPrefix]
        public static bool Prefix(Item __instance, bool _special, ref Transform __result)
        {
            try
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
            catch (Exception e)
            {
                SL.LogInnerException(e);
                return true;
            }
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

    #region Bugfixes

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
}
