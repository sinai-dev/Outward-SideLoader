using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader_2
{
    public class DamageHolder
    {
        public float Damage;
        public string Damage_Type;

        public static List<DamageHolder> ParseDamageList(DamageList list)
        {
            var damages = new List<DamageHolder>();

            foreach (DamageType type in list.List)
            {
                damages.Add(ParseDamageType(type));
            }

            return damages;
        }

        public static List<DamageHolder> ParseDamageArray(DamageType[] types)
        {
            List<DamageHolder> damages = new List<DamageHolder>();

            foreach (DamageType type in types)
            {
                damages.Add(ParseDamageType(type));
            }

            return damages;
        }

        public static DamageHolder ParseDamageType(DamageType damage)
        {
            return new DamageHolder
            {
                Damage = damage.Damage,
                Damage_Type = damage.Type.ToString()
            };
        }
    }
}
