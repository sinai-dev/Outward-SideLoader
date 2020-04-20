using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace SideLoader
{
	/// <summary>
	/// This class just contains some useful helper functions for setting up custom NPCs or AI Characters
	/// </summary>
	public class CustomCharacters : MonoBehaviour
    {
        public static CustomCharacters Instance;

        public static GameObject BasicAIPrefab = null;

        internal void Awake()
        {
            Instance = this;

			SetupBasicAIPrefab();
		}

		public static GameObject InstantiatePlayerPrefab(Vector3 _position, string _UID)
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

			return playerPrefab;
		}

		public static CharacterAI SetupBasicAI(Character _char)
		{
			// add required components for AIs (no setup required)
			_char.gameObject.AddComponent<NavMeshAgent>();
			_char.gameObject.AddComponent<AISquadMember>();
			_char.gameObject.AddComponent<EditorCharacterAILoadAI>();

			// add our basic AIStatesPrefab to a CharacterAI component. This is the prefab set up by SetupBasicAIPrefab(), below.
			CharacterAI charAI = _char.gameObject.AddComponent<CharacterAI>();
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
	}
}
