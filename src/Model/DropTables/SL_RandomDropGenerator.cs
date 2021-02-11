using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_RandomDropGenerator
    {
        // ~~~~~~ User-Defined ~~~~~~

        public int MinNumberOfDrops = 1;
        public int MaxNumberOfDrops = 1;

        public int NoDrop_DiceValue;

        public List<SL_ItemDropChance> Drops = new List<SL_ItemDropChance>();

        // ~~~~~~ Internal ~~~~~~

        [XmlIgnore] internal Dictionary<int, int> m_dropRanges;

        public int MaxDiceValue
        {
            get
            {
                if (m_maxDiceValue == -1)
                {
                    int ret = NoDrop_DiceValue;

                    if (Drops != null)
                    {
                        foreach (var drop in Drops)
                            ret += drop.DiceValue;
                    }

                    if (ret > 0)
                        ret--; // -1 because 0 is included.

                    m_maxDiceValue = ret;
                }

                return m_maxDiceValue;
            }
        }
        private int m_maxDiceValue = -1;

        public void GenerateDrops(Transform container)
        {
            // Random.Range with INT arguments means the max value is exclusive.
            // For this reason, always +1 on the max value.
            // For example, if min rolls is 1 and max is also 1, we actually want Range(1, 2).
            int numRolls = Random.Range(this.MinNumberOfDrops, this.MaxNumberOfDrops + 1);

            for (int i = 0; i < numRolls; i++)
            {
                // also +1 on the max dice value to cancel out the "-1 because 0 is included"
                int roll = Random.Range(0, this.MaxDiceValue + 1);

                if (GetDropForDiceRoll(roll) is SL_ItemDropChance drop)
                {
                    drop.GenerateDrop(container);
                }
                //else
                //    SL.Log("No drop");
            }
        }

        public SL_ItemDropChance GetDropForDiceRoll(int diceRoll)
        {
            if (diceRoll < 0 || diceRoll > MaxDiceValue)
            {
                SL.Log("ERROR: Roll " + diceRoll + " is <0 or >" + MaxDiceValue);
                return null;
            }

            if (m_dropRanges == null)
                GetDropRanges();

            for (int i = 0; i < m_dropRanges.Count; i++)
            {
                var entry = m_dropRanges.ElementAt(i);

                // if dice roll was within range
                if (entry.Key <= diceRoll && entry.Value >= diceRoll)
                {
                    // if there is a NoDrop chance
                    if (NoDrop_DiceValue > 0)
                    {
                        if (i == 0)
                            return null; // Index was 0 so it was a no-drop.
                        else
                            i--;         // NoDrop was set, so remove 1 from index to get the real drop index.
                    }

                    return Drops[i];
                }
            }

            return null;
        }

        private void GetDropRanges()
        {
            //SL.Log("~~~~~ generating ranges ~~~~~~");

            m_dropRanges = new Dictionary<int, int>();

            int currentIndex = 0;

            if (NoDrop_DiceValue > 0)
                AddDropRange(NoDrop_DiceValue);

            if (this.Drops != null)
            {
                foreach (var drop in this.Drops)
                    AddDropRange(drop.DiceValue);
            }

            void AddDropRange(int diceValue)
            {
                int add = currentIndex + diceValue - 1;
                m_dropRanges.Add(currentIndex, add);
                currentIndex += diceValue;
                //SL.Log("Added range " + m_dropRanges.Last());
            }

            //SL.Log("~~~~~ DONE getting ranges ~~~~~~");
        }
    }
}
