using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ProximityCondition : SL_EffectCondition
    {
        public List<SL_Skill.SkillItemReq> RequiredItems = new List<SL_Skill.SkillItemReq>();
        public float MaxDistance;
        public Vector3 Offset = Vector3.zero;
        public float ProximityAngle;
        public bool OrMode;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as ProximityCondition;

            comp.ProximityDist = this.MaxDistance;
            comp.Offset = this.Offset;
            comp.OrMode = this.OrMode;
            comp.ProximityAngle = this.ProximityAngle;

            var list = new List<Skill.ItemRequired>();
            foreach (var req in this.RequiredItems)
            {
                Item item = ResourcesPrefabManager.Instance.GetItemPrefab(req.ItemID);
                if (!item)
                {
                    SL.Log("SkillItemReq: Couldn't find an item with the ID " + req.ItemID);
                    continue;
                }
                
                list.Add(new Skill.ItemRequired()
                {
                    Item = item,
                    Quantity = req.Quantity,
                    Consume = req.Consume
                });
            }

            comp.ProximityItemReq = list.ToArray();
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as ProximityCondition;

            MaxDistance = comp.ProximityDist;
            Offset = comp.Offset;
            this.ProximityAngle = comp.ProximityAngle;
            this.OrMode = comp.OrMode;

            RequiredItems = new List<SL_Skill.SkillItemReq>();
            foreach (var req in comp.ProximityItemReq)
            {
                RequiredItems.Add(new SL_Skill.SkillItemReq()
                {
                    Consume = req.Consume,
                    Quantity = req.Quantity,
                    ItemID = req.Item.ItemID
                });
            }
        }
    }
}
