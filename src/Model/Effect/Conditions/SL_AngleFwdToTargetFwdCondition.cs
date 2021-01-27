using System.Collections.Generic;
using System.Linq;
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

        public override void SerializeEffect<T>(T component)
        {
            var angles = (component as AngleFwdToTargetFwdCondition).AnglesToCompare;

            AnglesToCompare = angles.ToList();
        }
    }
}
