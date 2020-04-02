using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SideLoader_2
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

            if (type != "" && Type.GetType("SideLoader_2." + type + ", SideLoader_2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null") is Type t)
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
            typeof(AddStatusEffectBuildupHolder),
            typeof(AddStatusEffectHolder),
            typeof(AffectBurntHealthHolder),
            typeof(AffectBurntManaHolder),
            typeof(AffectBurntStaminaHolder),
            typeof(AffectHealthHolder),
            typeof(AffectHealthParentOwnerHolder),
            typeof(AffectManaHolder),
            typeof(AffectNeedHolder),
            typeof(AffectStabilityHolder),
            typeof(AffectStaminaHolder),
            typeof(AffectStatHolder),
            typeof(BagHolder),
            //typeof(Character.SpellCastModifier),
            //typeof(Item.CastTakeTypes),
            //typeof(EffectConditionHolder),
            //typeof(EffectConditionHolder.ConditionHolder),
            typeof(EffectHolder),
            typeof(EffectTransformHolder),
            typeof(EquipmentHolder),
            typeof(EquipmentStatsHolder),
            //typeof(EquipmentSlot.EquipmentSlotIDs),
            //typeof(Equipment.IKMode),
            //typeof(Equipment.TwoHandedType),
            typeof(ImbueWeaponHolder),
            typeof(ItemHolder),
            typeof(ItemStatsHolder),
            typeof(PunctualDamageHolder),
            typeof(RecipeHolder),
            typeof(RecipeHolder.ItemQty),
            typeof(ReduceDurabilityHolder),
            typeof(RemoveStatusEffectHolder),
            typeof(SkillHolder),
            typeof(SkillHolder.SkillItemReq),
            //typeof(ShootBlastHolder),
            //typeof(ShootProjectileHolder),
            //typeof(TrapEffectHolder),
            //typeof(TrapHolder),
            typeof(Vector3),
            typeof(WeaponHolder),
            typeof(WeaponStatsHolder),
            typeof(WeaponStats.AttackData),
            typeof(WeaponDamageHolder),
        };
    }
}
