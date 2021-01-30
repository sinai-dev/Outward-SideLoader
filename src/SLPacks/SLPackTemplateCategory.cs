using SideLoader.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SideLoader.SLPacks
{
    public interface ITemplateCategory
    {
        IList Internal_CSharpTemplates { get; }
        IList Internal_AllCurrentTemplates { get; }

        // implicit
        string FolderName { get; }
        int LoadOrder { get; }
        Type BaseContainedType { get; }
    }

    public abstract class SLPackTemplateCategory<T> : SLPackCategory, ITemplateCategory where T : ContentTemplate
    {
        public override Type BaseContainedType => typeof(T);

        public IList Internal_CSharpTemplates => CSharpTemplates;
        public static readonly List<T> CSharpTemplates = new List<T>();

        public IList Internal_AllCurrentTemplates => AllCurrentTemplates;
        public static readonly List<T> AllCurrentTemplates = new List<T>();

        public abstract void ApplyTemplate(ContentTemplate template);

        protected internal override void OnHotReload()
        {
        }

        protected internal override void InternalLoad(List<SLPack> packs, bool isHotReload)
        {
            if (Internal_AllCurrentTemplates.Count < 1)
                Internal_AllCurrentTemplates.Clear();

            var list = new List<ContentTemplate>();

            // Load SL packs first 

            foreach (var pack in packs)
            {
                try
                {
                    var dict = new Dictionary<string, object>();

                    var dirPath = pack.GetPathForCategory(this.GetType());

                    if (!Directory.Exists(dirPath))
                        continue;

                    foreach (var filePath in Directory.GetFiles(dirPath, "*.xml"))
                        DeserializeTemplate(filePath);

                    // check one-level subfolders
                    foreach (var subDir in Directory.GetDirectories(dirPath))
                    {
                        foreach (var filePath in Directory.GetFiles(subDir, "*.xml"))
                            DeserializeTemplate(filePath, Path.GetFileName(subDir));
                    }

                    AddToSLPackDictionary(pack, dict);

                    void DeserializeTemplate(string pathOfFile, string subFolder = null)
                    {
                        var template = (T)Serializer.LoadFromXml(pathOfFile);

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
                catch (Exception ex)
                {
                    SL.LogWarning("Exception loading " + this.FolderName + " from '" + pack.Name + "'");
                    SL.LogInnerException(ex);
                }
            }

            // Load CSharp templates

            if (CSharpTemplates != null && CSharpTemplates.Any())
            {
                SL.Log(CSharpTemplates.Count + " registered C# templates found...");
                list.AddRange(CSharpTemplates);
            }

            list = TemplateDependancySolver.SolveDependencies(list);

            foreach (var template in list)
            {
                try
                {
                    ApplyTemplate(template);

                    Internal_AllCurrentTemplates.Add(template);
                }
                catch (Exception ex)
                {
                    SL.LogWarning("Exception applying template!");
                    SL.LogInnerException(ex);
                }
            }

            return;
        }
    }
}