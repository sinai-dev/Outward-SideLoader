using SideLoader.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using static SideLoader.SaveData.SLItemSpawnSaveManager;

namespace SideLoader
{
    public class SL_ItemSpawn : SL_ItemSource
    {
        // static cctor: subscribe to OnSceneLoaded for spawns
        static SL_ItemSpawn()
        {
            SL.OnGameplayResumedAfterLoading += OnSceneLoaded;
        }

        [XmlIgnore] internal static readonly Dictionary<string, SL_ItemSpawn> s_registeredSpawnSources = new Dictionary<string, SL_ItemSpawn>();
        [XmlIgnore] internal static readonly HashSet<ItemSpawnInfo> s_activeSavableSpawns = new HashSet<ItemSpawnInfo>();

        public int ItemID;
        public int Quantity = 1;

        public string SceneToSpawnIn = "";
        public Vector3 SpawnPosition;
        public Vector3 SpawnRotation;

        public bool ForceNonPickable;
        public bool TryLightFueledContainer = true;

        public override void ApplyActualTemplate()
        {
            base.ApplyActualTemplate();

            if (string.IsNullOrEmpty(this.IdentifierName))
            {
                SL.LogWarning("Cannot register an SL_ItemSpawn without an InternalName set!");
                return;
            }

            if (s_registeredSpawnSources.ContainsKey(this.IdentifierName))
            {
                SL.LogWarning($"An SL_ItemSpawn with the UID '{IdentifierName}' has already been registered!");
                return;
            }

            s_registeredSpawnSources.Add(this.IdentifierName, this);
        }

        internal static void OnSceneLoaded()
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
                return;

            SL.Log("Checking SL_ItemSpawns...");

            s_activeSavableSpawns.Clear();

            Dictionary<string, ItemSpawnInfo> savedData = null;

            if (!SL.WasLastSceneReset)
                savedData = LoadItemSpawnData();

            foreach (var spawn in s_registeredSpawnSources.Values)
            {
                // Check if already spawned
                if (savedData != null && savedData.ContainsKey(spawn.IdentifierName))
                {
                    var data = savedData[spawn.IdentifierName];
                    s_activeSavableSpawns.Add(data);

                    var item = ItemManager.Instance.GetItem(data.ItemUID);
                    if (item && string.IsNullOrEmpty(item.MostRecentOwnerUID))
                    {
                        // item hasn't been picked up yet, update it to current template settings.
                        s_registeredSpawnSources[spawn.IdentifierName].ApplyToItem(item);
                    }

                    continue;
                }

                // else, new spawn
                if (spawn.SceneToSpawnIn == SceneManagerHelper.ActiveSceneName)
                    spawn.GenerateItem();
            }
        }

        internal void GenerateItem()
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
                return;

            if (s_activeSavableSpawns.Any(it => it.SpawnIdentifier == this.IdentifierName))
            {
                SL.LogWarning("Trying to spawn two SL_ItemSpawns with the same Identifier: " + this.IdentifierName);
                return;
            }

            var prefab = ResourcesPrefabManager.Instance.GetItemPrefab(this.ItemID);
            if (!prefab)
            {
                SL.LogWarning($"SL_ItemSpawn: Could not find any item by ID '{ItemID}'!");
                return;
            }

            SL.Log($"SL_ItemSpawn '{this.IdentifierName}' spawning...");

            var item = ItemManager.Instance.GenerateItemNetwork(this.ItemID);

            if (!ForceNonPickable)
            {
                s_activeSavableSpawns.Add(new ItemSpawnInfo
                {
                    SpawnIdentifier = this.IdentifierName,
                    ItemID = this.ItemID,
                    ItemUID = item.UID
                });
            }

            ApplyToItem(item);
        }

        /// <summary>
        /// Called for new spawns, or if loading from save and item hasnt been picked up yet.
        /// </summary>
        internal void ApplyToItem(Item item)
        {
            item.ChangeParent(null, this.SpawnPosition, Quaternion.Euler(this.SpawnRotation));

            if (Quantity > 1 && item.HasMultipleUses)
                item.RemainingAmount = Quantity;

            if (ForceNonPickable)
            {
                item.IsPickable = false;
                item.ForceNonSavable = true;
            }

            if (TryLightFueledContainer && item is FueledContainer fueled)
                fueled.SetLight(true);
        }
    }
}
