using SideLoader.SLPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SideLoader.Model
{
    [SL_Serialized]
    public abstract class ContentTemplate
    {
        // ~~~~ Abstract

        /// <summary>
        /// The corresponding SLPackTemplateCategory for this Template class.
        /// </summary>
        public abstract ITemplateCategory PackCategory { get; }

        /// <summary>
        /// The default name for new templates of this class.
        /// </summary>
        public abstract string DefaultTemplateName { get; }

        /// <summary>
        /// Called to actually apply the template (or for SL_Character/SL_DropTable, just prepares callbacks).
        /// </summary>
        public abstract void ApplyActualTemplate();

        // ~~~~ Virtual

        /// <summary>
        /// Only used when DoesTargetExist returns false. Should point to the TargetID field in the template.
        /// </summary>
        public virtual object TargetID => throw new NotImplementedException($"'{this.GetType()}' did not implement dependency support!" +
                                                                            $"DoesTargetExist returned false, but TargetID was not implemented!");

        /// <summary>
        /// Only used when DoesTargetExist returns false. Should point to the "new / applied" ID field in the template, or otherwise the target.
        /// </summary>                                                           $"DoesTargetExist returned false, but TargetID was not implemented!");
        public virtual object AppliedID => throw new NotImplementedException($"'{this.GetType()}' did not implement dependency support!" +
                                                                            $"DoesTargetExist returned false, but AppliedID was not implemented!");

        /// <summary>
        /// Default return value is true. If the template requires a target object to clone from, this should return whether the TargetID object exists.
        /// </summary>
        public virtual bool DoesTargetExist => true;

        [XmlIgnore] public virtual string SerializedSLPackName { get; set; }
        [XmlIgnore] public virtual string SerializedSubfolderName { get; set; }
        [XmlIgnore] public virtual string SerializedFilename { get; set; }

        /// <summary>
        /// Default is false, true if the template is allowed in a subfolder.
        /// </summary>
        public virtual bool TemplateAllowedInSubfolder => false;

        /// <summary>
        /// True if the template is able to parse content into the template (and vice versa), false if not (default).
        /// </summary>
        public virtual bool CanParseContent => false;

        /// <summary>
        /// Parse the provided content into this template. Only used if CanParseContent is true.
        /// </summary>
        /// <param name="content">The content this template will parse into the template.</param>
        /// <returns>The parsed template.</returns>
        public virtual ContentTemplate ParseToTemplate(object content)
            => throw new NotImplementedException($"'{this.GetType()}' did not implement ParseToTemplate!");

        /// <summary>
        /// Used for cloning if CanParseContent, this would be used by the Target ID to get the content.
        /// </summary>
        public virtual object GetContentFromID(object id)
            => throw new NotImplementedException($"'{this.GetType()}' did not implement GetContentFromID!");

        // ~~~~ Concrete members

        /// <summary>
        /// Call this to Prepare/Apply a C# Template. If SL has already loaded Packs then the template will be applied immediately, 
        /// otherwise it will be applied during SideLoader's setup process.<br/><br/>
        /// For maximum compatibility with other mods, this should be called during the Awake() method of your mod, or at SL.BeforePacksLoaded.
        /// </summary>
        public virtual void ApplyTemplate()
        {
            PackCategory.Internal_CSharpTemplates.Add(this);
            
            if (SL.PacksLoaded)
            { 
                ApplyActualTemplate(); 
                PackCategory.Internal_AllCurrentTemplates.Add(this);
            }
        }
    }
}
