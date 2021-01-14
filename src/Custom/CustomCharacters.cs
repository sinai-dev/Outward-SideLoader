using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using HarmonyLib;
using UnityEngine.SceneManagement;
using SideLoader.Helpers;
using System.IO;
using SideLoader.SaveData;

namespace SideLoader
{
    /// <summary>
    /// SideLoader's manager class for Custom Characters. Contains useful methods for the creation, mangement and destruction  of SL_Characters.
    /// </summary>
    public class CustomCharacters
    {
        // ======================== PUBLIC HELPERS ======================== //

        /// <summary>
        /// Use this to cleanup a custom character. This will send out an RPC.
        /// </summary>
        /// <param name="character">The Character to destroy.</param>
        public static void DestroyCharacterRPC(Character character) 
            => SLRPCManager.Instance.DestroyCharacter(character.UID);

        /// <summary>
        /// Use this to cleanup a custom character. This will send out an RPC.
        /// </summary>
        /// <param name="UID">The UID of the Character to destroy.</param>
        public static void DestroyCharacterRPC(string UID) 
            => SLRPCManager.Instance.DestroyCharacter(UID);

        // ============ Spawning ============

        [Obsolete("addCombatAI is no longer part of CustomCharacters. Use an SL_Character template instead.")]
        public static GameObject SpawnCharacter(Vector3 position, string uid, string name = "SL_Character", bool addCombatAI = false, string extraRpcData = null)
           => InternalSpawn(position, Vector3.zero, uid, name, null, null, extraRpcData);

        [Obsolete("addCombatAI is no longer part of CustomCharacters. Use an SL_Character template instead.")]
        public static GameObject SpawnCharacter(Vector3 pos, Vector3 rotation, string uid, string name = "SL_Character", bool addCombatAI = false, string extraRpcData = null)
            => InternalSpawn(pos, rotation, uid, name, null, uid, extraRpcData);

        /// <summary>
        /// Spawns a custom character and applies the template. Optionally provide a manual spawn position and Character UID.
        /// The OnSpawn callback is based on the Template UID. You should have already called template.Prepare() before calling this.
        /// </summary>
        /// <param name="template">The SL_Character template containing most of the main information</param>
        /// <param name="position">Optional manual spawn position, otherwise just use the template.SpawnPosition</param>
        /// <param name="characterUID">Optional manual character UID, if dynamically spawning multiple from one template.</param>
        /// <param name="extraRpcData">Optional extra RPC data to send with the spawn.</param>
        /// <returns>The GameObject of your new character</returns>
        public static GameObject SpawnCharacter(SL_Character template, Vector3 position, string characterUID = null, string extraRpcData = null)
            => SpawnCharacter(template, position, Vector3.zero, characterUID, extraRpcData);

        /// <summary>
        /// Spawns a custom character and applies the template. Optionally provide a manual spawn position and Character UID.
        /// The OnSpawn callback is based on the Template UID. You should have already called template.Prepare() before calling this.
        /// </summary>
        /// <param name="template">The SL_Character template containing most of the main information</param>
        /// <param name="position">Optional manual spawn position, otherwise just use the template.SpawnPosition</param>
        /// <param name="rotation">Optional manual rotation to spawn with, otherwise just use the template.SpawnRotation.</param>
        /// <param name="characterUID">Optional manual character UID, if dynamically spawning multiple from one template.</param>
        /// <param name="extraRpcData">Optional extra RPC data to send with the spawn.</param>
        /// <returns>Your custom character</returns>
        public static GameObject SpawnCharacter(SL_Character template, Vector3 position, Vector3 rotation, string characterUID = null, string extraRpcData = null)
            => InternalSpawn(position, rotation, characterUID ?? template.UID, template.Name, template.CharacterVisualsData?.ToString(), template.UID, extraRpcData);

        // ==================== Internal ====================

        /// <summary>Key: Spawn callback UID (generally template UID), Value: SL_Character with OnSpawn event to invoke</summary>
        internal static Dictionary<string, SL_Character> Templates = new Dictionary<string, SL_Character>();

        internal static readonly List<CustomSpawnInfo> ActiveCharacters = new List<CustomSpawnInfo>();

        internal static event Action INTERNAL_SpawnCharacters;

        // public static bool IsRealScene(Scene scene) => true; // CustomScenes.IsRealScene(scene);

        internal static void InvokeSpawnCharacters()
        {
            var scene = SceneManager.GetActiveScene();
            if (INTERNAL_SpawnCharacters != null) // && IsRealScene(scene)
            {
                SL.Log($"Spawning characters ({scene.name})");
                SL.TryInvoke(INTERNAL_SpawnCharacters);
            }

            if (!PhotonNetwork.isNonMasterClientInRoom)
                SLPlugin.Instance.StartCoroutine(SLCharacterSaveManager.TryLoadSaveData());
        }

        internal static void AddActiveCharacter(CustomSpawnInfo info)
        {
            if (!ActiveCharacters.Contains(info))
                ActiveCharacters.Add(info);
        }

        internal static void RequestSpawnedCharacters()
        {
            SLRPCManager.Instance.photonView.RPC(nameof(SLRPCManager.RPC_RequestCharacters), PhotonTargets.MasterClient);
        }

        // Main internal spawn method
        internal static GameObject InternalSpawn(Vector3 position, Vector3 rotation, string UID, string name, string visualData = null, string spawnCallbackUID = null, string extraRpcData = null)
        {
            SL.Log($"Spawning character '{name}', _UID: {UID}, spawnCallbackUID: {spawnCallbackUID}");

            try
            {
                GameObject prefab;
                Character character;

                var args = new object[]
                {
                    (int)CharacterManager.CharacterInstantiationTypes.Temporary,
                    "NewPlayerPrefab",
                    UID,
                    string.Empty // dont send a creator UID, otherwise it links the current summon (used by Conjure Ghost)
                };

                prefab = PhotonNetwork.InstantiateSceneObject("_characters/NewPlayerPrefab", position, Quaternion.Euler(rotation), 0, args);
                prefab.SetActive(false);

                character = prefab.GetComponent<Character>();

                character.SetUID(UID);

                //FixStats(prefab.GetComponent<Character>());

                var viewID = PhotonNetwork.AllocateSceneViewID();

                if (string.IsNullOrEmpty(spawnCallbackUID))
                    spawnCallbackUID = UID;

                prefab.SetActive(true);

                SLRPCManager.Instance.SpawnCharacter(UID, viewID, name, visualData, spawnCallbackUID, extraRpcData);

                return prefab;
            }
            catch (Exception e)
            {
                SL.Log("Exception spawning character: " + e.ToString());
                return null;
            }
        }

        internal static IEnumerator SpawnCharacterCoroutine(string charUID, int viewID, string name, string visualData, string spawnCallbackUID, string extraRpcData)
        {
            // get character from manager
            Character character = CharacterManager.Instance.GetCharacter(charUID);
            float start = 0f;
            while (!character && start < 15f)
            {
                yield return null;
                start += Time.deltaTime;
                character = CharacterManager.Instance.GetCharacter(charUID);
            }

            if (!character)
                yield break;

            // add to cache list
            AddActiveCharacter(new CustomSpawnInfo(character, spawnCallbackUID, extraRpcData));

            if (string.IsNullOrEmpty(name))
                name = "SL_Character";

            // set name
            character.name = $"{name}_{charUID}";

            //if (addCombatAI) // && !localSpawn)
            //    SetupBasicAI(character);

            // invoke OnSpawn callback
            if (Templates.ContainsKey(spawnCallbackUID))
            {
                var template = Templates[spawnCallbackUID];

                template.INTERNAL_OnSpawn(character, extraRpcData);
            }

            character.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(visualData))
                SLPlugin.Instance.StartCoroutine(SL_Character.SetVisuals(character, visualData));

            // fix Photon View component
            if (character.gameObject.GetComponent<PhotonView>() is PhotonView view)
            {
                int id = view.viewID;
                GameObject.DestroyImmediate(view);

                if (!PhotonNetwork.isNonMasterClientInRoom && id > 0)
                    PhotonNetwork.UnAllocateViewID(view.viewID);
            }

            // setup new view
            var pView = character.gameObject.AddComponent<PhotonView>();
            pView.viewID = viewID;
            pView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
            pView.onSerializeRigidBodyOption = OnSerializeRigidBody.All;
            pView.synchronization = ViewSynchronization.Unreliable;

            // fix photonview serialization components
            if (pView.ObservedComponents == null || pView.ObservedComponents.Count < 1)
                pView.ObservedComponents = new List<Component>() { character };

            float t = 0f;
            while (t < 0.5f)
            {
                yield return null;
                t += Time.deltaTime;
            }

            character.gameObject.SetActive(false);
            character.gameObject.SetActive(true);
        }

        // called from a patch on level unload
        internal static void CleanupCharacters()
        {
            if (ActiveCharacters.Count > 0 && !PhotonNetwork.isNonMasterClientInRoom)
            {
                // SL.Log("Cleaning up " + ActiveCharacters.Count + " characters.");

                for (int i = ActiveCharacters.Count - 1; i >= 0; i--)
                {
                    var info = ActiveCharacters[i];

                    if (info.ActiveCharacter)
                        DestroyCharacterRPC(info.ActiveCharacter);
                    else
                        ActiveCharacters.RemoveAt(i);
                }
            }
        }

        internal static void DestroyCharacterLocal(Character character)
        {
            if (!character)
            {
                //SL.Log("Trying to destroy a character that is null or already destroyed!");
                return;
            }

            character.gameObject.SetActive(false);

            var query = ActiveCharacters.Where(it => it.ActiveCharacter?.UID == character.UID);
            if (query.Any())
            {
                var info = query.First();
                ActiveCharacters.Remove(info);
            }

            var m_characters = At.GetField(CharacterManager.Instance, "m_characters") as DictionaryExt<string, Character>;
            if (m_characters.ContainsKey(character.UID))
                m_characters.Remove(character.UID);

            var pv = character.photonView;
            int view = pv.viewID;

            //  DestroyImmediate
            GameObject.DestroyImmediate(character.gameObject);

            if (character)
                SL.LogError("ERROR - Could not seem to destroy character " + character.UID);
            else
                PhotonNetwork.UnAllocateViewID(view);
        }

        #region Stats

        /// <summary>
        /// Removes PlayerCharacterStats and replaces with CharacterStats.
        /// </summary>
        public static void FixStats(Character character)
        {
            // remove PlayerCharacterStats
            if (character.GetComponent<PlayerCharacterStats>() is PlayerCharacterStats pStats)
            {
                pStats.enabled = false;
                GameObject.DestroyImmediate(pStats);
                if (character.GetComponent<PlayerCharacterStats>())
                    GameObject.Destroy(pStats);
            }
            // add new CharacterStats
            var newStats = character.gameObject.AddComponent<CharacterStats>();
            At.SetField(character, "m_characterStats", newStats);
            SetupBlankCharacterStats(newStats);
        }


        /// <summary>
        /// Resets a CharacterStats to have all default stats (default for the Player).
        /// </summary>
        /// <param name="stats"></param>
        public static void SetupBlankCharacterStats(CharacterStats stats)
        {
            At.SetField(stats, "m_damageResistance",
                new Stat[] { new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f) });
            At.SetField(stats, "m_damageProtection",
                new Stat[] { new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f) });
            At.SetField(stats, "m_damageTypesModifier", 
                new Stat[] { new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f) });

            At.SetField(stats, "m_heatRegenRate", new Stat(0f));
            At.SetField(stats, "m_coldRegenRate", new Stat(0f));
            At.SetField(stats, "m_heatProtection", new Stat(0f));
            At.SetField(stats, "m_coldProtection", new Stat(0f));
            At.SetField(stats, "m_corruptionResistance", new Stat(0f));
            At.SetField(stats, "m_waterproof", new Stat(0f));
            At.SetField(stats, "m_healthRegen", new Stat(0f));
            At.SetField(stats, "m_manaRegen", new Stat(0f));
            At.SetField(stats, "m_statusEffectsNaturalImmunity", new TagSourceSelector[0]);
        }

        #endregion

        #region ENEMY CLONE TEST
        // ===================== A test I did with cloning enemies. It mostly works. =======================

        /// <summary>
        /// [BETA] Finds a GameObject with _gameObjectName and clones it into a new Character (if it contains a Character component)
        /// </summary>
        public static void CloneCharacter(string _gameObjectName)
        {
            if (GameObject.Find(_gameObjectName) is GameObject obj && obj.GetComponent<Character>() is Character c)
            {
                CloneCharacter(c);
            }
        }

        /// <summary>
        /// [BETA] Clone a character by providing the component directly
        /// </summary>
        public static void CloneCharacter(Character _targetCharacter)
        {
            try
            {
                var targetObj = _targetCharacter.gameObject;

                // prepare original for clone
                targetObj.SetActive(false);
                bool disable = _targetCharacter.DisableAfterInit;
                _targetCharacter.DisableAfterInit = false;

                // make clone
                var clone = GameObject.Instantiate(targetObj);
                clone.SetActive(false);

                // fix original
                _targetCharacter.DisableAfterInit = disable;
                targetObj.SetActive(true);

                // fix clone UIDs, etc
                var character = clone.GetComponent<Character>();
                At.SetField(character, "m_uid", UID.Generate());
                clone.name = "[CLONE] " + character.Name + "_" + character.UID;

                // allocate a scene view ID (will need RPC if to work in multiplayer)
                clone.GetComponent<PhotonView>().viewID = PhotonNetwork.AllocateSceneViewID();

                var items = character.GetComponentsInChildren<Item>();
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];

                    var new_item = ItemManager.Instance.GenerateItemNetwork(item.ItemID);
                    new_item.transform.parent = item.transform.parent;

                    GameObject.DestroyImmediate(item);
                }

                //// todo same for droptable components
                //var lootable = clone.GetComponent<LootableOnDeath>();

                //var oldTables = new List<GameObject>();

                clone.SetActive(true);

                // heal reset
                character.Stats.Reset();

                clone.transform.position = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
            }
            catch (Exception e)
            {
                SL.LogError($"Error cloning enemy: {e.GetType()}, {e.Message}\r\nStack trace: {e.StackTrace}");
            }
        }
        #endregion
    }
}
