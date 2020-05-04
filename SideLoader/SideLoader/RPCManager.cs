using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        public void SetCharacterViewID(string charUID, int viewID)
        {
            StartCoroutine(SetCharViewIDCoroutine(charUID, viewID));
        }

        private static IEnumerator SetCharViewIDCoroutine(string charUID, int viewID)
        {
            Character character = null;
            while (character == null)
            {
                character = CharacterManager.Instance.GetCharacter(charUID);
                yield return null;
            }

            if (character.gameObject.GetPhotonView() is PhotonView view)
            {
                DestroyImmediate(view);
            }

            var pView = character.gameObject.AddComponent<PhotonView>();
            pView.viewID = viewID;
            pView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
            pView.onSerializeRigidBodyOption = OnSerializeRigidBody.All;
            pView.synchronization = ViewSynchronization.Unreliable;
        }
    }
}
