using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    [SL_Serialized]
    public class SL_Damage
    {
        public float Damage = 0f;
        public DamageType.Types Type = DamageType.Types.Count;

        public static DamageList GetDamageList(List<SL_Damage> list)
        {
            var newlist = new DamageList();
            foreach (var entry in list)
            {
                newlist.Add(entry.GetDamageType());
            }

            return newlist;
        }

        public DamageType GetDamageType()
        {
            return new DamageType(this.Type, this.Damage);
        }

        // --------- from game class to our holder ----------

        public static List<SL_Damage> ParseDamageList(DamageList list)
        {
            var damages = new List<SL_Damage>();

            foreach (DamageType type in list.List)
            {
                damages.Add(ParseDamageType(type));
            }

            return damages;
        }

        public static List<SL_Damage> ParseDamageArray(DamageType[] types)
        {
            List<SL_Damage> damages = new List<SL_Damage>();

            foreach (DamageType type in types)
            {
                damages.Add(ParseDamageType(type));
            }

            return damages;
        }

        public static SL_Damage ParseDamageType(DamageType damage)
        {
            return new SL_Damage
            {
                Damage = damage.Damage,
                Type = damage.Type
            };
        }
    }
}
