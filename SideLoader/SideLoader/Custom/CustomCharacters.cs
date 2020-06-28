using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace SideLoader
{
	/// <summary>
	/// This class contains helpers for creating custom Characters, and will manage their PhotonView IDs and clean them up on scene changes.
	/// </summary>
	public class CustomCharacters : MonoBehaviour
	{
		public static CustomCharacters Instance;

		public static GameObject BasicAIPrefab { get; private set; }

		private static readonly List<Character> ActiveCharacters = new List<Character>();

		public static event Action INTERNAL_SpawnCharacters;

		/// <summary>
		/// Key: Spawn callback UID (generally template UID), Value: SL_Character with OnSpawn event to invoke
		/// </summary>
		public static Dictionary<string, SL_Character> OnSpawnCallbacks = new Dictionary<string, SL_Character>();

		// ======== harmony patches =========

		// Just catches a harmless null ref exception, hiding it until I figure out a cleaner fix
		[HarmonyPatch(typeof(Character), "ProcessOnEnable")]
		public class Character_ProcessOnEnable
        {
			[HarmonyFinalizer]
			public static Exception Finalizer()
            {
				return null;
            }
		}

		// This harmony patch is to sneak into when the game applies characters.
		// I figure it's best to do it at the same time.
		[HarmonyPatch(typeof(NetworkLevelLoader), "MidLoadLevel")]
		public class NetworkLevelLoader_MidLoadLevel
		{
			[HarmonyPostfix]
			public static void Postfix()
			{
				var scene = SceneManager.GetActiveScene();
				if (IsRealScene(scene))
				{
					SL.Log($"Spawning characters ({scene.name})");

					SL.TryInvoke(INTERNAL_SpawnCharacters);
				}
			}
		}

		// Like the last patch, we sneak into when the game should have destroyed previous scene characters to cleanup there.
		[HarmonyPatch(typeof(CharacterManager), "ClearNonPersitentCharacters")]
		public class CharacterManager_ClearNonPersitentCharacters
		{
			[HarmonyPrefix]
			public static void Prefix()
            {
				CleanupCharacters();
            }
        }

		// ======================== PUBLIC HELPERS ======================== //

		/// <summary>
		/// Use this to cleanup a custom character. This will send out an RPC.
		/// </summary>
		/// <param name="character">The Character to destroy.</param>
		public static void DestroyCharacterRPC(Character character)
		{
			RPCManager.Instance.DestroyCharacter(character.UID);
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
		public static Character CreateCharacter(SL_Character template, Vector3 position, string characterUID = null, string extraRpcData = null)
        {
			characterUID = characterUID ?? template.UID;

			var character = CreateCharacter(
				position, 
				characterUID, 
				template.Name, 
				template.CharacterVisualsData?.ToString(), 
				template.AddCombatAI, 
				template.UID,
				extraRpcData
			).GetComponent<Character>();

			return character;
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
		public static GameObject CreateCharacter(Vector3 pos, string uid, string name = "SL_Character", bool addCombatAI = false, string extraRpcData = null)
		{
			return CreateCharacter(pos, uid, name, null, addCombatAI, uid, extraRpcData);
		}

		/// <summary>
		/// Instantiates a new human character with the attributes provided. Only one client should call this.
		/// This is the main CreateCharacter method, called by the other CreateCharacter methods.
		/// </summary>
		/// <param name="_position">The spawn position for the character.</param>
		/// <param name="_UID">The UID for the character.</param>
		/// <param name="_name">The Name of your custom character.</param>
		/// <param name="spawnCallbackUID">Optional custom UID for the spawn callback (checks against registered SL_Character template UIDs)</param>
		/// <param name="addCombatAI">Whether to add basic combat AI to the character</param>
		/// <param name="visualData">Optional visual data (network data). Use SL_Character.VisualData.ToString().</param>
		/// <param name="extraRpcData">Optional extra RPC data to send with the spawn</param>
		/// <returns>The custom character (instantly for executing client)</returns>
		public static GameObject CreateCharacter(Vector3 _position, string _UID, string _name, string visualData = null, bool addCombatAI = false, string spawnCallbackUID = null, string extraRpcData = null)
		{
			SL.Log($"Spawning character '{_name}', _UID: {_UID}, spawnCallbackUID: {spawnCallbackUID}");

			// setup Player Prefab
			var prefab = PhotonNetwork.InstantiateSceneObject("_characters/NewPlayerPrefab", _position, Quaternion.identity, 0, new object[]
			{
				(int)CharacterManager.CharacterInstantiationTypes.Temporary,
				"NewPlayerPrefab",
				_UID,
				string.Empty // dont send a creator UID, otherwise it links the current summon (used by Conjure Ghost)
			});

			prefab.SetActive(false);

			var character = prefab.GetComponent<Character>();

			//At.SetValue(new UID(_UID), typeof(Character), character, "m_uid");
			character.SetUID(_UID);

			//FixStats(prefab.GetComponent<Character>());

			var view = PhotonNetwork.AllocateSceneViewID();

			if (string.IsNullOrEmpty(spawnCallbackUID))
            {
				spawnCallbackUID = _UID;
            }

			prefab.SetActive(true);

			RPCManager.Instance.SpawnCharacter(_UID, view, _name, visualData, addCombatAI, spawnCallbackUID, extraRpcData);

			return prefab;
		}

		/// <summary>
		/// INTERNAL. Coroutine that executes locally for all clients to spawn a Character (continues directly from CreateCharacter)
		/// </summary>
		public static IEnumerator SpawnCharacterCoroutine(string charUID, int viewID, string name, string visualData, bool addCombatAI, string spawnCallbackUID, string extraRpcData)
		{
			// get character from manager
			Character character = CharacterManager.Instance.GetCharacter(charUID);
			while (!character)
			{
				yield return null;
				character = CharacterManager.Instance.GetCharacter(charUID);
			}

			// add to cache list
			AddActiveCharacter(character);

			if (string.IsNullOrEmpty(name))
			{
				name = "SL_Character";
			}

			// set name
			character.name = $"{name}_{charUID}";

			if (!string.IsNullOrEmpty(visualData))
			{
				Instance.StartCoroutine(SL_Character.SetVisuals(character, visualData));
			}

			if (addCombatAI)
			{
				SetupBasicAI(character);
			}

			// invoke OnSpawn callback
			if (OnSpawnCallbacks.ContainsKey(spawnCallbackUID))
			{
				var template = OnSpawnCallbacks[spawnCallbackUID];

				template.INTERNAL_OnSpawn(character, extraRpcData);
			}

			character.gameObject.SetActive(true);

			// fix Photon View component
			if (character.gameObject.GetPhotonView() is PhotonView view)
			{
				int id = view.viewID;
				DestroyImmediate(view);

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

			yield return new WaitForSeconds(0.5f);

			character.gameObject.SetActive(false);
			character.gameObject.SetActive(true);
		}

		/// <summary>
		/// Add basic combat AI to a Character.
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
			At.SetValue(_char, typeof(CharacterAI), charAI, "m_character");
			charAI.AIStatesPrefab = BasicAIPrefab.GetComponent<AIRoot>();

			// remove unwanted components
			if (_char.GetComponent<NavMeshObstacle>() is NavMeshObstacle navObstacle)
			{
				Destroy(navObstacle);
			}

			// initialize the AI States (not entirely necessary, but helpful if we want to do something with the AI immediately after)
			At.Call(typeof(CharacterAI), charAI, "GetAIStates", null, new object[0]);

			return charAI;
		}


		// ======================== INTERNAL ======================== //

		internal void Awake()
        {
            Instance = this;

			SetupBasicAIPrefab();
		}

		private static bool IsRealScene(Scene scene)
		{
			var name = scene.name.ToLower();

			return !(name.Contains("lowmemory") || name.Contains("mainmenu"));
		}

		/// <summary>
		/// The host calls this on Scene Changes to cleanup non-persistent characters (currently all SL_Character are non-persistent)
		/// </summary>
		public static void CleanupCharacters()
        {
			if (ActiveCharacters.Count > 0 && !PhotonNetwork.isNonMasterClientInRoom)
            {
				SL.Log("Cleaning up " + ActiveCharacters.Count + " characters.");

				// Reverse iteration to remove elements from a list
				for (int i = ActiveCharacters.Count - 1; i >= 0; i--)
				{
					var character = ActiveCharacters[i];

					if (character)
					{
						DestroyCharacterRPC(character);
					}
					else
					{
						SL.Log("Trying to destroy a null or destroyed character!");
					}
				}
			}
		}

		/// <summary>
		/// Used internally to destroy a Character locally. Use DestroyCharacterRPC to cleanup a character.
		/// </summary>
		public static void DestroyCharacterLocal(Character character)
		{
			if (!character)
            {
				SL.Log("Trying to destroy a character that is null or already destroyed!");
				return;
            }

			character.gameObject.SetActive(false);

			// Reverse iteration to remove elements from a list
			for (int i = ActiveCharacters.Count - 1; i >= 0; i--)
            {
				var c = ActiveCharacters[i];
				if (c)
                {
					if (c.UID == character.UID)
					{
						ActiveCharacters.RemoveAt(i);
					}
                }
            }

			var m_characters = (DictionaryExt<string, Character>)At.GetValue(typeof(CharacterManager), CharacterManager.Instance, "m_characters");
			if (m_characters.ContainsKey(character.UID))
            {
				m_characters.Remove(character.UID);
            }

			var pv = character.photonView;
			int view = pv.viewID;

			//  DestroyImmediate
			GameObject.DestroyImmediate(character.gameObject);			

			if (character)
            {
				Debug.LogError("ERROR - Could not seem to destroy character " + character.UID);
            }
			else
            {
				PhotonNetwork.UnAllocateViewID(view);
			}
		}

		/// <summary>
		/// Used internally by this class. Use CreateCharacter to create a new character.
		/// </summary>
		public static void AddActiveCharacter(Character character)
		{
			if (!ActiveCharacters.Contains(character))
			{
				ActiveCharacters.Add(character);
			}
		}

		/// <summary>
		/// Removes PlayerCharacterStats and replaces with CharacterStats
		/// </summary>
		public static void FixStats(Character character)
		{
			// remove PlayerCharacterStats
			if (character.GetComponent<PlayerCharacterStats>() is PlayerCharacterStats pStats)
			{
				pStats.enabled = false;
				DestroyImmediate(pStats);
				if (character.GetComponent<PlayerCharacterStats>())
				{
					Destroy(pStats);
				}
			}
			// add new CharacterStats
			var newStats = character.gameObject.AddComponent<CharacterStats>();
			At.SetValue(newStats, typeof(Character), character, "m_characterStats");
			SetupBlankCharacterStats(newStats);
		}
		

		// ===================== A test I did with cloning enemies. It mostly works. =======================

		///// <summary>
		///// Finds a GameObject with _gameObjectName and clones it into a new Character (if it contains a Character component)
		///// </summary>
		//public static void CloneCharacter(string _gameObjectName)
		//{
		//	if (GameObject.Find(_gameObjectName) is GameObject obj && obj.GetComponent<Character>() is Character c)
		//	{
		//		CloneCharacter(c);
		//	}
		//}

		///// <summary>
		///// Clone a character by providing the component directly
		///// </summary>
		//public static void CloneCharacter(Character _targetCharacter)
		//{
		//	try
		//	{
		//		var targetObj = _targetCharacter.gameObject;

		//		// prepare original for clone
		//		targetObj.SetActive(false);
		//		bool disable = _targetCharacter.DisableAfterInit;
		//		_targetCharacter.DisableAfterInit = false;

		//		// make clone
		//		var clone = Instantiate(targetObj);
		//		clone.SetActive(false);

		//		// fix original
		//		_targetCharacter.DisableAfterInit = disable;
		//		targetObj.SetActive(true);

		//		// fix clone UIDs, etc
		//		var character = clone.GetComponent<Character>();
		//		At.SetValue(UID.Generate(), typeof(Character), character, "m_uid");
		//		clone.name = "[CLONE] " + character.Name + "_" + character.UID;

		//		// allocate a scene view ID (will need RPC if to work in multiplayer)
		//		clone.GetPhotonView().viewID = PhotonNetwork.AllocateSceneViewID();

		//		var items = character.GetComponentsInChildren<Item>();
		//		for (int i = 0; i < items.Length; i++)
		//		{
		//			var item = items[i];

		//			var new_item = ItemManager.Instance.GenerateItemNetwork(item.ItemID);
		//			new_item.transform.parent = item.transform.parent;

		//			DestroyImmediate(item);
		//		}

		//		//// todo same for droptable components
		//		//var lootable = clone.GetComponent<LootableOnDeath>();

		//		//var oldTables = new List<GameObject>();

		//		foreach (var component in clone.GetComponentsInChildren<MonoBehaviour>())
		//		{
		//			try
		//			{
		//				At.Call(typeof(MonoBehaviour), component, "Awake", null, new object[0]);
		//			}
		//			catch { }
		//		}

		//		//var charAI = clone.GetComponent<CharacterAI>();

		//		//var navmeshAgent = clone.GetComponent<UnityEngine.AI.NavMeshAgent>();
		//		//At.SetValue(navmeshAgent, typeof(CharacterAI), charAI, "m_navMeshAgent");

		//		//var airoot = clone.GetComponentInChildren<AIRoot>();
		//		//At.SetValue(charAI, typeof(AIRoot), airoot, "m_charAI");

		//		clone.SetActive(true);

		//		clone.transform.position = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
		//	}
		//	catch (Exception e)
		//	{
		//		SL.Log("Error cloning enemy: " + e.Message + "\r\nStack: " + e.StackTrace, 1);
		//	}
		//}


		// ================= OTHER INTERNAL ================== //

		private static void SetupBlankCharacterStats(CharacterStats stats)
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
		private void SetupBasicAIPrefab()
		{
			// Check if we've already set up the Prefab...
			if (BasicAIPrefab != null) { return; }

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

			wanderState.SpeedModif = 1.7f; // set custom state speed
			wanderState.ContagionRange = 20f;
			//wanderState.FollowMaxRange = 3f;

			var wanderDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
			wanderDetection.transform.parent = wanderState.transform;
			var wanderDetectEffects = new GameObject("DetectEffects").AddComponent<AIESwitchState>();
			wanderDetectEffects.ToState = susState;
			wanderDetectEffects.transform.parent = wanderDetection.transform;
			wanderDetection.DetectEffectsTrans = wanderDetectEffects.transform;

			//setup 2 - Suspicious

			susState.SpeedModif = 2f;
			susState.SuspiciousDuration = 3f;
			susState.Range = 10;
			susState.WanderFar = true;
			susState.TurnModif = 0.5f;

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

			alertState.SpeedModif = 2f;

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
			combatState.CanBlock = false;
			combatState.CanDodge = true;

			var combatDetect = new GameObject("Detection").AddComponent<AICEnemyDetection>();
			combatDetect.transform.parent = combatState.transform;
			var combatEnd = new GameObject("EndCombatEffects").AddComponent<AIESwitchState>();
			combatEnd.ToState = wanderState;
			combatEnd.transform.parent = combatState.transform;

			BasicAIPrefab = _AIStatesPrefab.gameObject;
			DontDestroyOnLoad(BasicAIPrefab);
			BasicAIPrefab.SetActive(false);
		}

		// legacy support

		[Obsolete("Use CustomCharacters.CreateCharacter instead (naming change)")]
		public static GameObject InstantiatePlayerPrefab(Vector3 _position, string _UID)
		{
			return CreateCharacter(_position, _UID);
		}
	}
}
