using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Summon : SL_Effect
    {
        public const string CorruptionSpiritPath = @"CorruptionSpirit.prefab";
        public const string SummonGhostPath = @"NewGhostOneHandedAlly.prefab";

        public enum PrefabTypes
        {
            Item,
            Resource
        }

        // if Item: the ItemID, if Character: the 'Resources/' asset path.
        public string Prefab;
        public PrefabTypes SummonPrefabType;
        public int BufferSize = 1;
        public bool LimitOfOne;
        public Summon.InstantiationManagement SummonMode;
        public Summon.SummonPositionTypes PositionType;
        public float MinDistance;
        public float MaxDistance;
        public bool SameDirectionAsSummoner;
        public Vector3 SummonLocalForward;
        public bool IgnoreOnDestroy;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Summon;

            if (this.SummonPrefabType == PrefabTypes.Resource)
            {
                if (Resources.Load<GameObject>(Prefab) is GameObject prefab)
                {
                    comp.SummonedPrefab = prefab.transform;
                }
                else
                {
                    SL.Log("Could not find Resources asset: " + this.Prefab + "!", 1);
                    return;
                }
            }
            else
            {
                if (int.TryParse(this.Prefab, out int id) && ResourcesPrefabManager.Instance.GetItemPrefab(id) is Item item)
                {
                    comp.SummonedPrefab = item.transform;
                }
                else
                {
                    SL.Log("Could not find an Item with the ID " + this.Prefab + ", or that is not a valid ID!", 0);
                    return;
                }
            }

            comp.BufferSize = this.BufferSize;
            comp.LimitOfOne = this.LimitOfOne;
            comp.InstantiationMode = this.SummonMode;
            comp.PositionType = this.PositionType;
            comp.MinDistance = this.MinDistance;
            comp.MaxDistance = this.MaxDistance;
            comp.SameDirAsSummoner = this.SameDirectionAsSummoner;
            comp.SummonLocalForward = this.SummonLocalForward;
            comp.IgnoreOnDestroy = this.IgnoreOnDestroy;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var template = holder as SL_Summon;
            var comp = effect as Summon;

            if (effect is SummonAI)
            {
                template.SummonPrefabType = PrefabTypes.Resource;

                var name = comp.SummonedPrefab.name;
                if (name.EndsWith("(Clone)"))
                {
                    name = name.Substring(0, name.Length - 7);
                }

                template.Prefab = name;
            }
            else
            {
                template.SummonPrefabType = PrefabTypes.Item;

                template.Prefab = comp.SummonedPrefab.gameObject.GetComponent<Item>().ItemID.ToString();
            }

            template.BufferSize = comp.BufferSize;
            template.LimitOfOne = comp.LimitOfOne;
            template.SummonMode = comp.InstantiationMode;
            template.PositionType = comp.PositionType;
            template.MinDistance = comp.MinDistance;
            template.MaxDistance = comp.MaxDistance;
            template.SameDirectionAsSummoner = comp.SameDirAsSummoner;
            template.SummonLocalForward = comp.SummonLocalForward;
            template.IgnoreOnDestroy = comp.IgnoreOnDestroy;
        }
    }
}
