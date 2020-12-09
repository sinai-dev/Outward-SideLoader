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

namespace SideLoader
{
    /// <summary>
    /// SideLoader's manager class for Custom Characters. Contains useful methods for the creation, mangement and destruction  of SL_Characters.
    /// </summary>
    public class CustomCharacters
    {
        internal static readonly List<CustomSpawnInfo> ActiveCharacters = new List<CustomSpawnInfo>();

        internal static event Action INTERNAL_SpawnCharacters;

        /// <summary>Key: Spawn callback UID (generally template UID), Value: SL_Character with OnSpawn event to invoke</summary>
        internal static Dictionary<string, SL_Character> Templates = new Dictionary<string, SL_Character>();

        public static bool WasLastAreaReset { get; internal set; }

        internal static void InvokeSpawnCharacters()
        {
            var scene = SceneManager.GetActiveScene();
            if (INTERNAL_SpawnCharacters != null) // && IsRealScene(scene)
            {
                SL.Log($"Spawning characters ({scene.name})");
                SL.TryInvoke(INTERNAL_SpawnCharacters);
            }

            if (!PhotonNetwork.isNonMasterClientInRoom)
                SLPlugin.Instance.StartCoroutine(TryLoadSaveData());
        }

        internal static GameObject BasicAIPrefab
        {
            get
            {
                if (!m_basicAIPrefab)
                {
                    SetupBasicAIPrefab();
                }
                return m_basicAIPrefab;
            }
        }
        private static GameObject m_basicAIPrefab;

        // ======================== PUBLIC HELPERS ======================== //

        /// <summary>
        /// Use this to cleanup a custom character. This will send out an RPC.
        /// </summary>
        /// <param name="character">The Character to destroy.</param>
        public static void DestroyCharacterRPC(Character character)
        {
            SLRPCManager.Instance.DestroyCharacter(character.UID);
        }

        /// <summary>
        /// Spawns a custom character and applies the template. Optionally provide a manual spawn position and Character UID.
        /// The OnSpawn callback is based on the Template UID. You should have already called template.Prepare() before calling this.
        /// </summary>
        /// <param name="template">The SL_Character template containing most of the main information</param>
        /// <param name="position">Optional manual spawn position, otherwise just provide the template.SpawnPosition</param>
        /// <param name="characterUID">Optional manual character UID, if dynamically spawning multiple from one template.</param>
        /// <param name="extraRpcData">Optional extra RPC data to send with the spawn.</param>
        /// <returns></returns>
        public static GameObject SpawnCharacter(SL_Character template, Vector3 position, string characterUID = null, string extraRpcData = null)
        {
            characterUID = characterUID ?? template.UID;

            return SpawnCharacter(
                position,
                characterUID,
                template.Name,
                template.CharacterVisualsData?.ToString(),
                template.AddCombatAI,
                template.UID,
                extraRpcData
            );
        }

        /// <summary>
        /// Simple helper to create a generic character without any template.
        /// </summary>
        /// <param name="pos">The spawn position for the character.</param>
        /// <param name="uid">The UID for the character.</param>
        /// <param name="name">Name for the character.</param>
        /// <param name="addCombatAI">Whether to add a generic combat AI to the character</param>
        /// <param name="extraRpcData">Optional extra RPC data to send.</param>
        /// <returns>Your custom character (instantly for Host)</returns>
        public static GameObject SpawnCharacter(Vector3 pos, string uid, string name = "SL_Character", bool addCombatAI = false, string extraRpcData = null)
        {
            return SpawnCharacter(pos, uid, name, null, addCombatAI, uid, extraRpcData);
        }

        /// <summary>
        /// Instantiates a new human character with the attributes provided. Only one client should call this.
        /// This is the main SpawnCharacter method, called by the other SpawnCharacter methods.
        /// </summary>
        /// <param name="_position">The spawn position for the character.</param>
        /// <param name="_UID">The UID for the character.</param>
        /// <param name="_name">The Name of your custom character.</param>
        /// <param name="spawnCallbackUID">Optional, the SL_Character template UID which will be used to invoke the OnSpawn callback.</param>
        /// <param name="addCombatAI">Whether to add basic combat AI to the character</param>
        /// <param name="visualData">Optional visual data (network data). Use SL_Character.VisualData.ToString().</param>
        /// <param name="extraRpcData">Optional extra RPC data to send with the spawn</param>
        /// <param name="onlySpawnLocally">Used internally</param>
        /// <param name="sceneView">Used internally</param>
        /// <returns>The custom character (instantly for executing client)</returns>
        public static GameObject SpawnCharacter(Vector3 _position, string _UID, string _name, string visualData = null, bool addCombatAI = false, string spawnCallbackUID = null, string extraRpcData = null, bool onlySpawnLocally = false, int sceneView = -1)
        {
            SL.Log($"Spawning character '{_name}', _UID: {_UID}, spawnCallbackUID: {spawnCallbackUID}");

            try
            {
                GameObject prefab;
                Character character;
                if (!onlySpawnLocally)
                {
                    var args = new object[]
                    {
                        (int)CharacterManager.CharacterInstantiationTypes.Temporary,
                        "NewPlayerPrefab",
                        _UID,
                        string.Empty // dont send a creator UID, otherwise it links the current summon (used by Conjure Ghost)
                    };

                    prefab = PhotonNetwork.InstantiateSceneObject("_characters/NewPlayerPrefab", _position, Quaternion.identity, 0, args);
                    prefab.SetActive(false);

                    character = prefab.GetComponent<Character>();
                }
                else // local spawn
                {
                    prefab = null;

                    prefab = GameObject.Instantiate(Resources.Load<GameObject>("_characters/NewPlayerPrefab"));
                    prefab.SetActive(false);

                    prefab.transform.position = _position;

                    character = prefab.GetComponent<Character>();

                    At.SetValue("NewPlayerPrefab", "m_prefabPath", character);
                    At.SetValue(CharacterManager.CharacterInstantiationTypes.Temporary, "m_instantiationType", character);
                    At.SetValue(string.Empty, "m_intantiationExtraData", character);
                }

                character.SetUID(_UID);

                //FixStats(prefab.GetComponent<Character>());

                var view = sceneView > 0 ? sceneView : PhotonNetwork.AllocateSceneViewID();

                if (string.IsNullOrEmpty(spawnCallbackUID))
                {
                    spawnCallbackUID = _UID;
                }

                prefab.SetActive(true);

                if (!onlySpawnLocally)
                    SLRPCManager.Instance.SpawnCharacter(_UID, view, _name, visualData, addCombatAI, spawnCallbackUID, extraRpcData);
                else
                    SLPlugin.Instance.StartCoroutine(SpawnCharacterCoroutine(_UID, view, _name, visualData, addCombatAI, spawnCallbackUID, extraRpcData));

                return prefab;
            }
            catch (Exception e)
            {
                SL.Log("Exception spawning character: " + e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Helper to add basic combat AI to a Character.
        /// </summary>
        public static CharacterAI SetupBasicAI(Character _char)
        {
            if (_char.GetComponent<CharacterAI>() is CharacterAI comp)
            {
                SL.Log($"This character '{_char.Name}' is already a CharacterAI!");
                return comp;
            }

            // add required components for AIs (no setup required)
            _char.gameObject.AddComponent<NavMeshAgent>();
            _char.gameObject.AddComponent<AISquadMember>();
            _char.gameObject.AddComponent<EditorCharacterAILoadAI>();

            // add our basic AIStatesPrefab to a CharacterAI component. This is the prefab set up by SetupBasicAIPrefab(), below.
            CharacterAI charAI = _char.gameObject.AddComponent<CharacterAI>();
            At.SetValue(_char, "m_character", charAI);
            charAI.AIStatesPrefab = BasicAIPrefab.GetComponent<AIRoot>();

            // initialize the AI States (not entirely necessary, but helpful if we want to do something with the AI immediately after)
            At.Call(charAI, "GetAIStates", null);

            return charAI;
        }

        // ======================== INTERNAL ======================== //

        internal void Awake()
        {
            SetupBasicAIPrefab();
        }

        // public static bool IsRealScene(Scene scene) => true; // CustomScenes.IsRealScene(scene);

        // called from a patch on level unload
        internal static void CleanupCharacters()
        {
            if (ActiveCharacters.Count > 0 && !PhotonNetwork.isNonMasterClientInRoom)
            {
                //SL.Log("Cleaning up " + ActiveCharacters.Count + " characters.");

                for (int i = ActiveCharacters.Count - 1; i >= 0; i--)
                {
                    var info = ActiveCharacters[i];

                    if (info.ActiveCharacter)
                    {
                        DestroyCharacterRPC(info.ActiveCharacter);
                    }
                    else
                    {
                        //SL.Log("Trying to destroy a null or destroyed character!");
                        ActiveCharacters.RemoveAt(i);
                    }

                }
            }
        }

        internal static void RequestSpawnedCharacters()
        {
            SLRPCManager.Instance.photonView.RPC(nameof(SLRPCManager.RPC_RequestCharacters), PhotonTargets.MasterClient);
        }

        // called from a patch on SaveManager.Save
        internal static void SaveCharacters()
        {
            if (PhotonNetwork.isNonMasterClientInRoom)
                return;

            var sceneSaveDataList = new List<SL_CharacterSaveData>();
            var followerDataList = new List<SL_CharacterSaveData>();

            foreach (var info in ActiveCharacters)
            {
                if (info.Template != null)
                {
                    if (info.Template.SaveType == CharSaveType.Scene)
                    {
                        var data = info.ToSaveData();
                        if (data != null)
                            sceneSaveDataList.Add(data);
                    }
                    else if (info.Template.SaveType == CharSaveType.Follower)
                    {
                        var data = info.ToSaveData();
                        if (data != null)
                            followerDataList.Add(data);
                    }
                }
            }

            SL.Log("Saving " + (sceneSaveDataList.Count + followerDataList.Count) + " characters");

            if (sceneSaveDataList.Count > 0)
                SaveListOfData(sceneSaveDataList.ToArray(), CharSaveType.Scene);

            if (followerDataList.Count > 0)
                SaveListOfData(followerDataList.ToArray(), CharSaveType.Follower);
        }

        internal static void SaveListOfData(SL_CharacterSaveData[] list, CharSaveType type)
        {
            var savePath = GetCurrentSavePath(type);

            if (File.Exists(savePath))
                File.Delete(savePath);

            using (var file = File.Create(savePath))
            {
                var serializer = Serializer.GetXmlSerializer(typeof(SL_CharacterSaveData[]));
                serializer.Serialize(file, list);
            }
        }

        internal static string GetCurrentSavePath(CharSaveType saveType)
        {
            var saveFolder = $@"{SLSaveManager.GetSaveFolderForWorldHost()}\{SLSaveManager.CHARACTERS_FOLDER}";

            return saveType == CharSaveType.Scene
                ? saveFolder + $@"\{SceneManager.GetActiveScene().name}.chardata"
                : saveFolder + $@"\followers.chardata";
        }

        private static IEnumerator TryLoadSaveData()
        {
            while (!NetworkLevelLoader.Instance.AllPlayerReadyToContinue && NetworkLevelLoader.Instance.IsGameplayPaused)
                yield return null;
            
            if (!WasLastAreaReset)
            {
                TryLoadSaveData(CharSaveType.Scene);
            }
            else
            {
                var path = GetCurrentSavePath(CharSaveType.Scene);
                if (File.Exists(path))
                    File.Delete(path);
            }

            TryLoadSaveData(CharSaveType.Follower);
        }

        internal static void TryLoadSaveData(CharSaveType type)
        {
            var savePath = GetCurrentSavePath(type);

            if (File.Exists(savePath))
            {
                using (var file = File.OpenRead(savePath))
                {
                    var serializer = Serializer.GetXmlSerializer(typeof(SL_CharacterSaveData[]));
                    var list = serializer.Deserialize(file) as SL_CharacterSaveData[];

                    var playerPos = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;

                    foreach (var saveData in list)
                    {
                        if (type == CharSaveType.Scene)
                        {
                            var character = CharacterManager.Instance.GetCharacter(saveData.CharacterUID);
                            if (!character)
                            {
                                SL.LogWarning($"Trying to apply a Scene-type SL_CharacterSaveData but could not find character with UID '{saveData.CharacterUID}'");
                                continue;
                            }

                            saveData.ApplyToCharacter(character);
                        }
                        else
                        {
                            // Followers loaded from a save should be re-spawned.
                            if (!Templates.TryGetValue(saveData.TemplateUID, out SL_Character template))
                            {
                                SL.LogWarning($"Loading a follower save data, but cannot find any SL_Character template with the UID '{saveData.TemplateUID}'");
                                continue;
                            }

                            var character = template.Spawn(playerPos, saveData.CharacterUID, saveData.ExtraRPCData);
                            saveData.ApplyToCharacter(character);
                        }
                    }
                }
            }
        }

        internal static IEnumerator SpawnCharacterCoroutine(string charUID, int viewID, string name, string visualData, bool addCombatAI, string spawnCallbackUID, string extraRpcData)
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
            {
                name = "SL_Character";
            }

            // set name
            character.name = $"{name}_{charUID}";

            if (!string.IsNullOrEmpty(visualData))
            {
                //SL.Instance.StartCoroutine(SL_Character.SetVisuals(character, visualData));
                SLPlugin.Instance.StartCoroutine(SL_Character.SetVisuals(character, visualData));
            }

            if (addCombatAI)
            {
                SetupBasicAI(character);
            }

            // invoke OnSpawn callback
            if (Templates.ContainsKey(spawnCallbackUID))
            {
                var template = Templates[spawnCallbackUID];

                template.INTERNAL_OnSpawn(character, extraRpcData);
            }

            character.gameObject.SetActive(true);

            // fix Photon View component
            if (character.gameObject.GetComponent<PhotonView>() is PhotonView view)
            {
                int id = view.viewID;
                GameObject.DestroyImmediate(view);

                if (id > 0)
                {
                    PhotonNetwork.UnAllocateViewID(view.viewID);
                }
            }

            // setup new view
            var pView = character.gameObject.AddComponent<PhotonView>();
            pView.viewID = viewID;
            pView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
            pView.onSerializeRigidBodyOption = OnSerializeRigidBody.All;
            pView.synchronization = ViewSynchronization.Unreliable;

            // fix photonview serialization components
            if (pView.ObservedComponents == null || pView.ObservedComponents.Count < 1)
            {
                pView.ObservedComponents = new List<Component>()
                {
                    character
                };
            }

            float t = 0f;
            while (t < 0.5f)
            {
                yield return null;
                t += Time.deltaTime;
            }

            character.gameObject.SetActive(false);
            character.gameObject.SetActive(true);
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
                ActiveCharacters.Remove(query.First());

            }

            var m_characters = At.GetValue("m_characters", CharacterManager.Instance) as DictionaryExt<string, Character>;
            if (m_characters.ContainsKey(character.UID))
                m_characters.Remove(character.UID);

            var pv = character.photonView;
            int view = pv.viewID;

            //  DestroyImmediate
            GameObject.DestroyImmediate(character.gameObject);

            if (character)
            {
                SL.LogError("ERROR - Could not seem to destroy character " + character.UID);
            }
            else
            {
                PhotonNetwork.UnAllocateViewID(view);
            }
        }

        internal static void AddActiveCharacter(CustomSpawnInfo info)
        {
            if (!ActiveCharacters.Contains(info))
                ActiveCharacters.Add(info);
        }

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
                {
                    GameObject.Destroy(pStats);
                }
            }
            // add new CharacterStats
            var newStats = character.gameObject.AddComponent<CharacterStats>();
            At.SetValue(newStats, "m_characterStats", character);
            SetupBlankCharacterStats(newStats);
        }

        // ================= OTHER INTERNAL ================== //

        /// <summary>
        /// Resets a CharacterStats to have all default stats (default for the Player).
        /// </summary>
        /// <param name="stats"></param>
        public static void SetupBlankCharacterStats(CharacterStats stats)
        {
            At.SetValue(new Stat[] { new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f) },
                typeof(CharacterStats), stats, "m_damageResistance");
            At.SetValue(new Stat[] { new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f), new Stat(0f) },
                typeof(CharacterStats), stats, "m_damageProtection");
            At.SetValue(new Stat[] { new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f), new Stat(1f) },
                typeof(CharacterStats), stats, "m_damageTypesModifier");

            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_heatRegenRate");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_coldRegenRate");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_heatProtection");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_coldProtection");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_corruptionResistance");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_waterproof");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_healthRegen");
            At.SetValue(new Stat(0f), typeof(CharacterStats), stats, "m_manaRegen");
            At.SetValue(new TagSourceSelector[0], typeof(CharacterStats), stats, "m_statusEffectsNaturalImmunity");
        }

        /// <summary>
        /// This is a completely custom AI States setup from scratch. It copies the Summoned Ghost AI.
        /// </summary>
        private static void SetupBasicAIPrefab()
        {
            // Check if we've already set up the Prefab...
            if (m_basicAIPrefab)
            {
                return;
            }

            var _AIStatesPrefab = new GameObject("AIRoot").AddComponent<AIRoot>();
            _AIStatesPrefab.gameObject.SetActive(false);

            // -- create base state objects --

            // state 1: Wander
            var wanderState = new GameObject("1_Wander").AddComponent<AISWander>();
            wanderState.transform.parent = _AIStatesPrefab.transform;

            // state 2: Suspicious
            var susState = new GameObject("2_Suspicious").AddComponent<AISSuspicious>();
            susState.transform.parent = _AIStatesPrefab.transform;

            //state 3: alert
            var alertState = new GameObject("3_Alert").AddComponent<AISSuspicious>();
            alertState.transform.parent = _AIStatesPrefab.transform;

            //state 4: Combat
            var combatState = new GameObject("4_Combat").AddComponent<AISCombatMelee>();
            combatState.transform.parent = _AIStatesPrefab.transform;

            // ---- setup actual state parameters and links ----

            // setup 1 - Wander

            wanderState.SpeedModif = 1.1f; // set custom state speed
            wanderState.ContagionRange = 20f;

            var wanderDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
            wanderDetection.transform.parent = wanderState.transform;
            var wanderDetectEffects = new GameObject("DetectEffects").AddComponent<AIESwitchState>();
            wanderDetectEffects.ToState = susState;
            wanderDetectEffects.transform.parent = wanderDetection.transform;
            wanderDetection.DetectEffectsTrans = wanderDetectEffects.transform;

            //setup 2 - Suspicious

            susState.SpeedModif = 1.75f;
            susState.SuspiciousDuration = 5f;
            susState.Range = 30;
            susState.WanderFar = true;
            susState.TurnModif = 0.9f;

            var susEnd = new GameObject("EndSuspiciousEffects").AddComponent<AIESwitchState>();
            susEnd.ToState = wanderState;
            var sheathe = susEnd.gameObject.AddComponent<AIESheathe>();
            sheathe.Sheathed = true;
            susEnd.transform.parent = susState.transform;
            susState.EndSuspiciousEffectsTrans = susEnd.transform;

            var susDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
            susDetection.transform.parent = susState.transform;
            var susDetectEffects = new GameObject("DetectEffects").AddComponent<AIESwitchState>();
            susDetectEffects.ToState = combatState;
            susDetectEffects.transform.parent = susDetection.transform;
            susDetection.DetectEffectsTrans = susDetectEffects.transform;
            var susSuspiciousEffects = new GameObject("SuspiciousEffects").AddComponent<AIESwitchState>();
            susSuspiciousEffects.ToState = alertState;
            susSuspiciousEffects.transform.parent = susDetection.transform;
            susDetection.SuspiciousEffectsTrans = susSuspiciousEffects.transform;

            // setup 3 - alert

            alertState.SpeedModif = 1.75f;

            var alertEnd = new GameObject("EndSuspiciousEffects").AddComponent<AIESwitchState>();
            alertEnd.ToState = susState;
            var alertsheathe = alertEnd.gameObject.AddComponent<AIESheathe>();
            alertsheathe.Sheathed = true;
            alertEnd.transform.parent = alertState.transform;
            alertState.EndSuspiciousEffectsTrans = alertEnd.transform;

            var alertDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
            alertDetection.transform.parent = alertState.transform;
            var alertDetectEffects = new GameObject("DetectEffects").AddComponent<AIESwitchState>();
            alertDetectEffects.ToState = combatState;
            alertDetectEffects.transform.parent = alertDetection.transform;
            alertDetection.DetectEffectsTrans = alertDetectEffects.transform;

            // setup 4 - Combat

            combatState.ChargeTime = new Vector2(4, 8);
            combatState.TargetVulnerableChargeTimeMult = 0.5f;
            combatState.ChargeAttackRangeMult = 1;
            combatState.ChargeAttackTimeToAttack = 0.4f;
            combatState.ChargeStartRangeMult = new Vector2(0.8f, 3.0f);
            combatState.AttackPatterns = new AttackPattern[]
            {
                new AttackPattern { ID = 0, Chance = 20, Range = new Vector2(0.9f, 2.5f), Attacks = new AttackPattern.AtkTypes[] { AttackPattern.AtkTypes.Normal } },
                new AttackPattern { ID = 1, Chance = 15, Range = new Vector2(0.0f, 2.9f), Attacks = new AttackPattern.AtkTypes[] { AttackPattern.AtkTypes.Normal, AttackPattern.AtkTypes.Normal } },
                new AttackPattern { ID = 2, Chance = 30, Range = new Vector2(0.0f, 1.5f), Attacks = new AttackPattern.AtkTypes[] { AttackPattern.AtkTypes.Special }},
                new AttackPattern { ID = 3, Chance = 30, Range = new Vector2(0.0f, 1.5f), Attacks = new AttackPattern.AtkTypes[] { AttackPattern.AtkTypes.Normal, AttackPattern.AtkTypes.Special }},
                new AttackPattern { ID = 4, Chance = 30, Range = new Vector2(0.0f, 1.3f), Attacks = new AttackPattern.AtkTypes[] { AttackPattern.AtkTypes.Normal, AttackPattern.AtkTypes.Normal, AttackPattern.AtkTypes.Special }}
            };
            combatState.SpeedModifs = new float[] { 0.8f, 1.3f, 1.7f };
            combatState.ChanceToAttack = 75;
            combatState.KnowsUnblockable = true;
            combatState.DodgeCooldown = 3f;
            combatState.CanBlock = true;
            combatState.CanDodge = true;

            var combatDetect = new GameObject("Detection").AddComponent<AICEnemyDetection>();
            combatDetect.transform.parent = combatState.transform;
            var combatEnd = new GameObject("EndCombatEffects").AddComponent<AIESwitchState>();
            combatEnd.ToState = wanderState;
            combatEnd.transform.parent = combatState.transform;

            m_basicAIPrefab = _AIStatesPrefab.gameObject;
            GameObject.DontDestroyOnLoad(m_basicAIPrefab);
            m_basicAIPrefab.SetActive(false);
        }

        // legacy support

        [Obsolete("Use CustomCharacters.CreateCharacter instead (naming change)")]
        public static GameObject InstantiatePlayerPrefab(Vector3 _position, string _UID)
        {
            return SpawnCharacter(_position, _UID);
        }

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
                At.SetValue(UID.Generate(), "m_uid", character);
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
    }
}
