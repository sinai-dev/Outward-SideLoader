using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class CustomTags
    {
        /// <summary>
        /// Returns the game's actual Tag for the string you provide, if it exists.
        /// </summary>
        /// <param name="TagName">Eg "Food", "Blade", etc...</param>
        /// <param name="logging">Whether to log error messages to debug console or not (if tag doesnt exist)</param>
        /// <returns></returns>
        public static Tag GetTag(string TagName, bool logging = true)
        {
            var tags = (Tag[])At.GetValue(typeof(TagSourceManager), TagSourceManager.Instance, "m_tags");
            var tag = tags.FirstOrDefault(x => x.TagName == TagName);
            if (tag.TagName == TagName)
            {
                return tag;
            }
            else
            {
                if (logging)
                {
                    SL.Log("GetTag :: Could not find a tag by the name: " + TagName);
                }
                return Tag.None;
            }
        }

        /// <summary>
        /// Helper for creating a new Tag
        /// </summary>
        /// <param name="name">The new tag name</param>
        public static Tag CreateTag(string name)
        {
            if (GetTag(name, false) is Tag tag && tag.TagName == name)
            {
                SL.Log($"Error: A tag already exists called '{name}'");
            }
            else
            {
                tag = new Tag(TagSourceManager.TagRoot, name);
                tag.SetTagType(Tag.TagTypes.Custom);

                TagSourceManager.Instance.DbTags.Add(tag);
                TagSourceManager.Instance.RefreshTags(true);

                SL.Log($"Created a tag, name: {tag.TagName}");
            }

            return tag;
        }
    }
}
