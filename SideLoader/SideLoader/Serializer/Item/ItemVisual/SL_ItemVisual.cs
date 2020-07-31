using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_ItemVisual
    {
        /// <summary>You don't need to set this if you're setting a value in an SL_Item template, SideLoader will figure this out for you.</summary>
        [XmlIgnore]
        public VisualPrefabType Type;

        /// <summary>
        /// Used to set the visual prefab from an existing prefab at the given path. Checks the Unity Resources "folder" for the asset path.
        /// </summary>
        public string ResourcesPrefabPath = "";

        /// <summary>SLPack using for AssetBundle visuals</summary>
        public string Prefab_SLPack = "";
        /// <summary>AssetBundle file name inside specified Prefab_SLPack</summary>
        public string Prefab_AssetBundle = "";
        /// <summary>Prefab GameObject name inside specified Prefab_AssetBundle</summary>
        public string Prefab_Name = "";

        /// <summary>Optional, directly set the position</summary>
        public Vector3? Position;
        /// <summary>Optional, directly set the rotation</summary>
        public Vector3? Rotation;

        /// <summary>Optional, add offset to position</summary>
        public Vector3? PositionOffset;
        /// <summary>Optional, add offset to rotation</summary>
        public Vector3? RotationOffset;

        /// <summary>
        /// Apply the SL_ItemVisual prefab to the Item.
        /// </summary>
        /// <param name="item">The Item to set to.</param>
        public void ApplyToItem(Item item)
        {
            if (CustomItemVisuals.GetOrigItemVisuals(item, Type) is Transform prefab)
            {
                bool setPrefab = false;
                
                // Check for SLPack Prefabs first
                if (!string.IsNullOrEmpty(Prefab_SLPack) && SL.Packs.ContainsKey(Prefab_SLPack))
                {
                    var pack = SL.Packs[this.Prefab_SLPack];

                    if (pack.AssetBundles.ContainsKey(Prefab_AssetBundle))
                    {
                        var newVisuals = pack.AssetBundles[Prefab_AssetBundle].LoadAsset<GameObject>(Prefab_Name);
                        prefab = SetCustomVisualPrefab(item, this.Type, newVisuals.transform, prefab).transform;
                        setPrefab = true;
                    }
                }
                // Check for ResourcesPrefabPath.
                else if (!string.IsNullOrEmpty(ResourcesPrefabPath))
                {
                    // Only set this if the user has defined a different value than what exists on the item.
                    bool set = false;
                    switch (Type)
                    {
                        case VisualPrefabType.VisualPrefab:
                            set = item.VisualPrefabPath == ResourcesPrefabPath; break;
                        case VisualPrefabType.SpecialVisualPrefabDefault:
                            set = item.SpecialVisualPrefabDefaultPath == ResourcesPrefabPath; break;
                        case VisualPrefabType.SpecialVisualPrefabFemale:
                            set = item.SpecialVisualPrefabFemalePath == ResourcesPrefabPath; break;
                    }

                    if (!set)
                    {
                        // get visuals by GetOrigItemVIsuals? May as well just save the ID then?
                        //var id = ResourcesPrefabPath.Substring(ResourcesPrefabPath.LastIndexOf('/'), 7);
                        //var orig = CustomItemVisuals.GetOrigItemVisuals(ResourcesPrefabManager.Instance.GetItemPrefab(id), Type);

                        // If another SL Item modifies these visuals, we will be getting the modified version...
                        var orig = ResourcesPrefabManager.Instance.GetItemVisualPrefab(ResourcesPrefabPath);

                        if (!orig)
                        {
                            SL.Log("SL_ItemVisual: Could not find an Item Visual at the Resources path: " + ResourcesPrefabPath);
                        }
                        else
                        {
                            CustomItemVisuals.CloneVisualPrefab(item, orig.gameObject, Type, true);

                            switch (Type)
                            {
                                case VisualPrefabType.VisualPrefab:
                                    At.SetValue(ResourcesPrefabPath, typeof(Item), item, "m_visualPrefabPath"); break;
                                case VisualPrefabType.SpecialVisualPrefabDefault:
                                    At.SetValue(ResourcesPrefabPath, typeof(Item), item, "m_specialVisualPrefabDefaultPath"); break;
                                case VisualPrefabType.SpecialVisualPrefabFemale:
                                    At.SetValue(ResourcesPrefabPath, typeof(Item), item, "m_specialVisualPrefabFemalePath"); break;
                            }

                            setPrefab = true;
                        }
                    }
                }

                // If we didn't change the Visual Prefab in any way, clone the original to avoid conflicts.
                if (!setPrefab)
                {
                    prefab = CustomItemVisuals.CloneVisualPrefab(item, Type).transform;
                }

                // Get the actual visuals (for weapons and a lot of items, this is not the base prefab).
                Transform actualVisuals = prefab.transform;
                if (prefab.childCount > 0)
                {
                    foreach (Transform child in prefab)
                    {
                        if (child.gameObject.activeSelf && child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
                        {
                            actualVisuals = child;
                            break;
                        }
                    }
                }

                if (actualVisuals)
                {
                    var visualComp = prefab.GetComponent<ItemVisual>();

                    ApplyToVisuals(visualComp, actualVisuals);
                }
            }
        }

        /// <summary>
        /// Sets a CUSTOM visual prefab to an Item. Don't use this for transmogs.
        /// </summary>
        /// <param name="item">The Item to set to.</param>
        /// <param name="type">The Type of visual prefab you are setting.</param>
        /// <param name="newVisuals">The new CUSTOM visual prefab.</param>
        /// <param name="oldVisuals">The original visual prefab.</param>
        /// <returns></returns>
        public GameObject SetCustomVisualPrefab(Item item, VisualPrefabType type, Transform newVisuals, Transform oldVisuals)
        {
            Debug.Log($"Setting the {type} for {item.Name}");

            var basePrefab = GameObject.Instantiate(oldVisuals.gameObject);
            GameObject.DontDestroyOnLoad(basePrefab);
            basePrefab.SetActive(false);

            var visualModel = UnityEngine.Object.Instantiate(newVisuals.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(visualModel.gameObject);

            if (type == VisualPrefabType.VisualPrefab)
            {
                // At the moment, the only thing we replace on ItemVisuals is the 3d model, everything else is a clone.
                foreach (Transform child in basePrefab.transform)
                {
                    // the real 3d model will always have boxcollider and meshrenderer. this is the object we want to replace.
                    if (child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
                    {
                        child.gameObject.SetActive(false);

                        visualModel.transform.position = child.position;
                        visualModel.transform.rotation = child.rotation;

                        visualModel.transform.parent = child.parent;

                        break;
                    }
                }
            }
            else
            {
                if (!visualModel.GetComponent<ItemVisual>() && basePrefab.GetComponent<ItemVisual>() is ItemVisual itemVisual)
                {
                    SL.GetCopyOf(itemVisual, visualModel.transform);
                }

                visualModel.transform.position = basePrefab.transform.position;
                visualModel.transform.rotation = basePrefab.transform.rotation;
                visualModel.gameObject.SetActive(false);

                // we no longer need the clone for these visuals. we should clean it up.
                UnityEngine.Object.DestroyImmediate(basePrefab.gameObject);

                basePrefab = visualModel;
            }

            //At.SetValue(basePrefab.transform, typeof(Item), item, type.ToString());
            CustomItemVisuals.SetVisualPrefabLink(item, basePrefab, type);

            return basePrefab;
        }

        /// <summary>
        /// Applies the values to the ItemVisual component itself.
        /// </summary>
        /// <param name="itemVisual">The ItemVisual to apply to.</param>
        /// <param name="visuals">The visual prefab you want to set to.</param>
        public virtual void ApplyToVisuals(ItemVisual itemVisual, Transform visuals)
        {
            SL.Log($"Applying ItemVisuals settings to " + visuals.name);

            if (Position != null)
            {
                visuals.localPosition = (Vector3)Position;
            }
            if (Rotation != null)
            {
                visuals.eulerAngles = (Vector3)Rotation;
            }
            if (PositionOffset != null)
            {
                visuals.localPosition += (Vector3)PositionOffset;
            }
            if (RotationOffset != null)
            {
                visuals.eulerAngles += (Vector3)RotationOffset;
            }            
        }

        public static SL_ItemVisual ParseVisualToTemplate(Item item, VisualPrefabType type, ItemVisual itemVisual)
        {
            var template = (SL_ItemVisual)Activator.CreateInstance(Serializer.GetBestSLType(itemVisual.GetType()));
            template.SerializeItemVisuals(itemVisual);
            switch (type)
            {
                case VisualPrefabType.VisualPrefab:
                    template.ResourcesPrefabPath = item.VisualPrefabPath; break;
                case VisualPrefabType.SpecialVisualPrefabDefault:
                    template.ResourcesPrefabPath = item.SpecialVisualPrefabDefaultPath; break;
                case VisualPrefabType.SpecialVisualPrefabFemale:
                    template.ResourcesPrefabPath = item.SpecialVisualPrefabFemalePath; break;
            };
            return template;
        }

        public virtual void SerializeItemVisuals(ItemVisual itemVisual)
        {
            if (!itemVisual.gameObject.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                var t = itemVisual.transform;

                foreach (Transform child in t)
                {
                    if (child.GetComponent<MeshRenderer>() && child.GetComponent<BoxCollider>())
                    {
                        Position = child.localPosition;
                        Rotation = child.eulerAngles;

                        break;
                    }
                }
            }
            else
            {
                Position = itemVisual.transform.position;
                Rotation = itemVisual.transform.rotation.eulerAngles;
            }
        }
    }

    /// <summary>
    /// Helper enum for the possible Visual Prefab types on Items. 
    /// </summary>
    public enum VisualPrefabType
    {
        /// <summary>Item.VisualPrefab</summary>
        VisualPrefab,
        /// <summary>Item.SpecialVisualPrefabDefault</summary>
        SpecialVisualPrefabDefault,
        /// <summary>Item.SpecialVisualPrefabFemale</summary>
        SpecialVisualPrefabFemale
    }
}
