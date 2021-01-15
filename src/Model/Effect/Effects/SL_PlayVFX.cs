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

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as PlayVFX;

            HitPos = comp.HitPos;
            ParentMode = comp.ParentMode;
            DontInstantiateNew = comp.VfxAlreadyInst;

            if (comp.VFX is VFXSystem vfx && GetVFXSystemEnum(vfx) != VFXPrefabs.NONE)
            {
                VFXPrefab = GetVFXSystemEnum(vfx);
            }
        }

        public enum VFXPrefabs
        {
            NONE,
            BloodLeechTriggerVFX,
            CorruptionSpirit_v_VFXDeathGhost,
            ElemDecayBuffFX,
            ElemEtherealBuffFX,
            ElemFireBuffFX,
            ElemFrostBuffFX,
            ElemLightBuffFX,
            Flamethrower_FXCopper,
            Flamethrower_FXDecay,
            Flamethrower_FXEthereal,
            Flamethrower_FXFire,
            Flamethrower_FXIce,
            Flamethrower_FXLightning,
            GargoyleBoonVFX,
            GrandmotherBoonVFX,
            GrandmotherReanimate_ExplosionFX,
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
            HolyBlazeVFX,
            JinxProjectile_VFXChill,
            JinxProjectile_VFXCurse,
            JinxProjectile_VFXDoom,
            JinxProjectile_VFXHaunt,
            JinxProjectile_VFXScorch,
            LichRustReanimate_ExplosionFX,
            LionManBoonVFX1,
            LionManBoonVFX2,
            NewGhostOneHandedAlly_v_VFXDeathGhost,
            PlagueVFX,
            TormentBlast_NormalChill_VfxChill,
            TormentBlast_NormalConfusion_VfxConfusion,
            TormentBlast_NormalCurse_VFXCurse,
            TormentBlast_NormalDoom_VfxDoom,
            TormentBlast_NormalHaunt_VfxHaunt,
            TormentBlast_NormalPain_VfxPain,
            TormentBlast_NormalScorch_VfxScorch,
            UnerringReadTriggerVFX,
            UnerringReadVFX,
            VFX_RunicHeal,
            VFXAetherBomb,
            VFXAetherBombExplosion,
            VFXAnkleBlow,
            VFXBloodBullet,
            VFXBloodLust,
            VFXBloodLust_Blood,
            VFXBoonBolt,
            VFXBoonDecay,
            VFXBoonEthereal,
            VFXBoonFire,
            VFXBoonIce,
            VFXCageBossMultiStrikeWind,
            VFXCallToElements,
            VFXChakram,
            VFXChakramLong,
            VFXCleanse,
            VFXCounter,
            VFXCrescendo,
            VFXCrimsonMineAnticipations,
            VFXCrimsonMineEliteLvl2Anticipations,
            VFXCrimsonMineLvl2Anticipations,
            VFXDetectSoul,
            VFXDiscipline,
            VFXEvasionShot,
            VFXFinisherBlow,
            VFXForceBubble,
            VFXForceRaise,
            VFXGiftOfBlood,
            VFXGiftOfBloodAlly,
            VFXJuggernaut,
            VFXLeapAttack,
            VFXLifeSyphonHit,
            VFXMaceFillAbsorb,
            VFXMomentOfTruth,
            VFXMoonSwipe,
            VFXMultiStrikeWind,
            VFXNurturingEcho,
            VFXPetrification,
            VFXPetrified,
            VFXPetrifiedStop,
            VFXPiercingShot,
            VFXPreciseStrike,
            VFXRage,
            VFXRuneDeez,
            VFXRuneDezEgoth,
            VFXRuneEgoth,
            VFXRuneEgothDez,
            VFXRuneFal,
            VFXRuneFalShim,
            VFXRuneShim,
            VFXRunicBlade,
            VFXSavageStrikes,
            VFXShieldAbsorb,
            VFXShieldBrace,
            VFXShieldCharge,
            VFXSniperShot,
            VFXSplitter,
            VFXSweepKick,
            VFXTeleport,
            VFXTheTechnique,
            VFXViciousCycle,
            VFXViolentStab,

        }

        // ============ VFXSystem Dictionary ============ //

        private static bool m_initDone = false;

        internal static readonly Dictionary<VFXPrefabs, GameObject> VfxPrefabCache = new Dictionary<VFXPrefabs, GameObject>();

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

            var vfxSystems = Resources.FindObjectsOfTypeAll<VFXSystem>();
            foreach (var vfx in vfxSystems)
            {
                var name = GetVFXSystemEnum(vfx);

                if (name == VFXPrefabs.NONE)
                {
                    SL.Log("Couldn't parse vfx prefab to enum: " + vfx.transform.GetGameObjectPath());
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
        /// Gets the safe name of a VFXSystem (for serialization / enum).
        /// </summary>
        /// <param name="vfx">The VFXSystem to get the name for.</param>
        /// <returns>The actual, serialization-safe name.</returns>
        public static string GetSafeVFXName(VFXSystem vfx)
        {
            var safeName = vfx.transform.GetGameObjectPath().Trim();
            safeName = safeName.Replace("(Clone)", "").Replace(" (1)", "").Replace(" ", "_");
            safeName = Serializer.ReplaceInvalidChars(safeName);
            safeName = safeName.Substring(1, safeName.Length - 1);
            return safeName;
        }

        /// <summary>
        /// Helper to take a VFXSystem and get the VFXSystemPrefabs enum value for it (if valid).
        /// </summary>
        /// <param name="vfx">The vfx system</param>
        public static VFXPrefabs GetVFXSystemEnum(VFXSystem vfx)
        {
            if (Enum.TryParse(GetSafeVFXName(vfx), out VFXPrefabs name))
            {
                return name;
            }

            SL.Log("PlayVFX: could not get name for " + vfx.name);

            return VFXPrefabs.NONE;
        }

        public static void DebugVfxNames()
        {
            SL.Log("----------- VFXSystems ------------ ");
            var vfxsystems = Resources.FindObjectsOfTypeAll<VFXSystem>();
            var names = new List<string>();
            foreach (var vfx in vfxsystems)
            {
                var safename = GetSafeVFXName(vfx);
                if (!names.Contains(safename))
                {
                    names.Add(safename);
                }
            }
            File.WriteAllLines("vfxsystems.txt", names.ToArray());
        }
    }
}
