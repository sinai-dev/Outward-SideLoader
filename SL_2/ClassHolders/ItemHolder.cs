using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Dataminer
{
    public class ItemHolder
    {
        public int ItemID;
        public string Name;
        public string gameObjectName;
        public string saveDir;

        public string Description;
        public int LegacyItemID;

        public ItemStatsHolder StatsHolder;

        public List<string> Tags = new List<string>();

        public List<EffectTransformHolder> EffectTransforms = new List<EffectTransformHolder>();

        public static ItemHolder ParseItem(Item item)
        {
            // Debug.Log(item.Name);

            var itemHolder = new ItemHolder
            {
                Name = item.Name,
                gameObjectName = item.gameObject.name,
                Description = item.Description,
                ItemID = item.ItemID,
                LegacyItemID = item.LegacyItemID,
            };

            if (item.GetType() != typeof(Item))
            {
                try
                {
                    itemHolder.saveDir = GetRelativeTypeDirectory(item, "", item.GetType());
                }
                catch (Exception e)
                {
                    Debug.Log("Error getting savedir on item " + item.Name + ", message: " + e.Message);
                }
            }
            else
            {
                itemHolder.saveDir = "";
            }

            if (item.Stats != null)
            {
                itemHolder.StatsHolder = ItemStatsHolder.ParseItemStats(item.Stats);
            }

            if (item.Tags != null)
            {
                foreach (Tag tag in item.Tags)
                {
                    itemHolder.Tags.Add(tag.TagName);
                    ListManager.AddTagSource(tag, item.Name);
                }
            }

            foreach (Transform child in item.transform)
            {
                var effectsChild = EffectTransformHolder.ParseTransform(child);

                if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                {
                    itemHolder.EffectTransforms.Add(effectsChild);
                }
            }

            if (item is Equipment)
            {
                return EquipmentHolder.ParseEquipment(item as Equipment, itemHolder);
            }
            else if (item is DeployableTrap)
            {
                return TrapHolder.ParseTrap(item as DeployableTrap, itemHolder);
            }
            else if (item is Skill)
            {
                return SkillHolder.ParseSkill(item as Skill, itemHolder);
            }
            else if (item is Quest)
            {
                return QuestHolder.ParseQuest(item as Quest, itemHolder);
            }
            else
            {
                return itemHolder;
            }
        }

        public static void ParseAllItems()
        {
            if (At.GetValue(typeof(ResourcesPrefabManager), null, "ITEM_PREFABS") is Dictionary<string, Item> ItemPrefabs)
            {
                foreach (Item item in ItemPrefabs.Values)
                {
                    Debug.Log("Parsing " + item.Name + ", typeof: " + item.GetType());

                    // Parse the item. This will recursively dive.
                    var itemHolder = ParseItem(item);

                    ListManager.Items.Add(item.ItemID.ToString(), itemHolder);

                    // Folder and Save Name
                    string dir = GetItemFolder(item, itemHolder);
                    string saveName = item.Name + " (" + item.gameObject.name + ")";

                    Dataminer.SerializeXML(dir, saveName, itemHolder, typeof(ItemHolder));
                }
            }
            else
            {
                Debug.LogError("Could not find Item Prefabs!");
            }
        }

        private static string GetItemFolder(Item item, ItemHolder itemHolder)
        {
            string dir = Folders.Prefabs + "/Items";

            if (item.GetType().ToString() != "Item")
            {
                dir += GetRelativeTypeDirectory(item, "", item.GetType());
            }
            else
            {
                if (itemHolder.Tags.Contains("Consummable"))
                {
                    dir += "/Consumable";
                }
                else
                {
                    dir += "/_Unsorted";
                }
            }
            return dir;
        }

        public static string GetRelativeTypeDirectory(Item item, string dir, Type type)
        {
            dir = "/" + type + dir;
            var baseType = type.BaseType;
            if (baseType != null && baseType != typeof(Item))
            {
                dir = GetRelativeTypeDirectory(item, dir, baseType);
            }
            return dir;
        }
    }
}
