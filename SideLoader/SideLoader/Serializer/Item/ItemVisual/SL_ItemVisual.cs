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

        public void ApplyToItem(Item item)
        {
            if (CustomItemVisuals.GetOrigItemVisuals(item, Type) is Transform prefab)
            {
                bool setPrefab = false;
                if (!string.IsNullOrEmpty(Prefab_SLPack) && SL.Packs.ContainsKey(Prefab_SLPack))
                {
                    var pack = SL.Packs[this.Prefab_SLPack];

                    if (pack.AssetBundles.ContainsKey(Prefab_AssetBundle))
                    {
                        var newVisuals = pack.AssetBundles[Prefab_AssetBundle].LoadAsset<GameObject>(Prefab_Name);
                        prefab = SetVisualPrefab(item, this.Type, newVisuals.transform, prefab).transform;
                        setPrefab = true;
                    }
                }
                if (!setPrefab)
                {
                    prefab = CustomItemVisuals.CloneVisualPrefab(item, Type).transform;
                }

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

        public GameObject SetVisualPrefab(Item item, VisualPrefabType type, Transform newVisuals, Transform oldVisuals)
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

        public virtual void ApplyToVisuals(ItemVisual itemVisual, Transform actualVisuals)
        {
            SL.Log($"Applying ItemVisuals settings to " + actualVisuals.name);

            if (Position != null)
            {
                actualVisuals.localPosition = (Vector3)Position;
            }
            if (Rotation != null)
            {
                actualVisuals.eulerAngles = (Vector3)Rotation;
            }
            if (PositionOffset != null)
            {
                actualVisuals.localPosition += (Vector3)PositionOffset;
            }
            if (RotationOffset != null)
            {
                actualVisuals.eulerAngles += (Vector3)RotationOffset;
            }            
        }

        public static SL_ItemVisual ParseVisualToTemplate(ItemVisual itemVisual)
        {
            var template = (SL_ItemVisual)Activator.CreateInstance(Serializer.GetBestSLType(itemVisual.GetType()));
            template.SerializeItemVisuals(itemVisual);
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
            //else
            //{
            //    SL.Log("Custom SkinnedMesh Visual Prefabs (eg. Bows, Armor) are not supported yet, sorry!");
            //}
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
