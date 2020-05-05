using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace SideLoader
{
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

        public void SpawnCharacter(string charUID, int viewID, string name)
        {
            photonView.RPC("RPCSpawnCharacter", PhotonTargets.All, new object[] { charUID, viewID, name });
        }

        [PunRPC]
        public void RPCSpawnCharacter(string charUID, int viewID, string name)
        {
            StartCoroutine(CustomCharacters.SpawnCharacterCoroutine(charUID, viewID, name));
        }

        public void DestroyCharacter(string charUID)
        {
            photonView.RPC("RPCDestroyCharacter", PhotonTargets.All, new object[] { charUID });
        }

        [PunRPC]
        private void RPCDestroyCharacter(string charUID)
        {
            if (CharacterManager.Instance.GetCharacter(charUID) is Character character)
            {
                CustomCharacters.DestroyCharacter(character);
            }
        }
    }
}
