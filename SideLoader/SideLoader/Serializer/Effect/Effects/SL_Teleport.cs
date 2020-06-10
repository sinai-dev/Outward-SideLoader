using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Teleport : SL_Effect
    {
        public float MaxRange;
        public float MaxTargetRange;
        public float MaxYDiff;
        public Vector3 OffsetRelativeTarget;
        public bool UseTarget;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Teleport;

            comp.MaxRange = this.MaxRange;
            comp.MaxTargetRange = this.MaxTargetRange;
            comp.MaxYDiff = this.MaxYDiff;
            comp.OffsetRelativeTarget = this.OffsetRelativeTarget;
            comp.UseTarget = this.UseTarget;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var template = holder as SL_Teleport;
            var comp = effect as Teleport;

            template.MaxRange = comp.MaxRange;
            template.MaxYDiff = comp.MaxYDiff;
            template.MaxTargetRange = comp.MaxTargetRange;
            template.OffsetRelativeTarget = comp.OffsetRelativeTarget;
            template.UseTarget = comp.UseTarget;
        }
    }
}
