using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using UnityEngine.SceneManagement;
using SideLoader.Helpers;

namespace SideLoader
{
    /// <summary>
    /// Attribute used to mark a type that needs to be serialized by the Serializer.
    /// Usage is to just put [SL_Serialized] on a base class. Derived classes will inherit it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SL_Serialized : Attribute { }

    /// <summary>
    /// Sideloader's serializer. Handles Xml serialization and deserialization for SideLoader's custom types.
    /// </summary>
    public class Serializer
    {
        /// <summary>
        /// SideLoader.dll AppDomain reference.
        /// </summary>
        public static Assembly SL_Assembly => m_slAssembly ?? (m_slAssembly = Assembly.GetExecutingAssembly());
        private static Assembly m_slAssembly;

        /// <summary>
        /// The Assembly-Csharp.dll AppDomain reference.
        /// </summary>
        public static Assembly Game_Assembly => m_gameAssembly ?? (m_gameAssembly = typeof(Item).Assembly);
        private static Assembly m_gameAssembly;

        /// <summary>
        /// List of SL_Type classes (types marked as SL_Serialized).
        /// </summary>
        public static Type[] SLTypes => m_slTypes ?? GetSLTypes();
        private static Type[] m_slTypes;

        private static readonly Dictionary<Type, XmlSerializer> m_xmlCache = new Dictionary<Type, XmlSerializer>();

        /// <summary>
        /// Use this to get and cache an XmlSerializer for the provided Type, this will include all SL_Types as the extraTypes.
        /// </summary>
        /// <param name="type">The root type of the document</param>
        /// <returns>The new (or cached) XmlSerializer</returns>
        public static XmlSerializer GetXmlSerializer(Type type)
        {
            if (!m_xmlCache.ContainsKey(type))
            {
                m_xmlCache.Add(type, new XmlSerializer(type, SLTypes));
            }

            return m_xmlCache[type];
        }

        // Cached results from GetGameType or GetSLType. Can use same dictionary for both.
        internal static readonly Dictionary<Type, Type> s_typeConversions = new Dictionary<Type, Type>();

        /// <summary>
        /// Pass a SideLoader class type (eg, SL_Item) and get the corresponding Game class (eg, Item).
        /// </summary>
        /// <param name="_slType">Eg, typeof(SL_Item)</param>
        /// <param name="logging">If you want to log debug messages.</param>
        public static Type GetGameType(Type _slType, bool logging = true)
        {
            if (s_typeConversions.ContainsKey(_slType))
                return s_typeConversions[_slType];

            var name = _slType.Name.Substring(3, _slType.Name.Length - 3);

            Type ret = null;
            try
            {
                ret = Game_Assembly.GetType(name);
                if (ret == null) throw new Exception("Null");
            }
            catch
            {
                if (logging)
                {
                    SL.Log($"Could not get Game_Assembly Type '{name}'");
                }
            }

            s_typeConversions.Add(_slType, ret);

            return ret;
        }

        /// <summary>
        /// Pass a Game Class type (eg, Item) and get the corresponding SideLoader class (eg, SL_Item).
        /// </summary>
        /// <param name="_gameType">Eg, typeof(Item)</param>
        /// <param name="logging">If you want to log debug messages.</param>
        private static Type GetSLType(Type _gameType, bool logging = true)
        {
            //if (s_typeConversions.ContainsKey(_gameType))
            //    return s_typeConversions[_gameType];

            var name = $"SideLoader.SL_{_gameType.Name}";

            Type ret = null;
            try
            {
                ret = SL_Assembly.GetType(name);
                if (ret == null) throw new Exception("Null");
            }
            catch (Exception e)
            {
                if (logging)
                {
                    SL.Log($"Could not get SL_Assembly Type '{name}'");
                    SL.Log(e.Message);
                    SL.Log(e.StackTrace);
                }
            }

            //s_typeConversions.Add(_gameType, ret);

            return ret;
        }

        /// <summary>
        /// Get the "best-match" for the provided game class.
        /// Will get the highest-level base class of the provided game class with a matching SL class.
        /// </summary>
        /// <param name="gameType">The game class you want a match for.</param>
        /// <param name="originalQuery">Internal, do not use.</param>
        /// <returns>Best-match SL Type, if any, otherwise null.</returns>
        public static Type GetBestSLType(Type gameType, Type originalQuery = null)
        {
            var key = originalQuery ?? gameType;

            if (s_typeConversions.ContainsKey(key))
                return s_typeConversions[key];

            if (GetSLType(gameType, false) is Type slType && !slType.IsAbstract)
            {
                if (!s_typeConversions.ContainsKey(key))
                    s_typeConversions.Add(key, slType);

                return slType;
            }
            else
            {
                if (gameType.BaseType != null)
                    return GetBestSLType(gameType.BaseType, originalQuery ?? gameType);
                else
                    return null;
            }
        }

        /// <summary>
        /// Save an SL_Type object to xml.
        /// </summary>
        public static void SaveToXml(string dir, string saveName, object obj)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                dir += "/";
            }

            saveName = ReplaceInvalidChars(saveName);

            string path = dir + saveName + ".xml";
            if (File.Exists(path))
            {
                //SL.LogWarning("SaveToXml: A file already exists at " + path + "! Deleting...");
                File.Delete(path);
            }

            var xml = GetXmlSerializer(obj.GetType());

            FileStream file = File.Create(path);
            xml.Serialize(file, obj);
            file.Close();
        }

        /// <summary>
        /// Load an SL_Type object from XML.
        /// </summary>
        public static object LoadFromXml(string path)
        {
            if (!File.Exists(path))
            {
                SL.Log("LoadFromXml :: Trying to load an XML but path doesnt exist: " + path);
                return null;
            }

            try
            {
                // First we have to find out what kind of Type this xml was serialized as.
                string typeName = "";
                using (XmlReader reader = XmlReader.Create(path))
                {
                    while (reader.Read()) // just get the first element (root) then break.
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            // the real type might be saved as an attribute
                            if (!string.IsNullOrEmpty(reader.GetAttribute("type")))
                                typeName = reader.GetAttribute("type");
                            else
                                typeName = reader.Name;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(typeName) && SL_Assembly.GetType($"SideLoader.{typeName}") is Type type)
                {
                    using (var file = File.OpenRead(path))
                    {
                        var xml = GetXmlSerializer(type);
                        var obj = xml.Deserialize(file);
                        file.Dispose();
                        return obj;
                    }
                }
                else
                {
                    SL.LogError("LoadFromXml Error, could not serialize the Type of document! typeName: " + typeName);
                    return null;
                }
            }
            catch (Exception e)
            {
                SL.LogError("Exception reading the XML file, logging exceptions...");
                SL.LogInnerException(e);

                return null;
            }
        }

        internal static Type[] GetSLTypes()
        {
            var list = new List<Type>();

            foreach (var type in SL_Assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(SL_Serialized), true).Length > 0)
                    list.Add(type);
            }

            return m_slTypes = list.ToArray();
        }

        /// <summary>Remove invalid filename characters from a string</summary>
        public static string ReplaceInvalidChars(string s)
        {
            return string.Join("_", s.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
