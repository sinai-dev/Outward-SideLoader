using System.Collections.Generic;

namespace SideLoader
{
    public class SL_DealtDamageCondition : SL_EffectCondition
    {
        public List<SL_Damage> RequiredDamages = new List<SL_Damage>();

        public override void ApplyToComponent<T>(T component)
        {
            var types = SL_Damage.GetDamageList(this.RequiredDamages);

            (component as DealtDamageCondition).DealtDamages = types.List.ToArray();
        }

        public override void SerializeEffect<T>(T component)
        {
            this.RequiredDamages = SL_Damage.ParseDamageArray((component as DealtDamageCondition).DealtDamages);
        }
    }
}
