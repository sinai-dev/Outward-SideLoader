using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace SideLoader
{
    /// <summary>
    /// Used internally to manage various networking features, using Photon RPC calls.
    /// </summary>
    public class RPCManager : Photon.MonoBehaviour
    {
        public static RPCManager Instance;

        public const int ViewID = 899;

        internal void Awake()
        {
            Instance = this;

            var view = this.gameObject.GetOrAddComponent<PhotonView>();
            view.viewID = ViewID;
            Debug.Log("Registered SideLoader RPCManager with view ID " + view.viewID);
        }

        /// <summary>
        /// Internal RPC call used by CustomCharacters.CreateCharacter. This is essentially a link to the CustomCharacters.SpawnCharacterCoroutine method.
        /// </summary>
        public void SpawnCharacter(string charUID, int viewID, string name, string visualData, bool addCombatAI, string spawnCallbackUID, string extraRpcData)
        {
            //RPCSpawnCharacter(charUID, viewID, name, visualData, addCombatAI, spawnCallbackUID, extraRpcData);
            photonView.RPC("RPCSpawnCharacter", PhotonTargets.All, new object[] { charUID, viewID, name, visualData, addCombatAI, spawnCallbackUID, extraRpcData });
        }

        [PunRPC]
        private void RPCSpawnCharacter(string charUID, int viewID, string name, string visualData, bool addCombatAI, string spawnCallbackUID, string extraRpcData)
        {
            StartCoroutine(CustomCharacters.SpawnCharacterCoroutine(charUID, viewID, name, visualData, addCombatAI, spawnCallbackUID, extraRpcData));
        }

        /// <summary>
        /// Internal RPC call used by CustomCharacters.DestroyCharacterRPC.
        /// </summary>
        public void DestroyCharacter(string charUID)
        {
            photonView.RPC("RPCDestroyCharacter", PhotonTargets.All, new object[] { charUID });
        }

        [PunRPC]
        private void RPCDestroyCharacter(string charUID)
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
                    SL.Log("Could not destroy Character '" + charUID + "'");
                }
            }
        }
    }
}
