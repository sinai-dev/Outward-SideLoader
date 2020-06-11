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
                if (m_SLAssembly == null)
                {
                    // We should be able to get it this way
                    m_SLAssembly = Assembly.GetExecutingAssembly();

                    // If for some reason it doesnt work (perhaps called by another mod from outside SideLoader.dll before SideLoader initializes?)
                    if (!m_SLAssembly.FullName.Contains("SideLoader"))
                    {
                        m_SLAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName.Contains("SideLoader"));
                    }
                }

                return m_SLAssembly;
            }
        }

        private static Assembly m_SLAssembly; 
        
        /// <summary>
        /// The Assembly-Csharp.dll AppDomain reference.
        /// </summary>
        public static Assembly Game_Assembly
        {
            get
            {
                if (m_gameAssembly == null)
                {
                    m_gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == m_gameAssemblyFullName);
                }

                return m_gameAssembly;
            }
        }

        private const string m_gameAssemblyFullName = "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
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
                    var list = new List<Type>();
                    foreach (var type in SL_Assembly.GetTypes())
                    {
                        // check if marked as SL_Serialized
                        if (type.GetCustomAttributes(typeof(SL_Serialized), true).Length > 0)
                        {
                            list.Add(type);
                        }
                    }

                    // add other types
                    list.AddRange(new Type[]
                    {
                        typeof(WeaponStats.AttackData),
                    });

                    m_slTypes = list.ToArray();
                }

                return m_slTypes;
            }
        }

        private static Type[] m_slTypes;

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

            XmlSerializer xml = new XmlSerializer(obj.GetType(), SLTypes);
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
                XmlSerializer xml = new XmlSerializer(type, SLTypes);
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

        /// <summary>Remove invalid filename characters from a string</summary>
        public static string ReplaceInvalidChars(string s)
        {
            return string.Join("_", s.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
