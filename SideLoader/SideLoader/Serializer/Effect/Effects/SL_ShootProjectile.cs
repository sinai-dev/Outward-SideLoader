using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_ShootProjectile : SL_Shooter
    {
        public ProjectilePrefabs BaseProjectile;
        public List<SL_ProjectileShot> ProjectileShots = new List<SL_ProjectileShot>();

        public float Lifespan;
        public float LateShootTime;
        public bool Unblockable;
        public bool EffectsOnlyIfHitCharacter;
        public Projectile.EndLifeMode EndMode;
        public bool DisableOnHit;
        public bool IgnoreShooterCollision;

        public ShootProjectile.TargetMode TargetingMode;
        public int TargetCountPerProjectile;
        public float TargetRange;
        public bool AutoTarget;
        public float AutoTargetMaxAngle;
        public float AutoTargetRange;

        public float ProjectileForce;
        public Vector3 AddDirection;
        public Vector3 AddRotationForce;
        public float YMagnitudeAffect;
        public float YMagnitudeForce;
        public float DefenseLength;
        public float DefenseRange;

        public int PhysicsLayerMask;
        public bool OnlyExplodeOnLayerMask;

        public EquipmentSoundMaterials ImpactSoundMaterial;
        public Vector2 LightIntensityFade;
        public Vector3 PointOffset;
        public bool TrailEnabled;
        public float TrailTime;

        public EffectBehaviours EffectBehaviour = EffectBehaviours.OverrideEffects;
        public List<SL_EffectTransform> ProjectileEffects = new List<SL_EffectTransform>();

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as ShootProjectile;

            if (GetProjectilePrefab(this.BaseProjectile) is GameObject projectile)
            {
                var copy = GameObject.Instantiate(projectile);
                GameObject.DontDestroyOnLoad(copy);
                copy.SetActive(false);

                var newProjectile = copy.GetComponent<Projectile>();
                comp.BaseProjectile = newProjectile;

                comp.AddDirection = this.AddDirection;
                comp.AddRotationForce = this.AddRotationForce;
                comp.AutoTarget = this.AutoTarget;
                comp.AutoTargetMaxAngle = this.AutoTargetMaxAngle;
                comp.AutoTargetRange = this.AutoTargetRange;
                comp.IgnoreShooterCollision = this.IgnoreShooterCollision;
                comp.ProjectileForce = this.ProjectileForce;
                comp.TargetCountPerProjectile = this.TargetCountPerProjectile;
                comp.TargetingMode = this.TargetingMode;
                comp.TargetRange = this.TargetRange;
                comp.YMagnitudeAffect = this.YMagnitudeAffect;
                comp.YMagnitudeForce = this.YMagnitudeForce;

                var list = new List<ProjectileShot>();
                foreach (var shot in this.ProjectileShots)
                {
                    list.Add(new ProjectileShot()
                    {
                        RandomLocalDirectionAdd = shot.RandomLocalDirectionAdd,
                        LocalDirectionOffset = shot.LocalDirectionOffset,
                        LockDirection = shot.LockDirection,
                        MustShoot = shot.MustShoot,
                        NoBaseDir = shot.NoBaseDir
                    });
                }
                comp.ProjectileShots = list.ToArray();

                newProjectile.DefenseLength = this.DefenseLength;
                newProjectile.DefenseRange = this.DefenseRange;
                newProjectile.DisableOnHit = this.DisableOnHit;
                newProjectile.EffectsOnlyIfHitCharacter = this.EffectsOnlyIfHitCharacter;
                newProjectile.EndMode = this.EndMode;
                newProjectile.OnlyExplodeOnLayers = this.OnlyExplodeOnLayerMask;
                newProjectile.ExplodeOnContactWithLayers.value = this.PhysicsLayerMask;
                newProjectile.LateShootTime = this.LateShootTime;
                newProjectile.Lifespan = this.Lifespan;
                newProjectile.LightIntensityFade = this.LightIntensityFade;
                newProjectile.PointOffset = this.PointOffset;
                newProjectile.TrailEnabled = this.TrailEnabled;
                newProjectile.TrailTime = this.TrailTime;
                newProjectile.Unblockable = this.Unblockable;

                newProjectile.ImpactSoundMaterial = this.ImpactSoundMaterial;
                if (newProjectile.GetComponentInChildren<ImpactSoundPlayer>() is ImpactSoundPlayer player)
                {
                    player.SoundMaterial = this.ImpactSoundMaterial;
                }

                SL_EffectTransform.ApplyTransformList(newProjectile.transform, ProjectileEffects, EffectBehaviour);
            }
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            base.SerializeEffect(effect, holder);

            var template = holder as SL_ShootProjectile;
            var comp = effect as ShootProjectile;

            if (comp.BaseProjectile is Projectile projectile && GetProjectilePrefabEnum(projectile) != ProjectilePrefabs.NONE)
            {
                template.BaseProjectile = GetProjectilePrefabEnum(projectile);

                template.AddDirection = comp.AddDirection;
                template.AddRotationForce = comp.AddRotationForce;
                template.AutoTarget = comp.AutoTarget;
                template.AutoTargetMaxAngle = comp.AutoTargetMaxAngle;
                template.AutoTargetRange = comp.AutoTargetRange;
                template.IgnoreShooterCollision = comp.IgnoreShooterCollision;
                template.ProjectileForce = comp.ProjectileForce;
                template.TargetCountPerProjectile = comp.TargetCountPerProjectile;
                template.TargetingMode = comp.TargetingMode;
                template.TargetRange = comp.TargetRange;
                template.YMagnitudeAffect = comp.YMagnitudeAffect;
                template.YMagnitudeForce = comp.YMagnitudeForce;

                template.DefenseLength = projectile.DefenseLength;
                template.DefenseRange = projectile.DefenseRange;
                template.DisableOnHit = projectile.DisableOnHit;
                template.EffectsOnlyIfHitCharacter = projectile.EffectsOnlyIfHitCharacter;
                template.EndMode = projectile.EndMode;
                template.OnlyExplodeOnLayerMask = projectile.OnlyExplodeOnLayers;
                template.PhysicsLayerMask = projectile.ExplodeOnContactWithLayers.value;
                template.ImpactSoundMaterial = projectile.ImpactSoundMaterial;
                template.LateShootTime = projectile.LateShootTime;
                template.Lifespan = projectile.Lifespan;
                template.LightIntensityFade = projectile.LightIntensityFade;
                template.PointOffset = projectile.PointOffset;
                template.TrailEnabled = projectile.TrailEnabled;
                template.TrailTime = projectile.TrailTime;
                template.Unblockable = projectile.Unblockable;

                foreach (var shot in comp.ProjectileShots)
                {
                    template.ProjectileShots.Add(new SL_ProjectileShot()
                    {
                        RandomLocalDirectionAdd = shot.RandomLocalDirectionAdd,
                        LocalDirectionOffset = shot.LocalDirectionOffset,
                        LockDirection = shot.LockDirection,
                        MustShoot = shot.MustShoot,
                        NoBaseDir = shot.NoBaseDir
                    });
                }

                foreach (Transform child in projectile.transform)
                {
                    var effectsChild = SL_EffectTransform.ParseTransform(child);

                    if (effectsChild.ChildEffects.Count > 0 || effectsChild.Effects.Count > 0 || effectsChild.EffectConditions.Count > 0)
                    {
                        template.ProjectileEffects.Add(effectsChild);
                    }
                }
            }
            else if (comp.BaseProjectile)
            {
                SL.Log("Couldn't parse blast prefab to enum: " + comp.BaseProjectile.name);
            }
        }

        public class SL_ProjectileShot
        {
            public Vector3 LocalDirectionOffset;
            public Vector3 LockDirection;
            public bool MustShoot;
            public bool NoBaseDir;
            public Vector3 RandomLocalDirectionAdd;
        }



        // ============ Projectiles Dictionary ============ //

        private static bool m_initDone = false;

        public static Dictionary<ProjectilePrefabs, GameObject> ProjectilePrefabCache = new Dictionary<ProjectilePrefabs, GameObject>();

        public static GameObject GetProjectilePrefab(ProjectilePrefabs name)
        {
            if (name != ProjectilePrefabs.NONE)
            {
                return ProjectilePrefabCache[name];
            }
            else
            {
                return null;
            }
        }

        public static void BuildProjectileDictionary()
        {
            if (m_initDone)
            {
                return;
            }

            foreach (var projectile in Resources.FindObjectsOfTypeAll<Projectile>())
            {
                var name = GetProjectilePrefabEnum(projectile);

                if (name == ProjectilePrefabs.NONE)
                {
                    SL.Log("Couldn't parse projectile prefab: " + projectile.name, 0);
                }
                else if (!ProjectilePrefabCache.ContainsKey(name))
                {
                    bool wasActive = projectile.gameObject.activeSelf;
                    projectile.gameObject.SetActive(false);

                    var copy = GameObject.Instantiate(projectile.gameObject);
                    GameObject.DontDestroyOnLoad(copy);
                    copy.SetActive(false);

                    ProjectilePrefabCache.Add(name, copy);

                    projectile.gameObject.SetActive(wasActive);
                }
            }

            m_initDone = true;
        }

        /// <summary>
        /// Helper to take a Projectile and get the ProjectilePrefabs enum value for it (if valid).
        /// </summary>
        /// <param name="projectile">The projectile prefab</param>
        public static ProjectilePrefabs GetProjectilePrefabEnum(Projectile projectile)
        {
            var prefabName = projectile.name.Replace("(Clone)", "").Trim();

            if (Enum.TryParse(prefabName, out ProjectilePrefabs name))
            {
                return name;
            }
            else
            {
                return ProjectilePrefabs.NONE;
            }
        }

        public enum ProjectilePrefabs
        {
            NONE,
            AshPriestCirclingMine,
            BoozuDecayTrash,
            BulletBlood,
            BulletBloodSyphonProjectile,
            BulletLightning,
            BulletNormal,
            BulletShatter,
            CorruptionSpiritShot,
            ElementalBuffProjFire,
            ElementalParasiteMortar,
            ElementalProjectileBolt,
            ElementalProjectileDecay,
            ElementalProjectileEthereal,
            ElementalProjectileFire,
            ElementalProjectileIce,
            ElementalProjectileLight,
            ElementalProjectilePoison,
            ElementalProjectileWind,
            ElementalShot1Fire,
            ElementalShot3Fire,
            EliteBurningManHoming,
            EliteCalixaGunShot,
            EliteCalixaJumpDecayHomming,
            EliteCalixaJumpElectricHomming,
            EliteCalixaJumpFireHomming,
            EliteCalixaJumpIceHomming,
            EliteForgeGolemFirebolt,
            EliteForgeGolemIcebolt,
            EliteSharkMine,
            EliteShrimpLightningBolt,
            EliteTrogBoonHealProjectile,
            EliteTrogGrenade,
            EliteTrogQueenBoonHealProjectile,
            EliteTrogQueenPoisonProjectile,
            EliteTuanosaurFireball,
            EvasiveShotProjectile,
            Fireball,
            FireballBeetle,
            FireboltBeetleFireworks,
            FireboltForgeGolem,
            ForgeGolemRustLichMinionProjectile,
            GateProjectile,
            GolemShieldedBolt,
            GolemShieldedHealProjectile,
            GolemShieldedHomming,
            GolemShieldedHommingShortRange,
            GolemShieldedMortar,
            GolemShieldedMortarDown,
            IceWitchIcicleProjectile,
            IcicleProjectile,
            IlluminatorForwardMine,
            IlluminatorHealProjectile,
            IlluminatorMine,
            ImmaculateMaceWave,
            ImmaculateSwordMine,
            ImmaculateSwordMineProjectile,
            LichGoldLightningBolt,
            LichGoldStaffBolt,
            LichJadeCirclingMine,
            LichJadeHomming,
            LichJadeMine,
            ManticoreSpike,
            MultitargetStrike,
            ObsidianHoming,
            PierceShot,
            ProjectileArrow,
            PureIlluminatorAllieProjectile,
            PureIlluminatorBoonProjectile,
            PureIlluminatorHommingMine,
            RunicRayAmplifiedProjectile,
            RunicRayProjectile,
            ShrimpLightningBolt,
            SniperShot,
            SpecterLightBolt,
            SquireFireWave,
            SquireTendrils,
            StekoBoonHealProjectile,
            Tendrils,
            TrogBoonHealProjectile,
            TrogBoonProjectile,
            TrogGrenade,
            TrogHealProjectile,
            TrogHexProjectile,
            VendavelIcicleProjectile,
            WindGust,
        }

        //public static void DebugProjectileNames()
        //{
        //    var projs = Resources.FindObjectsOfTypeAll<Projectile>();
        //    var names = new List<string>();
        //    foreach (var proj in projs)
        //    {
        //        if (!names.Contains(proj.name))
        //        {
        //            names.Add(proj.name);
        //            Debug.Log(proj.name + ",");
        //        }
        //        else
        //        {
        //            //Debug.LogWarning("!!!!!!!!! DUPLICATE PROJ NAME !!!!!!!!!!!!!");
        //        }
        //    }
        //}
    }
}
