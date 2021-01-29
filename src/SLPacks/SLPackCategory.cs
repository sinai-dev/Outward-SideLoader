using System;
using System.Collections;
using System.Collections.Generic;

namespace SideLoader.SLPacks
{
    public abstract class SLPackCategory
    {
        public abstract string FolderName { get; }

        public abstract Type BaseContainedType { get; }

        public abstract int LoadOrder { get; }

        //public virtual bool HasLateContent => false;

        //public virtual void ApplyLateContent(bool isHotReload)
        //    => throw new NotImplementedException("This SLPackCategory does not have late content.");

        protected internal abstract void InternalLoad(List<SLPack> packs, bool isHotReload);

        /// <summary>
        /// Called for all SLPack Categories before a Hot Reload is about to happen.
        /// </summary>
        protected internal abstract void OnHotReload();

        protected void AddToSLPackDictionary(SLPack pack, Dictionary<string, object> dict)
        {
            var ctgType = this.GetType();

            if (!pack.LoadedContent.ContainsKey(ctgType))
                pack.LoadedContent.Add(ctgType, dict);
            else
            {
                foreach (var entry in dict)
                {
                    if (!pack.LoadedContent[ctgType].ContainsKey(entry.Key))
                        pack.LoadedContent[ctgType].Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
