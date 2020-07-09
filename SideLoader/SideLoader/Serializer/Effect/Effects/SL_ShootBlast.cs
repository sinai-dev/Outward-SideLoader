using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ShootBlast : SL_Shooter
    {
        public BlastPrefabs BaseBlast;
        public float Radius;
        public float RefreshTime;
        public float BlastLifespan;
        public int InstantiatedAmount;

        public bool Interruptible;
        public int MaxHitTargetCount;
        public bool AffectHitTargetCenter;
        public bool HitOnShoot;
        public bool IgnoreShooter;

        public bool IgnoreStop;
        public float NoTargetForwardMultiplier;
        public bool ParentToShootTransform;
        public bool UseTargetCharacterPositionType;

        public EquipmentSoundMaterials ImpactSoundMaterial;
        public bool DontPlayHitSound;
        public bool FXIsWorld;

        public EditBehaviours EffectBehaviour = EditBehaviours.Override;
        public List<SL_EffectTransform> BlastEffects = new List<SL_EffectTransform>();

        public override void ApplyToComponent<T>(T component)
        {
            if (this.EffectBehaviour == EditBehaviours.DestroyEffects)
            {
                SL.Log("EditBehaviours.DestroyEffects is deprecated. Use EditBehaviours.Destroy instead.");
                this.EffectBehaviour = EditBehaviours.Destroy;
            }
            else if (this.EffectBehaviour == EditBehaviours.OverrideEffects)
            {
                SL.Log("EditBehaviours.OverrideEffects is deprecated. Use EditBehaviours.Override instead.");
                this.EffectBehaviour = EditBehaviours.Override;
            }

            base.ApplyToComponent(component);

            var comp = component as ShootBlast;

            if (GetBlastPrefab(this.BaseBlast) is GameObject baseBlast)
            {
                var copy = GameObject.Instantiate(baseBlast);
                GameObject.DontDestroyOnLoad(copy);
                copy.SetActive(false);

                var newBlast = copy.GetComponent<Blast>();
                comp.BaseBlast = newBlast;

                comp.BlastLifespan = this.BlastLifespan;
                comp.IgnoreStop = this.IgnoreStop;
                comp.InstanstiatedAmount = this.InstantiatedAmount;
                comp.LocalCastPositionAdd = this.LocalPositionAdd;
                comp.NoAim = this.NoAim;
                comp.NoTargetForwardMultiplier = this.NoTargetForwardMultiplier;
                comp.ParentToShootTransform = this.ParentToShootTransform;
                comp.UseTargetCharacterPositionType = this.UseTargetCharacterPositionType;

                newBlast.AffectHitTargetCenter = this.AffectHitTargetCenter;
                newBlast.DontPlayHitSound = this.DontPlayHitSound;
                newBlast.FXIsWorld = this.FXIsWorld;
                newBlast.HitOnShoot = this.HitOnShoot;
                newBlast.IgnoreShooter = this.IgnoreShooter;
                newBlast.Interruptible = this.Interruptible;
                newBlast.MaxHitTargetCount = this.MaxHitTargetCount;
                newBlast.Radius = this.Radius;
                newBlast.RefreshTime = this.RefreshTime;

                newBlast.ImpactSoundMaterial = this.ImpactSoundMaterial;
                if (newBlast.GetComponentInChildren<ImpactSoundPlayer>() is ImpactSoundPlayer player)
                {
                    player.SoundMaterial = this.ImpactSoundMaterial;
                }

                SL_EffectTransform.ApplyTransformList(newBlast.transform, BlastEffects, EffectBehaviour);
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            var template = holder as SL_ShootBlast;
            var comp = effect as ShootBlast;

            if (comp.BaseBlast is Blast blast && GetBlastPrefabEnum(blast) != BlastPrefabs.NONE)
            {
                template.BaseBlast = GetBlastPrefabEnum(blast);
                template.AffectHitTargetCenter = blast.AffectHitTargetCenter;
                template.DontPlayHitSound = blast.DontPlayHitSound;
                template.FXIsWorld = blast.FXIsWorld;
                template.HitOnShoot = blast.HitOnShoot;
                template.IgnoreShooter = blast.IgnoreShooter;
                template.ImpactSoundMaterial = blast.ImpactSoundMaterial;
                template.Interruptible = blast.Interruptible;
                template.MaxHitTargetCount = blast.MaxHitTargetCount;
                template.Radius = blast.Radius;
                template.RefreshTime = blast.RefreshTime;

                template.BlastLifespan = comp.BlastLifespan;
                template.IgnoreStop = comp.IgnoreStop;
                template.InstantiatedAmount = comp.InstanstiatedAmount;
                template.NoTargetForwardMultiplier = comp.NoTargetForwardMultiplier;
                template.ParentToShootTransform = comp.ParentToShootTransform;
                template.UseTargetCharacterPositionType = comp.UseTargetCharacterPositionType;

                foreach (Transform child in blast.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                    {
                        template.BlastEffects.Add(effectsChild);
                    }
                }
            }
            else if (comp.BaseBlast)
            {
                SL.Log("Couldn't parse blast prefab to enum: " + comp.BaseBlast.name);
            }
        }

        // ============ Blasts Dictionary ============ //

        private static bool m_initDone = false;

        private static readonly Dictionary<BlastPrefabs, GameObject> BlastPrefabCache = new Dictionary<BlastPrefabs, GameObject>();

        public static GameObject GetBlastPrefab(BlastPrefabs name)
        {
            if (name != BlastPrefabs.NONE)
            {
                return BlastPrefabCache[name];
            }
            else
            {
                return null;
            }
        }

        public static void BuildBlastsDictionary()
        {
            if (m_initDone)
            {
                return;
            }

            foreach (var blast in Resources.FindObjectsOfTypeAll<Blast>())
            {
                var name = GetBlastPrefabEnum(blast);

                if (name == BlastPrefabs.NONE)
                {
                    SL.Log("Couldn't parse blast prefab to enum: " + blast.name);
                }
                else if (!BlastPrefabCache.ContainsKey(name))
                {
                    bool wasActive = blast.gameObject.activeSelf;
                    blast.gameObject.SetActive(false);

                    var copy = GameObject.Instantiate(blast.gameObject);
                    GameObject.DontDestroyOnLoad(copy);
                    copy.SetActive(false);

                    BlastPrefabCache.Add(name, copy);

                    blast.gameObject.SetActive(wasActive);
                }
            }

            m_initDone = true;
        }

        /// <summary>
        /// Helper to take a Blast and get the BlastPrefabs enum value for it (if valid).
        /// </summary>
        /// <param name="blast">The blast prefab</param>
        public static BlastPrefabs GetBlastPrefabEnum(Blast blast)
        {
            var prefabName = blast.name.Replace("(Clone)", "").Trim();

            if (Enum.TryParse(prefabName, out BlastPrefabs name))
            {
                return name;
            }
            else
            {
                return BlastPrefabs.NONE;
            }
        }

        public enum BlastPrefabs
        {
            NONE,
            AshGiantAsh,
            AshPriestExplosion,
            Bleedblast_Trap,
            BleedblastImproved_Trap,
            Bludgeonblast_Trap,
            BoltElectricBlast,
            BoozuDecayBlast,
            BoozuProudBeastBlast,
            BoozuProudBeastBlastBig,
            BubbleIceBlast,
            BulletBloodSyphonBlast,
            BulletIceBlast,
            BurningBlast,
            BurningManFlamethrower,
            CageBossBlastBig,
            CageBossBlastSmall,
            CageBossPunch,
            ColdBlast_HiddenTrap,
            ColdBlast_Trap,
            ConsumeSoulBlast,
            DecayBlast_Enchantment,
            ElectricBlast_Enchantment,
            ElementalParasiteLingerBlast,
            EliteAshGiantAsh,
            EliteBurningBlast,
            EliteBurningManForwardFlame,
            EliteCalixaGongTrike,
            EliteImmaculateBlast,
            EliteImmaculateLaser,
            EliteShrimpBigBlast,
            EliteShrimpExplosion,
            EliteSupremeShellLazer,
            EliteSupremeShellSpecialLaser,
            EliteTrogGrenadeBlast,
            EliteTuanosaurFireblast,
            EliteTuanosaurFlamethrower,
            EtherealBlast_Enchantment,
            EtherealBlast_HiddenTrap,
            EtherealBlast_RunicBlast,
            EtherealBlast_RunicBlastAmplified,
            EtherealBlast_RunicTrap,
            EtherealBlast_RunicTrapAmplified,
            EtherealBlast_Trap,
            Fireblast,
            FireBlast_Enchantment,
            FireblastBeetle,
            FireLinger,
            FireWall,
            Fireworks,
            Flamethrower,
            FlashBlast_HiddenTrap,
            FlashBlast_Trap,
            FlintKindleBlast,
            ForcePush,
            ForceRaiseLightning,
            ForgeGolemRustLichMinionElectricblast,
            ForgeGolemRustLichMinionFlamethrower,
            FrostBlast_Enchantment,
            Gateblast,
            GhostSoulSyphon,
            GiantHorrorAsh,
            GiantHorrorAshBlast,
            GiantHorrorDecayThrower,
            GiftOfBloodAllyBlast,
            GolemShieldedLingerBlast,
            GolemShieldedMortarBlast,
            GongStrikeDecay,
            GongStrikeFire,
            GongStrikeFrost,
            GongStrikeLight,
            GongStrikeLightning,
            GongStrikePoison,
            GongStrikeSpiritual,
            GongStrikeWind,
            HexTouchChillBlast,
            HexTouchCurseBlast,
            HexTouchDoomBlast,
            HexTouchHauntBlast,
            HexTouchScorchBlast,
            HiddenBonusDamageBlast_Trap,
            HornetBurst,
            HornetHeal,
            HornetLargeSwarm,
            HornetSmallSwarm,
            IceWitchIceBlast,
            IlluminatorMineLingerBlast,
            IncendiaryBlast_Trap,
            IncendiaryBlastImproved_Trap,
            KickIceBlast,
            KindleBlast,
            LeapAttackBlast,
            LeapAttackBlastSagar,
            LichGoldLightBlast,
            LichGoldMinionBoltSoulSyphon,
            LichGoldStaffBlast,
            LichJadeCircleMineBlast,
            LichJadeTentacleBlast,
            LichRustOrderProj_0,
            LichRustOrderProj_1,
            LichRustOrderProj_2,
            LichRustProjectileEtherealExplosion,
            LichRustReanimate,
            LichRustTeleportBlast,
            LightLichDamageBlast_BoltElectric,
            MantisManaExplosion,
            Monster_1DamageBlast_BoltElectric,
            Monster_1DamageBlast_Fire,
            Monster_1DamageBlast_IceFrost,
            NerveGasBlast_HiddenTrap,
            NerveGasBlast_Trap,
            PhytosaurPollen,
            PhytosaurPollenFloral,
            PureIlluminatorExplosion,
            PureIlluminatorExplosion2,
            PureIlluminatorExplosion3,
            PureIlluminatorMineLingerBlast,
            RunicBeastBlast,
            RuptureBlast,
            RuptureBlast_Chill,
            RuptureBlast_Confusion,
            RuptureBlast_Curse,
            RuptureBlast_Doom,
            RuptureBlast_Haunt,
            RuptureBlast_Pain,
            RuptureBlast_RevealedSoul,
            RuptureBlast_Scorch,
            SappedBlast_Trap,
            ShieldAbsorbDecayBlast,
            ShieldAbsorbEtherealBlast,
            ShieldAbsorbFireBlast,
            ShieldAbsorbIceBlast,
            ShieldAbsorbLightningBlast,
            ShockBlast_HiddenTrap,
            ShockBlast_Trap,
            ShrimpExplosion,
            Spark,
            SparkShockBlast,
            SpecterRangeFlamethrower,
            Spikeblast_Trap,
            SpikeblastImproved_Trap,
            SpikeblastWeak_Trap,
            SupremeShellLazer,
            SupremeShellLeapBlast,
            SupremeShellRageBlast,
            TormentBlast,
            ToxicBlast_Trap,
            ToxicBlastImproved_Trap,
            TrogGrenadeBlast,
            TuanosaurExplosion,
            VendavelWitchElectricBlast,
            VendavelWitchKickIceBlast,
            WendigoSoulSyphon,

        }

        public static void DebugBlastNames()
        {
            Debug.Log("----------- BLASTS ------------ ");
            var blasts = Resources.FindObjectsOfTypeAll<Blast>();
            var names = new List<string>();
            foreach (var blast in blasts)
            {
                if (!names.Contains(blast.name))
                {
                    names.Add(blast.name);
                    //Debug.Log(blast.name + ",");
                }
            }
            File.WriteAllLines("blasts.txt", names.ToArray());
        }
    }
}
