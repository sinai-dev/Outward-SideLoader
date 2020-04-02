using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace SideLoader_2
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
			CharacterAI charAI = _char.gameObject.AddComponent(new CharacterAI { AIStatesPrefab = BasicAIPrefab.GetComponent<AIRoot>() });

			// remove unwanted components
			if (_char.GetComponent<NavMeshObstacle>() is NavMeshObstacle navObstacle)
			{
				Destroy(navObstacle);
			}

			// initialize the AI States (not entirely necessary, but helpful if we want to do something with the AI immediately after)
			At.Call(charAI, "GetAIStates", new object[0]);

			return charAI;
		}

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
			var susState = new GameObject("2_Suspicious").AddComponent(new AISSuspicious { SuspiciousDuration = 3f, Range = 10, WanderFar = true, TurnModif = 0.5f, });
			susState.transform.parent = _AIStatesPrefab.transform;

			//state 3: alert
			var alertState = new GameObject("3_Alert").AddComponent(new AISSuspicious { });
			alertState.transform.parent = _AIStatesPrefab.transform;

			//state 4: Combat
			var combatState = new GameObject("4_Combat").AddComponent(new AISCombatMelee { });
			combatState.transform.parent = _AIStatesPrefab.transform;

			// ---- setup actual state parameters and links ----

			// setup 1 - Wander

			wanderState.SpeedModif = 1.7f; // set custom state speed
			wanderState.ContagionRange = 20f;
			//wanderState.FollowMaxRange = 3f;

			var wanderDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
			wanderDetection.transform.parent = wanderState.transform;
			var wanderDetectEffects = new GameObject("DetectEffects").AddComponent(new AIESwitchState { ToState = susState });
			wanderDetectEffects.transform.parent = wanderDetection.transform;
			wanderDetection.DetectEffectsTrans = wanderDetectEffects.transform;

			//setup 2 - Suspicious

			susState.SpeedModif = 2f;

			var susEnd = new GameObject("EndSuspiciousEffects").AddComponent(new AIESwitchState { ToState = wanderState });
			susEnd.gameObject.AddComponent(new AIESheathe { Sheathed = true });
			susEnd.transform.parent = susState.transform;
			susState.EndSuspiciousEffectsTrans = susEnd.transform;

			var susDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
			susDetection.transform.parent = susState.transform;
			var susDetectEffects = new GameObject("DetectEffects").AddComponent(new AIESwitchState { ToState = combatState });
			susDetectEffects.transform.parent = susDetection.transform;
			susDetection.DetectEffectsTrans = susDetectEffects.transform;
			var susSuspiciousEffects = new GameObject("SuspiciousEffects").AddComponent(new AIESwitchState { ToState = alertState });
			susSuspiciousEffects.transform.parent = susDetection.transform;
			susDetection.SuspiciousEffectsTrans = susSuspiciousEffects.transform;

			// setup 3 - alert

			alertState.SpeedModif = 2f;

			var alertEnd = new GameObject("EndSuspiciousEffects").AddComponent(new AIESwitchState { ToState = susState });
			alertEnd.gameObject.AddComponent(new AIESheathe { Sheathed = true });
			alertEnd.transform.parent = alertState.transform;
			alertState.EndSuspiciousEffectsTrans = alertEnd.transform;

			var alertDetection = new GameObject("Detection").AddComponent<AICEnemyDetection>();
			alertDetection.transform.parent = alertState.transform;
			var alertDetectEffects = new GameObject("DetectEffects").AddComponent(new AIESwitchState { ToState = combatState });
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
			var combatEnd = new GameObject("EndCombatEffects").AddComponent(new AIESwitchState { ToState = wanderState });
			combatEnd.transform.parent = combatState.transform;

			BasicAIPrefab = _AIStatesPrefab.gameObject;
			DontDestroyOnLoad(BasicAIPrefab);
			BasicAIPrefab.SetActive(false);
		}
	}
}
