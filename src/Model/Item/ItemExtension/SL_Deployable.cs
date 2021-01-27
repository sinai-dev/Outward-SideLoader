using SideLoader.SLPacks;
using System;
using UnityEngine;

namespace SideLoader
{
    public class SL_Deployable : SL_BasicDeployable
    {
        public bool? AutoTake;
        public Character.SpellCastType? CastAnim;
        public int? DeployedStateItemID;
        public GlobalAudioManager.Sounds? DeploymentSound;
        public Vector3? DisassembleOffset;
        public GlobalAudioManager.Sounds? DisassembleSound;
        public int? PackedItemPrefabID;
        public Deployable.DeployStates? State;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as Deployable;

            SLPackManager.AddLateApplyListener(OnLateApply, comp);

            if (this.AutoTake != null)
                comp.AutoTake = (bool)this.AutoTake;

            if (this.CastAnim != null)
                comp.CastAnim = (Character.SpellCastType)this.CastAnim;

            if (this.DeploymentSound != null)
                comp.DeploySound = (GlobalAudioManager.Sounds)this.DeploymentSound;

            if (this.DisassembleOffset != null)
                comp.DisassembleOffset = (Vector3)this.DisassembleOffset;

            if (this.DisassembleSound != null)
                comp.DisassembleSound = (GlobalAudioManager.Sounds)this.DisassembleSound;

            if (this.State != null)
                comp.State = (Deployable.DeployStates)this.State;
        }

        private void OnLateApply(object[] obj)
        {
            var comp = obj[0] as Deployable;

            if (!comp)
                return;

            if (this.DeployedStateItemID != null)
            {
                var deployedItem = ResourcesPrefabManager.Instance.GetItemPrefab((int)this.DeployedStateItemID);
                if (deployedItem)
                    comp.DeployedStateItemPrefab = deployedItem;
            }

            if (this.PackedItemPrefabID != null)
            {
                var packeditem = ResourcesPrefabManager.Instance.GetItemPrefab((int)this.PackedItemPrefabID);
                if (packeditem)
                    comp.PackedStateItemPrefab = packeditem;
            }
        }

        public override void SerializeComponent<T>(T extension)
        {
            var comp = extension as Deployable;

            this.AutoTake = comp.AutoTake;
            this.CastAnim = comp.CastAnim;
            this.DeployedStateItemID = comp.DeployedStateItemPrefab?.ItemID;
            this.DeploymentSound = comp.DeploySound;
            this.DisassembleOffset = comp.DisassembleOffset;
            this.DisassembleSound = comp.DisassembleSound;
            this.PackedItemPrefabID = comp.PackedStateItemPrefab?.ItemID;
            this.State = comp.State;
        }
    }
}
