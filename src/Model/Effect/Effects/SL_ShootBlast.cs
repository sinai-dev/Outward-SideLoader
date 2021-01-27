using System;
using System.Collections.Generic;
using System.IO;
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
        public bool PlaySounOnRefresh;
        public float DelayFirstShoot;
        public bool PlayFXOnRefresh;

        public EditBehaviours EffectBehaviour = EditBehaviours.Override;
        public SL_EffectTransform[] BlastEffects;

        public override void ApplyToComponent<T>(T component)
        {
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
                newBlast.DontPlayHitSound = this.DontPlayHitSound;
                newBlast.PlayFXOnRefresh = this.PlayFXOnRefresh;
                newBlast.DelayFirstShoot = this.DelayFirstShoot;

                newBlast.ImpactSoundMaterial = this.ImpactSoundMaterial;
                if (newBlast.GetComponentInChildren<ImpactSoundPlayer>() is ImpactSoundPlayer player)
                {
                    player.SoundMaterial = this.ImpactSoundMaterial;
                }

                SL_EffectTransform.ApplyTransformList(newBlast.transform, BlastEffects, EffectBehaviour);

                if (newBlast is BlastDelayedHits delayedBlast)
                {
                    var conditionChilds = new List<Transform>();
                    foreach (Transform child in delayedBlast.transform)
                    {
                        if (child.GetComponent<EffectCondition>())
                        {
                            conditionChilds.Add(child);
                        }
                    }

                    var list = new List<BlastDelayedHits.SplitCondition>();
                    foreach (var child in conditionChilds)
                    {
                        var split = new BlastDelayedHits.SplitCondition
                        {
                            ConditionHolder = child
                        };
                        split.Init();
                        list.Add(split);
                    }
                    delayedBlast.EffectsPerCondition = list.ToArray();

                    if (delayedBlast.transform.Find("RevealedSoul") is Transform soulTransform)
                    {
                        var soulFX = new BlastDelayedHits.SplitCondition
                        {
                            ConditionHolder = soulTransform,
                        };
                        soulFX.Init();
                        delayedBlast.RevealSoulEffects = soulFX;
                    }
                }
            }
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as ShootBlast;

            if (comp.BaseBlast is Blast blast && GetBlastPrefabEnum(blast) != BlastPrefabs.NONE)
            {
                BaseBlast = GetBlastPrefabEnum(blast);
                AffectHitTargetCenter = blast.AffectHitTargetCenter;
                DontPlayHitSound = blast.DontPlayHitSound;
                FXIsWorld = blast.FXIsWorld;
                HitOnShoot = blast.HitOnShoot;
                IgnoreShooter = blast.IgnoreShooter;
                ImpactSoundMaterial = blast.ImpactSoundMaterial;
                Interruptible = blast.Interruptible;
                MaxHitTargetCount = blast.MaxHitTargetCount;
                Radius = blast.Radius;
                RefreshTime = blast.RefreshTime;
                DontPlayHitSound = blast.DontPlayHitSound;
                PlayFXOnRefresh = blast.PlayFXOnRefresh;
                DelayFirstShoot = blast.DelayFirstShoot;

                BlastLifespan = comp.BlastLifespan;
                IgnoreStop = comp.IgnoreStop;
                InstantiatedAmount = comp.InstanstiatedAmount;
                NoTargetForwardMultiplier = comp.NoTargetForwardMultiplier;
                ParentToShootTransform = comp.ParentToShootTransform;
                UseTargetCharacterPositionType = comp.UseTargetCharacterPositionType;

                if (blast.transform.childCount > 0)
                {
                    var list = new List<SL_EffectTransform>();
                    foreach (Transform child in blast.transform)
                    {
                        var effectsChild = SL_EffectTransform.ParseTransform(child);

                        if (effectsChild.HasContent)
                        {
                            list.Add(effectsChild);
                        }
                    }
                    BlastEffects = list.ToArray();
                }
            }
            else if (comp.BaseBlast)
            {
                SL.Log("Couldn't parse blast prefab to enum: " + comp.BaseBlast.name);
            }
        }

        // ============ Blasts Dictionary ============ //

        private static bool m_initDone = false;

        internal static readonly Dictionary<BlastPrefabs, GameObject> BlastPrefabCache = new Dictionary<BlastPrefabs, GameObject>();

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
            var prefabName = blast.name.Replace("(Clone)", "").Replace(" ", "_").Trim();

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
            AetherbombBlast,
            AncientDwellerSong,
            AncientDwellerWaile,
            AshGiantAsh,
            AshPriestExplosion,
            BeetleFrostIceBlast,
            BeetleFrostSpreadBlast,
            BlazeLinger,
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
            ChimesDoomBlast,
            ChimesHealBlast,
            ChimesReverbBlastLarge,
            ChimesReverbBlastMedium,
            ChimesReverbBlastSmall,
            ChimesRhythmBlast,
            ChrimsonLaser,
            ColdBlast_HiddenTrap,
            ColdBlast_Trap,
            ConsumeSoulBlast,
            CrimsonBlast,
            CrimsonBlastPowered,
            CrimsonCircleMineBlast,
            CrimsonEliteBlast,
            CrimsonEliteBlastPowered,
            CrimsonEliteBlastPowered2,
            CrimsonEliteBlastSunBig,
            CrimsonEliteCircleMineBlast,
            CrimsonEliteCircleMineBlastDark,
            CrimsonEliteLaser,
            DecayBlast_Enchantment,
            Dispersion,
            DispersionBolt,
            DispersionDecay,
            DispersionEthereal,
            DispersionFire,
            DispersionFrost,
            DispersionLight,
            DispersionPoison,
            DispersionWind,
            DjinBigMeleetBlast,
            DjinBigMeleetBlastBig,
            DjinBlast,
            DjinLingerBlast,
            DrumHauntedBlast,
            DrumHealBlast,
            DrumHealBlastSound,
            DrumReverbBlastLarge,
            DrumReverbBlastMedium,
            DrumReverbBlastSmall,
            DrumRhythmBlast,
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
            ExplosiveArrowBlast,
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
            GargoyleBlast,
            GargoyleBoonFX,
            GargoyleUrnQuake,
            Gateblast,
            GepEtherealBlast,
            GhostSoulSyphon,
            GiantHorrorAsh,
            GiantHorrorAshBlast,
            GiantHorrorDecayThrower,
            GiantHunterAsh,
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
            GrandmotherReanimate,
            GrenadeBlast,
            GrenadeBlazingBlast,
            GrenadeFrostBlast,
            GrenadeIncendiaryBlast,
            GrenadeNapalmBlast,
            GrenadeNerveBlast,
            GrenadeShrapnelBlast,
            GrenadeSparkBlast,
            GrenadeToxinBlast,
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
            HyppoElectricthrower,
            HyppoEtherealthrower,
            HyppoFlamethrower,
            HyppoIcethrower,
            HyppoPoisonthrower,
            IceWitchIceBlast,
            IlluminatorMineLingerBlast,
            IncendiaryBlast_Trap,
            IncendiaryBlastImproved_Trap,
            InstrumentReverbBlast,
            JellyBlast,
            JellyGreenMorphingProjectileBlast,
            JellyGreenMorphingProjectilePreBlast,
            JellyGroundBlast,
            JellyGroundSpawn,
            JellyLaser,
            JellyPreBlast,
            KickIceBlast,
            KindleBlast,
            KrypteiaGrenadeToxinBlast,
            KrypteiaMageDispertion,
            KrypteiaMineExplosion,
            KrypteiaThunder,
            KrypteiaTunderHit,
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
            LionmanBlast,
            LionmanPreBlast,
            MantisManaExplosion,
            Monster_1DamageBlast_BoltElectric,
            Monster_1DamageBlast_Fire,
            Monster_1DamageBlast_IceFrost,
            MyrmQuake,
            NerveGasBlast_HiddenTrap,
            NerveGasBlast_Trap,
            PhytosaurPollen,
            PhytosaurPollenFloral,
            PlagueBlast,
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
            ScarletEmissaryBlast,
            ScarletEmissaryLifeSteal,
            ShieldAbsorbDecayBlast,
            ShieldAbsorbEtherealBlast,
            ShieldAbsorbFireBlast,
            ShieldAbsorbIceBlast,
            ShieldAbsorbLightningBlast,
            ShockBlast_HiddenTrap,
            ShockBlast_Trap,
            ShrimpExplosion,
            SlugHellCrystalProtectiveCloud,
            SlugHellCrystalProtectiveCloudBig,
            SlugHellCrystalProtectiveCloudBigger,
            SlughellDecayBlast,
            SlugHellFireBast,
            SlugHellHealFX,
            SlugHellIceBlast,
            Spark,
            SparkShockBlast,
            SpecterRangeFlamethrower,
            Spikeblast_Trap,
            SpikeblastImproved_Trap,
            SpikeblastWeak_Trap,
            SupremeShellLazer,
            SupremeShellLeapBlast,
            SupremeShellRageBlast,
            TorcrabBlast,
            TorcrabGiantFireBlast,
            TorcrabGiantFireBlastLinger,
            TorcrabGiantFireBlastLingerHigh,
            TorcrabJump,
            TorcrabJumpLarge,
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
            SL.Log("----------- BLASTS ------------ ");
            var blasts = Resources.FindObjectsOfTypeAll<Blast>();
            var names = new List<string>();
            foreach (var blast in blasts)
            {
                if (!names.Contains(blast.name))
                {
                    names.Add(blast.name);
                    //SL.Log(blast.name + ",");
                }
            }
            File.WriteAllLines("blasts.txt", names.ToArray());
        }
    }
}
