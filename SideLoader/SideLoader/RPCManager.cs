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

        [PunRPC]
        public void RPCSpawnCharacter(string charUID, int viewID, string name)
        {
            StartCoroutine(SpawnCharacterCoroutine(charUID, viewID, name));
        }

        private static IEnumerator SpawnCharacterCoroutine(string charUID, int viewID, string name)
        {
            Character character = null;
            while (character == null)
            {
                character = CharacterManager.Instance.GetCharacter(charUID);
                yield return null;
            }

            // add to cache list
            CustomCharacters.AddActiveCharacter(character.gameObject);

            character.name = $"{name}_{charUID}";
            At.SetValue("", typeof(Character), character, "m_nameLocKey");
            At.SetValue(name, typeof(Character), character, "m_name");

            // fix UI bar offset
            character.UIBarOffSet += Vector3.up * 0.1f;

            // fix Photon View component
            if (character.gameObject.GetPhotonView() is PhotonView view)
            {
                DestroyImmediate(view);
            }

            var pView = character.gameObject.AddComponent<PhotonView>();
            pView.viewID = viewID;
            pView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
            pView.onSerializeRigidBodyOption = OnSerializeRigidBody.All;
            pView.synchronization = ViewSynchronization.Unreliable;

            //character.gameObject.SetActive(true);
        }
    }
}
