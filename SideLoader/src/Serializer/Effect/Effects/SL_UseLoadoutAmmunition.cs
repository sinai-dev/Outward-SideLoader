using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_UseLoadoutAmmunition : SL_Effect
    {
        public bool MainHand;

        public override void ApplyToComponent<T>(T component)
        {
            (component as UseLoadoutAmunition).MainHand = this.MainHand;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            (holder as SL_UseLoadoutAmmunition).MainHand = (effect as UseLoadoutAmunition).MainHand;
        }
    }
}
