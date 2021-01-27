using UnityEngine;

namespace SideLoader
{
    /// <summary>
    /// Abstract base class for SL_ShootBlast and SL_ShootProjectile
    /// </summary>
    public abstract class SL_Shooter : SL_Effect
    {
        public Shooter.CastPositionType CastPosition;
        public Vector3 LocalPositionAdd;
        public bool NoAim;
        public Shooter.TargetTypes TargetType;
        public string TransformName;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Shooter;

            comp.CastPosition = this.CastPosition;
            comp.LocalCastPositionAdd = this.LocalPositionAdd;
            comp.NoAim = this.NoAim;
            comp.TargetType = this.TargetType;
            comp.TransformName = this.TransformName;
        }

        public override void SerializeEffect<T>(T effect)
        {
            var comp = effect as Shooter;

            CastPosition = comp.CastPosition;
            NoAim = comp.NoAim;
            LocalPositionAdd = comp.LocalCastPositionAdd;
            TargetType = comp.TargetType;
            TransformName = comp.TransformName;
        }
    }
}
