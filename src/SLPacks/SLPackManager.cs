using SideLoader.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SideLoader.SLPacks
{
    public static class SLPackManager
    {
        internal static Dictionary<Type, SLPackCategory> s_slPackCategories;
        public static IEnumerable<SLPackCategory> SLPackCategories
        {
            get
            {
                CheckTypeCache();
                return s_slPackCategories.Values;
            }
        }

        internal static Dictionary<Type, SLPackCategory> s_slPackLateCategories;
        public static IEnumerable<SLPackCategory> SLPackCategoriesWithLateContent
        {
            get
            {
                CheckTypeCache();
                return s_slPackLateCategories.Values;
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
                foreach (var pack in packs)
                    LoadPackCategory(pack, ctg, firstSetup);

            // Late apply
            foreach (var ctg in SLPackCategoriesWithLateContent)
                foreach (var pack in packs)
                    ctg.ApplyLateContent(pack, !firstSetup);
        }

        private static void LoadPackCategory(SLPack pack, SLPackCategory ctg, bool firstSetup)
        {
            // SL.Log("Loading category '" + ctg.ToString() + "' from pack '" + pack.Name + "'");

            try
            {
                var serialized = ctg.InternalLoad(pack, !firstSetup);

                var ctgType = ctg.GetType();

                if (!pack.LoadedContent.ContainsKey(ctgType))
                    pack.LoadedContent.Add(ctgType, serialized);
                else
                {
                    foreach (var entry in serialized)
                    {
                        if (!pack.LoadedContent[ctgType].ContainsKey(entry.Key))
                            pack.LoadedContent[ctgType].Add(entry.Key, entry.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                SL.LogWarning($"Exception loading {ctg.FolderName} from '{pack.Name}'!");
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

                var name = Path.GetFileName(dir);

                var pack = new SLPack()
                {
                    InMainSLFolder = false,
                    Name = name,
                };

                packs.Add(pack);
                SL.Packs.Add(name, pack);
            }

            // 'Mods\SideLoader\...' packs:
            foreach (var dir in Directory.GetDirectories(SL.SL_FOLDER))
            {
                if (dir == SL.INTERNAL_FOLDER || dir == SLSaveManager.SAVEDATA_FOLDER || dir.Contains("_GENERATED"))
                    continue;

                var name = Path.GetFileName(dir);

                var pack = new SLPack
                {
                    InMainSLFolder = true,
                    Name = name
                };

                packs.Add(pack);
                SL.Packs.Add(name, pack);
            }

            return packs;
        }

        private static HashSet<Type> s_lastTypeCache;

        public static void CheckTypeCache()
        {
            //SL.Log("Getting implementations of SLPackCategory in all assemblies...");
            var allTypes = At.GetImplementationsOf(typeof(SLPackCategory));

            if (s_lastTypeCache != null && allTypes.Count == s_lastTypeCache.Count)
                return;

            s_lastTypeCache = allTypes;

            //foreach (var type in allTypes)
            //    SL.Log(type.ToString());

            var list = new List<SLPackCategory>();
            var lateList = new List<SLPackCategory>();

            foreach (var type in allTypes)
            {
                try
                {
                    var dummy = (SLPackCategory)At.TryCreateDefault(type);
                    if (dummy != null)
                    {
                        list.Add(dummy);
                        if (dummy.HasLateContent)
                            lateList.Add(dummy);
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

            s_slPackLateCategories = new Dictionary<Type, SLPackCategory>();
            foreach (var instance in lateList.OrderBy(it => it.LoadOrder))
                s_slPackLateCategories.Add(instance.GetType(), instance);
        }
    }
}
