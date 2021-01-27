using SideLoader.SaveData;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SideLoader
{
    /// <summary>
    /// Used internally to handle SideLoader's networked features.
    /// </summary>
    public class SLRPCManager : Photon.MonoBehaviour
    {
        public const int VIEW_ID = 899;
        public static SLRPCManager Instance;

        public static void Setup()
        {
            var obj = new GameObject("SL_RPC");
            GameObject.DontDestroyOnLoad(obj);
            obj.AddComponent<SLRPCManager>();
        }

        internal void Awake()
        {
            Instance = this;

            var view = this.gameObject.GetComponent<PhotonView>();
            if (!view)
                view = this.gameObject.AddComponent<PhotonView>();

            view.viewID = VIEW_ID;
            SL.Log("Registered SideLoader RPCManager with view ID " + view.viewID);
        }

        /// <summary>
        /// Internal RPC call used by CustomCharacters.CreateCharacter. This is essentially a link to the CustomCharacters.SpawnCharacterCoroutine method.
        /// </summary>
        public void SpawnCharacter(string charUID, int viewID, string name, string visualData, string spawnCallbackUID, string extraRpcData, bool loadingFromSave)
        {
            this.photonView.RPC(nameof(RPC_SpawnCharacter), PhotonTargets.All, charUID, viewID, name, visualData, spawnCallbackUID, extraRpcData, loadingFromSave);
        }

        [PunRPC]
        internal void RPC_SpawnCharacter(string charUID, int viewID, string name, string visualData, string spawnCallbackUID, string extraRpcData, bool loadingFromSave)
        {
            SLPlugin.Instance.StartCoroutine(CustomCharacters.SpawnCharacterCoroutine(charUID, viewID, name, visualData, spawnCallbackUID, extraRpcData, loadingFromSave));
        }

        /// <summary>
        /// Internal RPC call used by CustomCharacters.DestroyCharacterRPC.
        /// </summary>
        public void DestroyCharacter(string charUID)
        {
            this.photonView.RPC(nameof(RPC_DestroyCharacter), PhotonTargets.All, new object[] { charUID });
        }

        [PunRPC]
        internal void RPC_DestroyCharacter(string charUID)
        {
            // First try the easy way... (this normally is the one that is called)
            if (CharacterManager.Instance.GetCharacter(charUID) is Character character)
            {
                CustomCharacters.DestroyCharacterLocal(character);
            }
            else // Otherwise, sometimes we need to do this, but I think I fixed this happening much (or at all).
            {
                var characters = Resources.FindObjectsOfTypeAll<Character>();

                try
                {
                    var c = characters.First(x => x.UID == charUID);
                    CustomCharacters.DestroyCharacterLocal(c);
                }
                catch
                {
                    // SL.Log("Could not destroy Character '" + charUID + "'");
                }
            }
        }

        // Received by MasterClient, sent from guests when they join the world
        [PunRPC]
        public void RPC_RequestCharacters()
        {
            // use the local save to capture the current spawned characters info
            SLCharacterSaveManager.SaveCharacters();

            // destroy current spawns
            CustomCharacters.CleanupCharacters();

            // Wait a short delay before the respawn
            StartCoroutine(DelayedRequestSpawn());
        }

        private IEnumerator DelayedRequestSpawn()
        {
            yield return new WaitForSeconds(0.5f);

            // respawn for all new clients from save data
            CustomCharacters.InvokeSpawnCharacters();
        }
    }
}
