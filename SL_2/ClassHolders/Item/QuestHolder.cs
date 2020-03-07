using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NodeCanvas.StateMachines;
using NodeCanvas.Framework;

namespace Dataminer
{
    public class QuestHolder : ItemHolder
    {
        public string SerializedCanvas;

        public static QuestHolder ParseQuest(Quest quest, ItemHolder itemHolder)
        {
            var questHolder = new QuestHolder();
            try
            {
                var graph = quest.GetComponent<QuestTreeOwner>().graph as QuestTree;
                var serializedGraph = At.GetValue(typeof(Graph), graph as Graph, "_serializedGraph").ToString();
                questHolder.SerializedCanvas = serializedGraph;
            }
            catch { }

            At.InheritBaseValues(questHolder, itemHolder);

            return questHolder;
        }
    }
}
