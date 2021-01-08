using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    public class SL_InBoundsCondition : SL_EffectCondition
    {
        public Bounds Bounds;

        public override void ApplyToComponent<T>(T component)
        {
            (component as InBoundsCondition).Bounds = this.Bounds;
        }

        public override void SerializeEffect<T>(T component)
        {
            this.Bounds = (component as InBoundsCondition).Bounds;
        }
    }
}
