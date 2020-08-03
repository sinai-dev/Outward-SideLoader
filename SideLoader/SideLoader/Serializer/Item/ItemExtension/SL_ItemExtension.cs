using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public abstract class SL_ItemExtension
    {
        public bool? Savable;

        [XmlIgnore] public virtual bool AddToChild { get => false; }
        [XmlIgnore] public virtual string ChildToAddTo { get => ""; }

        public static void ApplyExtensionList(Item item, SL_ItemExtension[] list, EditBehaviours editBehaviour = EditBehaviours.Override)
        {
            var dict = new Dictionary<Type, SL_ItemExtension>(); // Key: Game_type, Value: SL_ItemExtension template.

            // First, prepare the Dictionary, and add components that don't already exist.
            foreach (var slExtension in list)
            {
                var sl_type = slExtension.GetType();
                var game_type = Serializer.GetGameType(sl_type);

                if (dict.ContainsKey(game_type))
                {
                    SL.Log("Cannot add more than one of the same ItemExtension!");
                    continue;
                }

                dict.Add(game_type, slExtension);

                if (!item.GetComponentInChildren(game_type))
                {
                    if (slExtension.AddToChild)
                    {
                        var child = item.transform.Find(slExtension.ChildToAddTo);
                        if (!child)
                        {
                            child = new GameObject(slExtension.ChildToAddTo).transform;
                            child.parent = item.transform;
                        }
                        child.gameObject.AddComponent(game_type);
                    }
                    else
                    {
                        item.gameObject.AddComponent(game_type);
                    }
                }
            }

            // Now iterate the actual ItemExtension components, removing ones the user didn't define.
            // Also, apply SL templates to the Extensions now.
            var toRemove = new List<ItemExtension>();
            foreach (var comp in item.GetComponentsInChildren<ItemExtension>())
            {
                var game_type = comp.GetType();
                if (!dict.ContainsKey(game_type))
                {
                    if (editBehaviour == EditBehaviours.Destroy)
                        toRemove.Add(comp);
                }
                else
                {
                    var template = dict[game_type];

                    if (template.Savable != null)
                    {
                        comp.Savable = (bool)template.Savable;
                    }

                    template.ApplyToComponent(comp);
                }
            }

            if (editBehaviour == EditBehaviours.Destroy)
            {
                // Finally, remove the Extensions we don't want to use.
                var array = toRemove.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    var comp = array[i];
                    GameObject.Destroy(comp);
                }
            }
        }

        public abstract void ApplyToComponent<T>(T component) where T : ItemExtension;

        public static SL_ItemExtension SerializeExtension(ItemExtension extension)
        {
            var type = extension.GetType();

            if (Serializer.GetBestSLType(type) is Type sl_type && !sl_type.IsAbstract)
            {
                var holder = Activator.CreateInstance(sl_type) as SL_ItemExtension;

                holder.Savable = extension.Savable;

                holder.SerializeComponent(extension);

                return holder;
            }
            else
            {
                SL.Log(type + " is not supported yet, sorry!");
                return null;
            }
        }

        public abstract void SerializeComponent<T>(T extension) where T : ItemExtension;
    }
}
