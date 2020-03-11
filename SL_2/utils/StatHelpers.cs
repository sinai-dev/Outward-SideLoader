using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SideLoader_2
{
    public static class StatHelpers
    {
        // -------------------------- ITEM STAT HELPER FUNCTIONS ------------------------

        /// <summary>
        /// Set the appropriate scaled damage depending on the attack ID (which part of combo)<br></br>
        /// </summary>
        /// <param name="type">Type of Weapon being used</param>
        /// <param name="attackID">0 and 1 are Light, 2 is Special, 3 and 4 are Combo</param>
        /// <param name="stepDamage">List of damage (float) in order. See WeaponStats.AttackData.Damage</param>
        /// <param name="stepImpact">Base Impact of the Weapon</param>

        public static void SetScaledDamages(Weapon.WeaponType type, int attackID, ref List<float> stepDamage, ref float stepImpact)
        {
            float dmgMulti = 1.0f;
            float impactMulti = 1.0f;

            switch (type)
            {
                // cases 0 and 1 are ignored because light attacks always use 1.0f modifiers. 2 is heavy attack, 3 and 4 are the combo attacks.
                case Weapon.WeaponType.Sword_1H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.1f;
                            impactMulti = 1.1f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Axe_1H:
                    switch (attackID)
                    {
                        case 2:
                        case 3:
                        case 4:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Mace_1H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.3f;
                            impactMulti = 2.5f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Sword_2H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.5f;
                            impactMulti = 1.5f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.1f;
                            impactMulti = 1.1f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Axe_2H:
                    switch (attackID)
                    {
                        case 2:
                        case 3:
                        case 4:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Mace_2H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 0.75f;
                            impactMulti = 2.0f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.4f;
                            impactMulti = 1.4f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Spear_2H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.4f;
                            impactMulti = 1.2f;
                            break;
                        case 3:
                            dmgMulti = 1.3f;
                            impactMulti = 1.2f;
                            break;
                        case 4:
                            dmgMulti = 1.2f;
                            impactMulti = 1.1f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Halberd_2H:
                    switch (attackID)
                    {
                        case 2:
                        case 3:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                        case 4:
                            dmgMulti = 1.7f;
                            impactMulti = 1.7f;
                            break;
                    }
                    break;
                default:
                    break;
            }

            for (int i = 0; i < stepDamage.Count(); i++)
            {
                stepDamage[i] *= dmgMulti;
            }

            stepImpact *= impactMulti;

            return;
        }

        /// <summary>
        /// Pretty-prints the Damage Types, in the order of DamageType.Types
        /// </summary>
        public static string[] DamageNames = new string[10]
        {
            "Physical",
            "Ethereal",
            "Decay",
            "Lightning",
            "Frost",
            "Fire",
            "DarkOLD",
            "LightOLD",
            "Raw",
            "None"
        };

        /// <summary>
        /// Pretty-prints the Weapon Types from dictionary
        /// </summary>
        public static Dictionary<Weapon.WeaponType, string> weaponTypes = new Dictionary<Weapon.WeaponType, string>() {
            { Weapon.WeaponType.Sword_1H,   "1H Sword" },
            { Weapon.WeaponType.Axe_1H,     "1H Axe" },
            { Weapon.WeaponType.Mace_1H,    "1H Mace" },
            { Weapon.WeaponType.Sword_2H,   "2H Sword" },
            { Weapon.WeaponType.Axe_2H,     "2H Axe" },
            { Weapon.WeaponType.Mace_2H,    "2H Mace" },
            { Weapon.WeaponType.Halberd_2H ,"Polearm" },
            { Weapon.WeaponType.Spear_2H ,  "Spear" },
            { Weapon.WeaponType.Shield ,    "Shield" }, 
            { Weapon.WeaponType.Bow ,       "Bow" },
            { Weapon.WeaponType.Pistol_OH , "Pistol" }, 
            { Weapon.WeaponType.Chakram_OH ,"Chakram" },
            { Weapon.WeaponType.Dagger_OH,  "Dagger" },
        };

        /// <summary>
        /// Status Identifiers which can be used as a Hit Effect
        /// </summary>
        public static List<string> StatusNames = new List<string>()
        {
            "Bleeding",
            "Bleeding +",
            "Burning",
            "Poisoned",
            "Poisoned +",
            "Burn",
            "Chill",
            "Curse",
            "Elemental Vulnerability",
            "Haunted",
            "Doom",
            "Pain",
            "Confusion",
            "Dizzy",
            "Cripped",
            "Slow Down",
        };
    }
}
