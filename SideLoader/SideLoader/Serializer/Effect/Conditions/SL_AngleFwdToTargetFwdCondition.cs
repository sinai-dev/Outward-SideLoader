using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_AngleFwdToTargetFwdCondition : SL_EffectCondition
    {
        public List<Vector2> AnglesToCompare = new List<Vector2>();

        public override void ApplyToComponent<T>(T component)
        {
            (component as AngleFwdToTargetFwdCondition).AnglesToCompare = this.AnglesToCompare.ToArray();
        }

        public override void SerializeEffect<T>(EffectCondition component, T template)
        {
            var holder = template as SL_AngleFwdToTargetFwdCondition;
            var angles = (component as AngleFwdToTargetFwdCondition).AnglesToCompare;

            holder.AnglesToCompare = angles.ToList();
        }
    }
}
