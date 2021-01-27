using System.Collections.Generic;
using System.Linq;

namespace SideLoader
{
    public class SL_AttackTypeCondition : SL_EffectCondition
    {
        List<int> AffectOnAttackIDs = new List<int>();

        public override void ApplyToComponent<T>(T component)
        {
            (component as AttackTypeCondition).AffectOnAttacks = this.AffectOnAttackIDs.ToArray();
        }

        public override void SerializeEffect<T>(T component)
        {
            AffectOnAttackIDs = (component as AttackTypeCondition).AffectOnAttacks.ToList();
        }
    }
}
