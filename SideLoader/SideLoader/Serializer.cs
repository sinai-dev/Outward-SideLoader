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
        public static Assembly SL_Assembly
        {
            get
            {
                if (m_slAssembly == null)
                {
                    m_slAssembly = Assembly.GetExecutingAssembly();
                }
                return m_slAssembly;
            }
        }

        private static Assembly m_slAssembly;

        /// <summary>
        /// The Assembly-Csharp.dll AppDomain reference.
        /// </summary>
        public static Assembly Game_Assembly
        {
            get
            {
                if (m_gameAssembly == null)
                {
                    // Any game-class would work, I just picked Item.
                    m_gameAssembly = typeof(Item).Assembly;
                }

                return m_gameAssembly;
            }
        }

        private static Assembly m_gameAssembly;

        /// <summary>
        /// List of SL_Type classes (types marked as SL_Serialized).
        /// </summary>
        public static Type[] SLTypes
        {
            get
            {
                if (m_slTypes == null || m_slTypes.Length < 1)
                {
                    var list = new List<Type>
                    {
                        // Serializable game classes (currently only use 1)
                        typeof(WeaponStats.AttackData),
                    };

                    // add SL_Serialized types (custom types)
                    foreach (var type in SL_Assembly.GetTypes())
                    {
                        // check if marked as SL_Serialized
                        if (type.GetCustomAttributes(typeof(SL_Serialized), true).Length > 0)
                        {
                            list.Add(type);
                        }
                    }

                    m_slTypes = list.ToArray();
                }

                return m_slTypes;
            }
        }

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

        /// <summary>
        /// Pass a SideLoader class type (eg, SL_Item) and get the corresponding Game class (eg, Item).
        /// </summary>
        /// <param name="_slType">Eg, typeof(SL_Item)</param>
        /// <param name="logging">If you want to log debug messages.</param>
        public static Type GetGameType(Type _slType, bool logging = true)
        {
            var name = _slType.Name.Substring(3, _slType.Name.Length - 3);

            Type t = null;
            try
            {
                t = Game_Assembly.GetType(name);
                if (t == null) throw new Exception("Null");
            }
            catch (Exception e)
            {
                if (logging)
                {
                    SL.Log($"Could not get Game_Assembly Type '{name}'", 0);
                    SL.Log(e.Message, 0);
                    SL.Log(e.StackTrace, 0);
                }
            }

            return t;
        }

        /// <summary>
        /// Pass a Game Class type (eg, Item) and get the corresponding SideLoader class (eg, SL_Item).
        /// </summary>
        /// <param name="_gameType">Eg, typeof(Item)</param>
        /// <param name="logging">If you want to log debug messages.</param>
        public static Type GetSLType(Type _gameType, bool logging = true)
        {
            var name = $"SideLoader.SL_{_gameType.Name}";

            Type t = null;
            try
            {
                t = SL_Assembly.GetType(name);
                if (t == null) throw new Exception("Null");
            }
            catch (Exception e)
            {
                if (logging)
                {
                    SL.Log($"Could not get SL_Assembly Type '{name}'", 0);
                    SL.Log(e.Message, 0);
                    SL.Log(e.StackTrace, 0);
                }
            }

            return t;
        }

        /// <summary>
        /// Get the "best-match" for the provided game class.
        /// Will get the highest-level base class of the provided game class with a matching SL class.
        /// </summary>
        /// <param name="type">The game class you want a match for.</param>
        /// <returns>Best-match SL Type, if any, otherwise null.</returns>
        public static Type GetBestSLType(Type type)
        {
            if (GetSLType(type, false) is Type slType && !slType.IsAbstract)
            {
                return slType;
            }
            else
            {
                if (type.BaseType != null)
                {
                    return GetBestSLType(type.BaseType);
                }
                else
                {
                    return null;
                }
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
                //Debug.LogWarning("SaveToXml: A file already exists at " + path + "! Deleting...");
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
                            {
                                typeName = reader.GetAttribute("type");
                            }
                            else
                            {
                                typeName = reader.Name;
                            }
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(typeName) && SL_Assembly.GetType($"SideLoader.{typeName}") is Type type)
                {
                    var xml = GetXmlSerializer(type);
                    FileStream file = File.OpenRead(path);
                    var obj = xml.Deserialize(file);
                    file.Close();
                    return obj;
                }
                else
                {
                    SL.Log("LoadFromXml Error, could not serialize the Type of document! typeName: " + typeName, 1);
                    return null;
                }
            }
            catch (Exception e)
            {
                SL.Log("Exception reading the XML file, logging exceptions...");
                while (e != null)
                {
                    SL.Log($" - {e.GetType()}, {e.Message}");
                    e = e.InnerException;
                }

                return null;
            }
        }

        /// <summary>Remove invalid filename characters from a string</summary>
        public static string ReplaceInvalidChars(string s)
        {
            return string.Join("_", s.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
