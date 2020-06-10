using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SideLoader
{

    public class SL_CounterSuccessCondition : SL_EffectCondition
    {
        public override void ApplyToComponent<T>(T component)
        {
            var skill = component.transform.root.gameObject.GetComponent<CounterSkill>(); 
            
            if (!skill)
            {
                SL.Log("Trying to apply a CounterSuccessCondition on a skill which is not a Counter Skill! Error!", 0);
                return;
            }

            (component as CounterSuccessCondition).Skill = skill;
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            // Don't actually need to do anything for this effect. Simply adding the component is the entirety of the effect.
        }
    }
}
