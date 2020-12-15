using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_Death : SL_Effect
    {
        // This class uses no fields, it's a self-executing effect.

        public override void ApplyToComponent<T>(T component) { }

        public override void SerializeEffect<T>(T effect) { }
    }
}
