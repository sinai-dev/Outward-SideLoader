using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_QuestEventAreaCondition : SL_EffectCondition
    {
        public List<string> EventUIDs = new List<string>();

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as QuestEventAreaCondition;

            var list = new List<QuestEventReference>();
            foreach (var uid in this.EventUIDs)
            {
                var _event = QuestEventManager.Instance.GetQuestEvent(uid);
                if (_event == null)
                {
                    SL.Log("", 0);
                    continue;
                }
                list.Add(new QuestEventReference()
                {
                    EventUID = uid,
                    UID = uid
                });
            }

            comp.EventToCheck = list.ToArray();
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var holder = template as SL_QuestEventAreaCondition;
            var comp = component as QuestEventAreaCondition;

            foreach (var _event in comp.EventToCheck)
            {
                holder.EventUIDs.Add(_event.EventUID);
            }
        }
    }
}
