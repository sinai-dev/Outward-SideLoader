using SideLoader.Model;
using SideLoader.SLPacks;
using SideLoader.SLPacks.Categories;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_StatusEffectFamily : IContentTemplate
    {
        #region IContentTemplate

        public object TargetID => this.UID;
        public object AppliedID => this.UID;
        public bool IsCreatingNewID => true;
        public bool DoesTargetExist => true;
        public ITemplateCategory PackCategory => SLPackManager.GetCategoryInstance<StatusFamilyCategory>();
        public bool TemplateAllowedInSubfolder => false;
        public string DefaultTemplateName => this.UID;

        [XmlIgnore] public bool CanParseContent => false;
        public IContentTemplate ParseToTemplate(object _) => throw new NotImplementedException();
        public object GetContentFromID(object id)
        {
            return StatusEffectFamilyLibrary.Instance.StatusEffectFamilies.FirstOrDefault(it => it.UID == (string)id);
        }

        [XmlIgnore] public string SerializedSLPackName { get; set; }
        [XmlIgnore] public string SerializedSubfolderName { get; set; }
        [XmlIgnore] public string SerializedFilename { get; set; }

        public void ApplyActualTemplate() => CreateFamily();

        #endregion

        public string UID;
        public string Name;

        public StatusEffectFamily.StackBehaviors? StackBehaviour;
        public int? MaxStackCount = -1;

        public StatusEffectFamily.LengthTypes? LengthType;

        /// <summary>
        /// Call this to register the template and create the status family.
        /// </summary>
        public void Apply()
        {
            CreateFamily();
        }

        internal static SL_StatusEffectFamily ParseEffectFamily(StatusEffectFamily family)
        {
            return new SL_StatusEffectFamily
            {
                UID = family.UID,
                Name = family.Name,
                LengthType = family.LengthType,
                MaxStackCount = family.MaxStackCount,
                StackBehaviour = family.StackBehavior
            };
        }

        internal StatusEffectFamily CreateFamily()
        {
            var ret = new StatusEffectFamily
            {
                Name = this.Name,
                LengthType = (StatusEffectFamily.LengthTypes)this.LengthType,
                MaxStackCount = (int)this.MaxStackCount,
                StackBehavior = (StatusEffectFamily.StackBehaviors)this.StackBehaviour
            };

            At.SetField(ret, "m_uid", new UID(this.UID));

            return ret;
        }

        internal void ApplyTemplate()
        {
            if (!StatusEffectFamilyLibrary.Instance)
                return;

            var library = StatusEffectFamilyLibrary.Instance;

            StatusEffectFamily family;
            if (library.StatusEffectFamilies.Where(it => (string)it.UID == this.UID).Any())
            {
                family = library.StatusEffectFamilies.First(it => (string)it.UID == this.UID);
            }
            else
            {
                family = new StatusEffectFamily();
                library.StatusEffectFamilies.Add(family);
            }

            if (family == null)
            {
                SL.LogWarning("Applying SL_StatusEffectFamily template, null error");
                return;
            }

            if (this.UID != null)
                At.SetField(family, "m_uid", new UID(this.UID));

            if (this.Name != null)
                family.Name = this.Name;

            if (this.StackBehaviour != null)
                family.StackBehavior = (StatusEffectFamily.StackBehaviors)this.StackBehaviour;

            if (this.MaxStackCount != null)
                family.MaxStackCount = (int)this.MaxStackCount;

            if (this.LengthType != null)
                family.LengthType = (StatusEffectFamily.LengthTypes)this.LengthType;
        }
    }
}
