using SideLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SideLoader.SLPacks
{
    public interface ITemplateCategory
    {
        List<IContentTemplate> CSharpTemplates { get; }

        // implicit
        string FolderName { get; }
        int LoadOrder { get; }
        Type BaseContainedType { get; }
    }

    public abstract class SLPackTemplateCategory<T> : SLPackCategory, ITemplateCategory where T : IContentTemplate
    {
        #region ITemplateCategory

        public List<IContentTemplate> CSharpTemplates => m_registeredCSharpTemplates;

        #endregion

        public override Type BaseContainedType => typeof(T);

        internal static readonly List<IContentTemplate> m_registeredCSharpTemplates = new List<IContentTemplate>();

        internal static readonly List<IContentTemplate> m_pendingLateTemplates = new List<IContentTemplate>();

        public override bool HasLateContent => true;

        public abstract bool ShouldApplyLate(IContentTemplate template);

        public abstract void ApplyTemplate(IContentTemplate template, SLPack pack);

        internal override Dictionary<string, object> InternalLoad(SLPack pack, bool isHotReload)
        {
            var dict = new Dictionary<string, object>();

            var dirPath = pack.GetPathForCategory(this.GetType());

            if (!Directory.Exists(dirPath))
                return dict;

            var list = new List<IContentTemplate>();

            foreach (var filePath in Directory.GetFiles(dirPath, "*.xml"))
                DeserializeTemplate(filePath);

            // check one-level subfolders
            foreach (var subDir in Directory.GetDirectories(dirPath))
            {
                foreach (var filePath in Directory.GetFiles(subDir, "*.xml"))
                    DeserializeTemplate(filePath, Path.GetFileName(subDir));
            }

            if (m_registeredCSharpTemplates != null && m_registeredCSharpTemplates.Any())
                list.AddRange(m_registeredCSharpTemplates);

            list = TemplateDependancySolver.SolveDependencies(list);

            foreach (var template in list)
            {
                if (ShouldApplyLate(template))
                    m_pendingLateTemplates.Add(template);
                else
                    ApplyTemplate(template, pack);
            }

            //m_registeredCSharpTemplates.Clear();

            return dict;

            void DeserializeTemplate(string pathOfFile, string subFolder = null)
            {
                var template = (IContentTemplate)Serializer.LoadFromXml(pathOfFile);

                if (template != null)
                {
                    template.SerializedSLPackName = pack.Name;
                    template.SerializedFilename = Path.GetFileNameWithoutExtension(pathOfFile);

                    if (!string.IsNullOrEmpty(subFolder))
                        template.SerializedSubfolderName = subFolder;

                    dict.Add(pathOfFile, template);
                    list.Add(template);
                }
            }
        }

        public override void ApplyLateContent(SLPack pack, bool isHotReload)
        {
            if (!m_pendingLateTemplates.Any())
                return;

            foreach (var template in m_pendingLateTemplates)
                ApplyTemplate(template, pack);

            m_pendingLateTemplates.Clear();
        }
    }
}