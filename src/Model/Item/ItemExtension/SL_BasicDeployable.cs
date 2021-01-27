using UnityEngine;

namespace SideLoader
{
    public abstract class SL_BasicDeployable : SL_ItemExtension
    {
        public bool? CantDeployInNoBedZones;
        public Vector3? DeploymentDirection;
        public Vector3? DeploymentOffset;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as BasicDeployable;

            if (this.DeploymentDirection != null)
                comp.DeploymentDirection = (Vector3)this.DeploymentDirection;

            if (this.DeploymentOffset != null)
                comp.DeploymentOffset = (Vector3)this.DeploymentOffset;

            if (this.CantDeployInNoBedZones != null)
                comp.CantDeployInNoBedZones = (bool)this.CantDeployInNoBedZones;
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as BasicDeployable;

            this.CantDeployInNoBedZones = comp.CantDeployInNoBedZones;
            this.DeploymentDirection = comp.DeploymentDirection;
            this.DeploymentOffset = comp.DeploymentOffset;
        }
    }
}
