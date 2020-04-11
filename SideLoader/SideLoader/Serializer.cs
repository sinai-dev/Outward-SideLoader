using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SideLoader
{
    public class Serializer
    {
        public static void SaveToXml(string dir, string saveName, object obj)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                dir += "/";
            }

            saveName = SL.ReplaceInvalidChars(saveName);

            string path = dir + saveName + ".xml";
            if (File.Exists(path))
            {
                Debug.LogWarning("SaveToXml: A file already exists at " + path + "! Deleting...");
                File.Delete(path);
            }

            XmlSerializer xml = new XmlSerializer(obj.GetType(), Types);
            FileStream file = File.Create(path);
            xml.Serialize(file, obj);
            file.Close();
        }

        public static object LoadFromXml(string path)
        {
            if (!File.Exists(path))
            {
                SL.Log("LoadFromXml :: Trying to load an XML but path doesnt exist: " + path);
                return null;
            }

            // First we have to find out what kind of Type this xml was serialized as.
            string type = "";
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read()) // just get the first element (root) then break.
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // the real type might be saved as an attribute
                        if (!string.IsNullOrEmpty(reader.GetAttribute("type")))
                        {
                            type = reader.GetAttribute("type");
                        }
                        else
                        {
                            type = reader.Name;
                        }
                        break;
                    }
                }
            }

            if (type != "" && Type.GetType("SideLoader." + type + ", SideLoader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null") is Type t)
            {
                XmlSerializer xml = new XmlSerializer(t, Types);
                FileStream file = File.OpenRead(path);
                var obj = xml.Deserialize(file);
                file.Close();
                return obj;
            }
            else
            {
                SL.Log("LoadFromXml :: Error, could not serialize the Type of document! typeName: " + type, 1);
                return null;
            }
        }

        public static Type[] Types { get; } = new Type[]
        {
            typeof(SL_AddStatusEffectBuildUp),
            typeof(SL_AddStatusEffect),
            typeof(SL_AffectBurntHealth),
            typeof(SL_AffectBurntMana),
            typeof(SL_AffectBurntStamina),
            typeof(SL_AffectHealth),
            typeof(SL_AffectHealthParentOwner),
            typeof(SL_AffectMana),            
            typeof(SL_AffectStability),
            typeof(SL_AffectStamina),
            typeof(SL_AffectStat),
            typeof(SL_Bag),            
            typeof(SL_Effect),
            typeof(SL_EffectTransform),
            typeof(SL_Equipment),
            typeof(SL_EquipmentStats),
            typeof(SL_ImbueWeapon),
            typeof(SL_Item),
            typeof(SL_ItemStats),
            typeof(SL_Material),
            typeof(SL_PunctualDamage),
            typeof(SL_Recipe),
            typeof(SL_RemoveStatusEffect),
            typeof(SL_Skill),            
            typeof(Vector3),
            typeof(SL_Weapon),
            typeof(SL_WeaponStats),
            typeof(WeaponStats.AttackData),
            typeof(SL_WeaponDamage),
        };
    }
}
