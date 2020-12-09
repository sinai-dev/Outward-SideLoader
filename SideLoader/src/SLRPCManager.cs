using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using SideLoader.Helpers;
using Photon;

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
        public void SpawnCharacter(string charUID, int viewID, string name, string visualData, bool addCombatAI, string spawnCallbackUID, string extraRpcData)
        {
            this.photonView.RPC(nameof(RPC_SpawnCharacter), PhotonTargets.All, charUID, viewID, name, visualData, addCombatAI, spawnCallbackUID, extraRpcData);
        }

        [PunRPC]
        internal void RPC_SpawnCharacter(string charUID, int viewID, string name, string visualData, bool addCombatAI, string spawnCallbackUID, string extraRpcData)
        {
            SLPlugin.Instance.StartCoroutine(CustomCharacters.SpawnCharacterCoroutine(charUID, viewID, name, visualData, addCombatAI, spawnCallbackUID, extraRpcData));
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
                    SL.Log("Could not destroy Character '" + charUID + "'");
                }
            }
        }

        // Received by MasterClient, sent from guests when they join the world
        [PunRPC]
        public void RPC_RequestCharacters(PhotonMessageInfo info)
        {
            if (info.sender == null)
            {
                SL.LogWarning("Received RPC_RequestCharacters, but the PhotonMessageInfo.sender was null!");
                return;
            }

            string charDataList = "";

            foreach (var charInfo in CustomCharacters.ActiveCharacters)
            {
                if (charInfo.Template == null || !charInfo.ActiveCharacter)
                    continue;

                if (charDataList != "")
                    charDataList += "\n";

                charDataList += $"{charInfo.ActiveCharacter.UID.ToString()}|{charInfo.Template.UID}|{Encryptor.EncodeString(charInfo.ExtraRPCData)}";
            }

            this.photonView.RPC(nameof(RPC_SendCharacters), info.sender, charDataList);
        }

        // Received by Sender of RequestCharacters, send by master client
        [PunRPC]
        public void RPC_SendCharacters(string characterList)
        {
            var datas = characterList.Split('\n');

            foreach (var charData in datas)
            {
                var data = charData.Split('|');

                string charUID = data[0];
                string templateUID = data[1];
                string extraRpcData = Encryptor.DecodeString(data[2]);

                CustomCharacters.Templates.TryGetValue(templateUID, out SL_Character template);

                if (template == null)
                {
                    SL.LogWarning("SLRPC_SendCharacters: Could not find a template with the UID " + templateUID);
                    continue;
                }

                // call SpawnCharacter with onlySpawnLocally = true.
                CustomCharacters.SpawnCharacter(template.SpawnPosition, charUID, template.Name, 
                    template.CharacterVisualsData.ToString(), template.AddCombatAI, templateUID, extraRpcData, true);
            }
        }
    }
}
