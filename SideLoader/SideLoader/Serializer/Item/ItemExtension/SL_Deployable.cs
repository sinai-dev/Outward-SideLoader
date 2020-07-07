using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SideLoader
{
    public class SL_Deployable : SL_ItemExtension
    {
        public bool? AutoTake;
        public bool? CantDeployInNoBedZones;
        public Character.SpellCastType? CastAnim;
        public int? DeployedStateItemID;
        public Vector3? DeploymentDirection;
        public Vector3? DeploymentOffset;
        public GlobalAudioManager.Sounds? DeploymentSound;
        public Vector3? DisassembleOffset;
        public GlobalAudioManager.Sounds? DisassembleSound;
        public int? PackedItemPrefabID;
        public Deployable.DeployStates? State;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Deployable;

            if (this.AutoTake != null)
            {
                comp.AutoTake = (bool)this.AutoTake;
            }
            if (this.CantDeployInNoBedZones != null)
            {
                comp.CantDeployInNoBedZones = (bool)this.CantDeployInNoBedZones;
            }
            if (this.CastAnim != null)
            {
                comp.CastAnim = (Character.SpellCastType)this.CastAnim;
            }
            if (this.DeployedStateItemID != null)
            {
                var deployedItem = ResourcesPrefabManager.Instance.GetItemPrefab((int)this.DeployedStateItemID);
                if (deployedItem)
                {
                    comp.DeployedStateItemPrefab = deployedItem;
                }
            }
            if (this.DeploymentDirection != null)
            {
                comp.DeploymentDirection = (Vector3)this.DeploymentDirection;
            }
            if (this.DeploymentOffset != null)
            {
                comp.DeploymentOffset = (Vector3)this.DeploymentOffset;
            }
            if (this.DeploymentSound != null)
            {
                comp.DeploySound = (GlobalAudioManager.Sounds)this.DeploymentSound;
            }
            if (this.DisassembleOffset != null)
            {
                comp.DisassembleOffset = (Vector3)this.DisassembleOffset;
            }
            if (this.DisassembleSound != null)
            {
                comp.DisassembleSound = (GlobalAudioManager.Sounds)this.DisassembleSound;
            }
            if (this.PackedItemPrefabID != null)
            {
                var packeditem = ResourcesPrefabManager.Instance.GetItemPrefab((int)this.PackedItemPrefabID);
                if (packeditem)
                {
                    comp.PackedStateItemPrefab = packeditem;
                }
            }
            if (this.State != null)
            {
                comp.State = (Deployable.DeployStates)this.State;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as Deployable;

            this.AutoTake = comp.AutoTake;
            this.CantDeployInNoBedZones = comp.CantDeployInNoBedZones;
            this.CastAnim = comp.CastAnim;
            this.DeployedStateItemID = comp.DeployedStateItemPrefab?.ItemID;
            this.DeploymentDirection = comp.DeploymentDirection;
            this.DeploymentOffset = comp.DeploymentOffset;
            this.DeploymentSound = comp.DeploySound;
            this.DisassembleOffset = comp.DisassembleOffset;
            this.DisassembleSound = comp.DisassembleSound;
            this.PackedItemPrefabID = comp.PackedStateItemPrefab?.ItemID;
            this.State = comp.State;
        }
    }
}
