using System.Collections.Generic;

namespace SideLoader
{
    public class SL_Building : SL_Item
    {
        public Building.BuildingTypes? BuildingType;

        public SL_ConstructionPhase[] ConstructionPhases;
        public SL_ConstructionPhase[] UpgradePhases;

        public override void ApplyToItem(Item item)
        {
            base.ApplyToItem(item);

            var comp = item as Building;

            if (this.BuildingType != null)
                comp.BuildingType = (Building.BuildingTypes)this.BuildingType;

            if (this.ConstructionPhases != null)
            {
                var list = new List<Building.ConstructionPhase>();
                foreach (var phase in this.ConstructionPhases)
                    list.Add(phase.GeneratePhase());
                At.SetField(comp, "m_constructionPhases", list.ToArray());
            }

            if (this.UpgradePhases != null)
            {
                var list = new List<Building.ConstructionPhase>();
                foreach (var phase in this.UpgradePhases)
                    list.Add(phase.GeneratePhase());
                At.SetField(comp, "m_upgradePhases", list.ToArray());
            }
        }

        public override void SerializeItem(Item item)
        {
            base.SerializeItem(item);

            var building = item as Building;

            var list = new List<SL_ConstructionPhase>();

            var constructionPhases = At.GetField(building, "m_constructionPhases") as Building.ConstructionPhase[];
            foreach (var phase in constructionPhases)
                list.Add(SL_ConstructionPhase.SerializePhase(phase));

            ConstructionPhases = list.ToArray();

            list.Clear();
            var upgradePhases = At.GetField(building, "m_upgradePhases") as Building.ConstructionPhase[];
            foreach (var phase in upgradePhases)
                list.Add(SL_ConstructionPhase.SerializePhase(phase));

            UpgradePhases = list.ToArray();
        }
    }
}
