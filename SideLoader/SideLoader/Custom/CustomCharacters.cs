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

		public static void AddActiveCharacter(Character character)
		{
			if (!ActiveCharacters.Contains(character))
			{
				ActiveCharacters.Add(character);
			}
		}

		public static void DestroyCharacterRPC(Character character)
		{
			RPCManager.Instance.DestroyCharacter(character.UID);
		}

		public static void DestroyCharacter(Character character)
		{
			if (ActiveCharacters.Contains(character))
			{
				ActiveCharacters.Remove(character);

				var pv = character.photonView;
				int view = pv.viewID;
				GameObject.DestroyImmediate(character.gameObject);
				PhotonNetwork.UnAllocateViewID(view);
			}
		}

		public static GameObject CreateCharacter(Vector3 _position, string _UID)
		{
			return CreateCharacter(_position, _UID, "SL_Character");
		}

		public static GameObject CreateCharacter(Vector3 _position, string _UID, string _name)
		{
			// setup Player Prefab
			var playerPrefab = PhotonNetwork.InstantiateSceneObject("_characters/NewPlayerPrefab", _position, Quaternion.identity, 0, new object[]
			{
				(int)CharacterManager.CharacterInstantiationTypes.Temporary,
				"NewPlayerPrefab",
				_UID,
				string.Empty // dont send a creator UID, otherwise it links the current summon (used by Conjure Ghost)
			});

			playerPrefab.SetActive(false);

			FixStats(playerPrefab.GetComponent<Character>());

			RPCManager.Instance.photonView.RPC("RPCSpawnCharacter", PhotonTargets.All, _UID, PhotonNetwork.AllocateSceneViewID(), _name);

			return playerPrefab;
		}

		// ============= main internal ==============

		internal void Awake()
        {
            Instance = this;

			SetupBasicAIPrefab();

			SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
		}

		private void SceneManager_sceneUnloaded(Scene arg0)
		{
			foreach (var character in ActiveCharacters)
			{
				DestroyCharacterRPC(character);
			}
		}

		/// <summary>
		/// Removes PlayerCharacterStats and replaces with CharacterStats
		/// </summary>
		private static void FixStats(Character character)
		{
			// remove PlayerCharacterStats
			var pStats = character.GetComponent<PlayerCharacterStats>();
			pStats.enabled = false;
			GameObject.DestroyImmediate(pStats);

			// add new CharacterStats
			var newStats = character.gameObject.AddComponent<CharacterStats>();
			At.SetValue(newStats, typeof(Character), character, "m_characterStats");
			SetupBlankCharacterStats(newStats);
		}

		/// <summary>
		/// INTERNAL. Coroutine that executes locally for all clients.
		/// </summary>
		public static IEnumerator SpawnCharacterCoroutine(string charUID, int viewID, string name)
		{
			// get character from manager
			Character character = CharacterManager.Instance.GetCharacter(charUID);
			while (character == null)
			{
				character = CharacterManager.Instance.GetCharacter(charUID);
				yield return null;
			}

			// add to cache list
			AddActiveCharacter(character);

			// set name
			character.name = $"{name}_{charUID}";
			At.SetValue("", typeof(Character), character, "m_nameLocKey");
			At.SetValue(name, typeof(Character), character, "m_name");

			// fix stats for non-hosts
			if (PhotonNetwork.isNonMasterClientInRoom && character.gameObject.GetComponent<PlayerCharacterStats>())
			{
				FixStats(character);
			}

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

		// ========= misc helpers ==========

		/// <summary>
		/// Add basic combat AI to a Character.
		/// </summary>
		public static CharacterAI SetupBasicAI(Character _char)
		{
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
			At.Call(charAI, "GetAIStates", new object[0]);

			return charAI;
		}

		/// <summary>
		/// Finds a GameObject with _gameObjectName and clones it into a new Character (if it contains a Character component)
		/// </summary>
		public static void CloneCharacter(string _gameObjectName)
		{
			if (GameObject.Find(_gameObjectName) is GameObject obj && obj.GetComponent<Character>() is Character c)
			{
				CloneCharacter(c);
			}
		}

		/// <summary>
		/// Clone a character by providing the component directly
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
				var clone = Instantiate(targetObj);
				clone.SetActive(false);

				// fix original
				_targetCharacter.DisableAfterInit = disable;
				targetObj.SetActive(true);

				// fix clone UIDs, etc
				var character = clone.GetComponent<Character>();
				At.SetValue(UID.Generate(), typeof(Character), character, "m_uid");
				clone.name = "[CLONE] " + character.Name + "_" + character.UID;

				// allocate a scene view ID (will need RPC if to work in multiplayer)
				clone.GetPhotonView().viewID = PhotonNetwork.AllocateSceneViewID();

				var items = character.GetComponentsInChildren<Item>();
				for (int i = 0; i < items.Length; i++)
				{
					var item = items[i];

					var new_item = ItemManager.Instance.GenerateItemNetwork(item.ItemID);
					new_item.transform.parent = item.transform.parent;

					DestroyImmediate(item);
				}

				//// todo same for droptable components
				//var lootable = clone.GetComponent<LootableOnDeath>();

				//var oldTables = new List<GameObject>();

				foreach (var component in clone.GetComponentsInChildren<MonoBehaviour>())
				{
					try
					{
						At.Call(component, "Awake", new object[0]);
					}
					catch { }
				}

				//var charAI = clone.GetComponent<CharacterAI>();

				//var navmeshAgent = clone.GetComponent<UnityEngine.AI.NavMeshAgent>();
				//At.SetValue(navmeshAgent, typeof(CharacterAI), charAI, "m_navMeshAgent");

				//var airoot = clone.GetComponentInChildren<AIRoot>();
				//At.SetValue(charAI, typeof(AIRoot), airoot, "m_charAI");

				clone.SetActive(true);

				clone.transform.position = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
			}
			catch (Exception e)
			{
				SL.Log("Error cloning enemy: " + e.Message + "\r\nStack: " + e.StackTrace, 1);
			}
		}

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

		// ================= INTERNAL ================== //

		private void SetupBasicAIPrefab()
		{
			if (BasicAIPrefab != null) { return; }

			var _AIStatesPrefab = new GameObject("AIRoot").AddComponent<AIRoot>();
			_AIStatesPrefab.gameObject.SetActive(false);

			// -- create base state objects --

			// state 1: Wander
			var wanderState = new GameObject("1_Wander").AddComponent<AISWander>();
			wanderState.transform.parent = _AIStatesPrefab.transform;

			// state 2: Suspicious
			var susState = new GameObject("2_Suspicious").AddComponent<AISSuspicious>();
			susState.SuspiciousDuration = 3f;
			susState.Range = 10;
			susState.WanderFar = true;
			susState.TurnModif = 0.5f;
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
