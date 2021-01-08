using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader
{
    public class SL_PrePackDeployableCondition : SL_EffectCondition
    {
        public float ProximityDist;
        public float ProximityAngle;
        public int[] PackableItemIDs;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as PrePackDeployableCondition;

            comp.ProximityDist = this.ProximityDist;
            comp.ProximityAngle = this.ProximityAngle;

            var list = new List<Item>();
            foreach (var id in this.PackableItemIDs)
            {
                if (ResourcesPrefabManager.Instance.GetItemPrefab(id) is Item item)
                    list.Add(item);
            }
            comp.PackableItems = list.ToArray();
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as PrePackDeployableCondition;

            ProximityAngle = comp.ProximityAngle;
            ProximityDist = comp.ProximityDist;

            this.PackableItemIDs = comp.PackableItems.Select(it => it.ItemID).ToArray();
        }
    }
}
