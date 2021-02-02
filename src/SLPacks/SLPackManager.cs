﻿using SideLoader.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SideLoader.SLPacks
{
    public static class SLPackManager
    {
        public enum LoadOrder
        {
            Assets = 0,
            Tags = 5,
            StatusFamily = 10,
            Status = 15,
            Item = 20,
            Recipe = 25,
            IndependantLast = 30,
        }

        //public static event Action<object[]> OnLateApply;
        internal static readonly Dictionary<Action<object[]>, object[]> s_onLateApplyListeners = new Dictionary<Action<object[]>, object[]>();

        /// <summary>
        /// Add a listener to the "late apply" part of SideLoader's setup. Use this to apply references to other prefabs, to ensure that those references
        /// have been set up first when you try to reference them.
        /// </summary>
        /// <param name="listener">Your action which will be invoked during the late apply process.</param>
        /// <param name="args">Your custom arguments of any type, to be passed along to your late apply method.</param>
        public static void AddLateApplyListener(Action<object[]> listener, params object[] args)
        {
            if (listener == null)
            {
                SL.LogWarning("Trying to AddLateApplyListener but the listener is null!");
                return;
            }

            // If we already did our setup (and not hot reloading) just invoke now.
            if (SL.PacksLoaded)
            {
                InvokeLateApplyListener(listener, args);
                return;
            }

            // Else, add the listener.
            s_onLateApplyListeners.Add(listener, args);
        }

        internal static Dictionary<Type, SLPackCategory> s_slPackCategories;
        public static IEnumerable<SLPackCategory> SLPackCategories
        {
            get
            {
                CheckTypeCache();
                return s_slPackCategories.Values;
            }
        }

        public static T GetCategoryInstance<T>() where T : SLPackCategory
            => (T)GetCategoryInstance(typeof(T));

        public static SLPackCategory GetCategoryInstance(Type slPackCategoryType)
        {
            if (slPackCategoryType == null || !typeof(SLPackCategory).IsAssignableFrom(slPackCategoryType))
                throw new ArgumentException("slPackCategoryType either null or not assignable to SLPackCategory.");

            CheckTypeCache();

            s_slPackCategories.TryGetValue(slPackCategoryType, out SLPackCategory ret);
            return ret;
        }

        internal static void LoadAllPacks(bool firstSetup)
        {
            var packs = LoadBaseSLPacks();

            // Normal load order
            foreach (var ctg in SLPackCategories)
                LoadPackCategory(packs, ctg, firstSetup);

            // Invoke late apply listeners, this is what SL uses instead of late apply now.
            if (s_onLateApplyListeners.Any())
            {
                SL.Log("Invoking " + s_onLateApplyListeners.Count + " OnLateApply listeners...");
                foreach (var entry in s_onLateApplyListeners)
                    InvokeLateApplyListener(entry.Key, entry.Value);

                s_onLateApplyListeners.Clear();
            }
        }

        private static void InvokeLateApplyListener(Action<object[]> listener, params object[] args)
        {
            try
            {
                listener.Invoke(args);
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception invoking OnLateApply listener!");
                SL.LogInnerException(ex);
            }
        }

        private static void LoadPackCategory(List<SLPack> packs, SLPackCategory ctg, bool firstSetup)
        {
            try
            {
                ctg.InternalLoad(packs, !firstSetup);
            }
            catch (Exception ex)
            {
                SL.LogWarning($"Exception loading {ctg.FolderName}!");
                SL.LogInnerException(ex);
            }
        }

        private static List<SLPack> LoadBaseSLPacks()
        {
            var packs = new List<SLPack>();

            // 'BepInEx\plugins\...' packs:
            foreach (var dir in Directory.GetDirectories(SL.PLUGINS_FOLDER))
            {
                if (!Directory.Exists($@"{dir}\SideLoader"))
                    continue;

                AddSLPack(Path.GetFileName(dir), false);
            }

            // 'Mods\SideLoader\...' packs:
            foreach (var dir in Directory.GetDirectories(SL.SL_FOLDER))
            {
                if (dir == SL.INTERNAL_FOLDER || dir == SLSaveManager.SAVEDATA_FOLDER || dir.Contains("_GENERATED"))
                    continue;

                AddSLPack(Path.GetFileName(dir), true);
            }

            void AddSLPack(string name, bool inMainFolder)
            {
                var pack = new SLPack
                {
                    InMainSLFolder = inMainFolder,
                    Name = name
                };

                packs.Add(pack);
                SL.s_packs.Add(name, pack);
            }

            return packs;
        }

        private static int s_lastTypeCount = -1;

        public static void CheckTypeCache()
        {
            //SL.Log("Getting implementations of SLPackCategory in all assemblies...");
            var allTypes = At.GetImplementationsOf(typeof(SLPackCategory));

            if (allTypes.Count == s_lastTypeCount)
                return;

            s_lastTypeCount = allTypes.Count;

            var list = new List<SLPackCategory>();
            //var lateList = new List<SLPackCategory>();

            foreach (var type in allTypes)
            {
                try
                {
                    SLPackCategory ctg;
                    if (s_slPackCategories != null && s_slPackCategories.ContainsKey(type))
                        ctg = s_slPackCategories[type];
                    else
                        ctg = (SLPackCategory)At.TryCreateDefault(type);

                    if (ctg != null)
                    {
                        list.Add(ctg);
                        //if (ctg.HasLateContent)
                        //    lateList.Add(ctg);
                    }
                    else
                        SL.Log("SLPack categories internal: could not create instance of type '" + type.FullName + "'");
                }
                catch (Exception ex)
                {
                    SL.LogWarning("Exception checking SLPackCategory '" + type.FullName + "'");
                    SL.LogInnerException(ex);
                }
            }

            s_slPackCategories = new Dictionary<Type, SLPackCategory>();
            foreach (var instance in list.OrderBy(it => it.LoadOrder))
                s_slPackCategories.Add(instance.GetType(), instance);

            //s_slPackLateCategories = new Dictionary<Type, SLPackCategory>();
            //if (lateList.Any())
            //    foreach (var instance in lateList.OrderBy(it => it.LoadOrder))
            //        s_slPackLateCategories.Add(instance.GetType(), instance);
        }
    }
}
