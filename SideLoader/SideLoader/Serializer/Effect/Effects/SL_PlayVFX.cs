using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_PlayVFX : SL_Effect
    {
        public VFXPrefabs VFXPrefab;

        public bool HitPos;
        public PlayVFX.e_ParentMode ParentMode;
        public bool DontInstantiateNew;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as PlayVFX;

            comp.HitPos = this.HitPos;
            comp.ParentMode = this.ParentMode;
            comp.VfxAlreadyInst = this.DontInstantiateNew;

            if (GetVfxSystem(this.VFXPrefab) is GameObject vfxPrefab)
            {
                var copy = GameObject.Instantiate(vfxPrefab);
                GameObject.DontDestroyOnLoad(copy);
                copy.SetActive(false);

                comp.VFX = copy.GetComponent<VFXSystem>();
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var comp = effect as PlayVFX;
            var template = holder as SL_PlayVFX;

            template.HitPos = comp.HitPos;
            template.ParentMode = comp.ParentMode;
            template.DontInstantiateNew = comp.VfxAlreadyInst;

            if (comp.VFX is VFXSystem vfx && GetVFXSystemEnum(vfx) != VFXPrefabs.NONE)
            {
                template.VFXPrefab = GetVFXSystemEnum(vfx);
            }
        }

        public enum VFXPrefabs
        {
            NONE,
            Blood,
            BloodLeechTriggerVFX,
            ElemDecayBuffFX,
            ElemEtherealBuffFX,
            ElemFireBuffFX,
            ElemFrostBuffFX,
            ElemLightBuffFX,
            ExplosionFX,
            FXCopper,
            FXDecay,
            FXEthereal,
            FXFire,
            FXIce,
            FXLightning,
            HexBleedingVFX,
            HexBurningVFX,
            HexChillVFX,
            HexConfusionVFX,
            HexCurseVFX,
            HexDoomVFX,
            HexHauntedVFX,
            HexPainVFX,
            HexPoisonNewFX,
            HexPoisonVFX,
            HexSappedVFX,
            HexScorchVFX,
            HexWeakenVFX,
            UnerringReadTriggerVFX,
            UnerringReadVFX,
            VFX_RunicHeal,
            VFXAnkleBlow,
            VFXBloodBullet,
            VFXBloodLust,
            VFXBoonBolt,
            VFXBoonDecay,
            VFXBoonEthereal,
            VFXBoonFire,
            VFXBoonIce,
            VFXCageBossMultiStrikeWind,
            VFXCallToElements,
            VFXChakram,
            VFXChakramLong,
            VfxChill,
            VFXCleanse,
            VfxConfusion,
            VFXCounter,
            VFXCurse,
            VFXDeathGhost,
            VFXDetectSoul,
            VFXDiscipline,
            VfxDoom,
            VFXEvasionShot,
            VFXFinisherBlow,
            VFXForceBubble,
            VFXForceRaise,
            VFXGiftOfBlood,
            VFXGiftOfBloodAlly,
            VfxHaunt,
            VFXJuggernaut,
            VFXLeapAttack,
            VFXLifeSyphonHit,
            VFXMaceFillAbsorb,
            VFXMoonSwipe,
            VFXMultiStrikeWind,
            VfxPain,
            VFXPiercingShot,
            VFXPreciseStrike,
            VFXRage,
            VFXRuneDeez,
            VFXRuneEgoth,
            VFXRuneFal,
            VFXRuneShim,
            VFXRunicBlade,
            VFXSavageStrikes,
            VfxScorch,
            VFXShieldAbsorb,
            VFXShieldBrace,
            VFXShieldCharge,
            VFXSniperShot,
            VFXSweepKick,
            VFXTeleport,
            VFXViolentStab
        }



        // ============ VFXSystem Dictionary ============ //

        private static bool m_initDone = false;

        private static readonly Dictionary<VFXPrefabs, GameObject> VfxPrefabCache = new Dictionary<VFXPrefabs, GameObject>();

        public static GameObject GetVfxSystem(VFXPrefabs name)
        {
            if (name != VFXPrefabs.NONE)
            {
                return VfxPrefabCache[name];
            }
            else
            {
                return null;
            }
        }

        public static void BuildPrefabDictionary()
        {
            if (m_initDone)
            {
                return;
            }

            foreach (var vfx in Resources.FindObjectsOfTypeAll<VFXSystem>())
            {
                var name = GetVFXSystemEnum(vfx);

                if (name == VFXPrefabs.NONE)
                {
                    SL.Log("Couldn't parse vfx prefab to enum: " + vfx.name);
                }
                else if (!VfxPrefabCache.ContainsKey(name))
                {
                    bool wasActive = vfx.gameObject.activeSelf;
                    vfx.gameObject.SetActive(false);

                    var copy = GameObject.Instantiate(vfx.gameObject);
                    GameObject.DontDestroyOnLoad(copy);
                    copy.SetActive(false);

                    VfxPrefabCache.Add(name, copy);

                    vfx.gameObject.SetActive(wasActive);
                }
            }

            m_initDone = true;
        }

        /// <summary>
        /// Helper to take a VFXSystem and get the VFXSystemPrefabs enum value for it (if valid).
        /// </summary>
        /// <param name="vfx">The vfx system</param>
        public static VFXPrefabs GetVFXSystemEnum(VFXSystem vfx)
        {
            var prefabName = vfx.name.Replace("(Clone)", "").Trim();
            prefabName = prefabName.Replace(" (1)", "");

            if (Enum.TryParse(prefabName, out VFXPrefabs name))
            {
                return name;
            }
            return VFXPrefabs.NONE;
        }

        public static void DebugVfxNames()
        {
            Debug.Log("----------- VFXSystems ------------ ");
            var vfxsystems = Resources.FindObjectsOfTypeAll<VFXSystem>();
            var names = new List<string>();
            foreach (var vfx in vfxsystems)
            {
                if (!names.Contains(vfx.name))
                {
                    names.Add(vfx.name);
                }
            }
            File.WriteAllLines("vfxsystems.txt", names.ToArray());
        }
    }
}
