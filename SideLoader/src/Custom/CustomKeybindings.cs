using BepInEx;
using BepInEx.Logging;
using Rewired;
using Rewired.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using RewiredDict = System.Collections.Generic.Dictionary<int, RewiredInputs>;
using System.Linq;
using SideLoader.Helpers;
using UnityEngine.UI;

// Credits:
// - Stian for the original version
// - Lasyan3 for helping port it over to BepInEx
// - johnathan-clause for logic fixes and improvements (https://github.com/johnathan-clause)
// - Sinai fixed for IL2CPP and added GetButton/GetButtonDown helper methods

namespace SideLoader
{
    public enum KeybindingsCategory : int
    {
        CustomKeybindings = 0,
        Menus = 1,
        //QuickSlot = 2, // Disabled due to being buggy.
        Actions = 4,
    }

    public enum InputType
    {
        Axis = 0,
        Button = 1
    }

    public enum ControlType : int
    {
        Keyboard,
        Gamepad,
        Both
    }

    public class KeybindInfo
    {
        public string name;
        public KeybindingsCategory category;
        public ControlType controllerType;
        public InputType type;

        internal int actionID;
        internal int[] keyIDs;

        public KeybindInfo(string name, KeybindingsCategory category, ControlType controllerType, InputType type)
        {
            this.name = name;
            this.category = category;
            this.controllerType = controllerType;
            this.type = type;

            this.keyIDs = new int[2];
            this.actionID = -1;
        }

        // Used by the public CustomKeybindings.GetKey(/Down) methods.
        internal bool GetKeyDown(out int playerID) => DoGetKey(true, out playerID);
        internal bool GetKey(out int playerID) => DoGetKey(false, out playerID);

        // Actual check if the keybind is down.
        internal bool DoGetKey(bool down, out int playerID)
        {
            playerID = -1;

            // check all players in case its split screen (this is the playerID returned)
            foreach (var entry in CustomKeybindings.PlayerInputManager)
            {
                var id = entry.Key;
                var player = entry.Value;

                // get the last controller used for this keybinding
                var lastInput = player.GetLastUsedControllerFirstElementMapWithAction(name);

                if (lastInput.aem == null)
                    continue;

                // update the internal ID used for the input
                keyIDs[id] = lastInput.aem.elementIdentifierId;

                // get the internal controller map
                var ctrlrList = ReInput.players.GetPlayer(id).controllers;
                var map = lastInput.aem.controllerMap;
                var controller = ctrlrList.GetController(map.controllerType, map.controllerId);

                // finally we just call this to check
                if (down)
                {
                    if (controller.GetButtonDownById(keyIDs[id]))
                    {
                        playerID = id;
                        return true;
                    }
                }
                else
                {
                    if (controller.GetButtonById(keyIDs[id]))
                    {
                        playerID = id;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public class CustomKeybindings
    {
        internal const int CUSTOM_CATEGORY = 0;

        public static RewiredDict PlayerInputManager
            => m_playerInputManager
            ?? (m_playerInputManager = (RewiredDict)At.GetField<ControlsInput>(null, "m_playerInputManager"));

        internal static RewiredDict m_playerInputManager;

        internal static Dictionary<string, KeybindInfo> s_customKeyDict = new Dictionary<string, KeybindInfo>();

        /// <summary>Use this to add a new Keybinding to the game.</summary>
        /// <param name="name">The name for the keybinding displayed in the menu.</param>
        /// <param name="category">The category to add to</param>
        /// <param name="controlType">What type of control this is</param>
        /// <param name="type">What type(s) of input it will accept</param>
        public static void AddAction(string name, KeybindingsCategory category, ControlType controlType = ControlType.Both, InputType type = InputType.Button)
        {
            bool initialized = (bool)At.GetPropertyStatic(typeof(ReInput), "initialized");
            if (initialized)
            {
                SL.LogWarning("Tried to add Custom Keybinding too late. Add your keybinding earlier, such as in your BaseUnityPlugin.Awake() method.");
                return;
            }

            if (s_customKeyDict.ContainsKey(name))
            {
                SL.LogWarning($"Attempting to add a keybind '{name}', but one with this name has already been registered.");
                return;
            }

            var customKey = new KeybindInfo(name, category, controlType, type);
            s_customKeyDict.Add(name, customKey);
        }

        /// <summary>Use this to check if a key is pressed this frame.</summary>
        /// <param name="keyName">The name of the key which you registered with.</param>
        /// <returns>True if pressed, false if not.</returns>
        public static bool GetKeyDown(string keyName) => GetKeyDown(keyName, out _);

        /// <summary>Use this to check if a key is pressed this frame, and get the local ID of the player who pressed it.</summary>
        /// <param name="keyName">The name of the key which you registered with.</param>
        /// <param name="playerID">If the key is pressed, this is the local split-player ID that pressed it.</param>
        /// <returns>True if pressed, false if not.</returns>
        public static bool GetKeyDown(string keyName, out int playerID)
        {
            if (!CheckKey(keyName))
            {
                playerID = -1;
                return false;
            }

            return s_customKeyDict[keyName].GetKeyDown(out playerID);
        }

        /// <summary>Use this to check if a key is held this frame.</summary>
        /// <param name="keyName">The name of the key which you registered with.</param>
        /// <returns>True if pressed, false if not.</returns>
        public static bool GetKey(string keyName) => GetKey(keyName, out _);

        /// <summary>Use this to check if a key is held this frame, and get the local ID of the player who pressed it.</summary>
        /// <param name="keyName">The name of the key which you registered with.</param>
        /// <param name="playerID">If the key is pressed, this is the local split-player ID that pressed it.</param>
        /// <returns>True if pressed, false if not.</returns>
        public static bool GetKey(string keyName, out int playerID)
        {
            if (!CheckKey(keyName))
            {
                playerID = -1;
                return false;
            }    

            return s_customKeyDict[keyName].GetKey(out playerID);
        }

        internal static readonly HashSet<string> s_loggedKeynames = new HashSet<string>();

        internal static bool CheckKey(string keyName)
        {
            if (!s_customKeyDict.ContainsKey(keyName))
            {
                if (!s_loggedKeynames.Contains(keyName))
                {
                    SL.LogWarning($"Attempting to get custom keybinding state, but no custom keybinding " +
                        $"with the name '{keyName}' was registered, this will not be logged again.");

                    s_loggedKeynames.Add(keyName);
                }
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(InputManager_Base), "Initialize")]
        public class InputManager_Base_Initialize
        {
            [HarmonyPrefix]
            public static void Prefix(InputManager_Base __instance)
            {
                foreach (var keybindInfo in s_customKeyDict.Values)
                {
                    // This actually creates the new actions:
                    AddRewiredAction(__instance.userData, keybindInfo.name, keybindInfo.category, keybindInfo.type, out int actionID);
                    keybindInfo.actionID = actionID;

                    SL.Log($"Set up custom keybinding '{keybindInfo.name}', actionID: " + keybindInfo.actionID);
                }
            }
        }

        internal static InputAction AddRewiredAction(UserData userData, string name, KeybindingsCategory category, InputType type, out int actionID)
        {
            // Add an action to the data store
            userData.AddAction((int)category);

            int[] actionIds = userData.GetActionIds();
            actionID = actionIds.Length - 1;

            // Get a reference to the added action
            var inputAction = userData.GetActionById(actionID);

            // Configure our action according to args
            At.SetField(inputAction, "_name", name);
            At.SetField(inputAction, "_descriptiveName", name);
            At.SetField(inputAction, "_type", (InputActionType)type);
            At.SetField(inputAction, "_userAssignable", true);
            At.SetField(inputAction, "_categoryId", (int)category);

            return inputAction;
        }

        // patch to set up custom keybinding section on the UI

        [HarmonyPatch(typeof(ControlMappingPanel), "InitMappings")]
        public class ControlMappingPanel_InitMappings
        {
            [HarmonyPrefix]
            public static void Prefix(ControlMappingPanel __instance, ControlMappingSection ___m_sectionTemplate,
                DictionaryExt<int, ControlMappingSection> ___m_sectionList, Controller ___m_lastJoystickController)
            {
                // check if any keybindings in "custom" category were defined
                if (!s_customKeyDict.Values.Where(it => it.category == CUSTOM_CATEGORY).Any())
                    return;

                // set up our custom localization
                InitLocalization();

                // set the template active while we clone from it
                ___m_sectionTemplate.gameObject.SetActive(true);

                // Create the UI (copied from how game does it)
                var mapSection = UnityEngine.Object.Instantiate(___m_sectionTemplate, ___m_sectionTemplate.transform.parent);
                mapSection.ControllerType = __instance.ControllerType;
                ___m_sectionList.Add(CUSTOM_CATEGORY, mapSection);

                mapSection.SetCategory(CUSTOM_CATEGORY);

                // Init the custom keys
                if (__instance.ControllerType == ControlMappingPanel.ControlType.Keyboard)
                {
                    var kbMap = ReInput.mapping.GetKeyboardMapInstance(CUSTOM_CATEGORY, 0);
                    InitSections(mapSection, kbMap);

                    var mouseMap = ReInput.mapping.GetMouseMapInstance(CUSTOM_CATEGORY, 0);
                    InitSections(mapSection, mouseMap);
                }
                else
                {
                    var joyMap = ReInput.mapping.GetJoystickMapInstance(___m_lastJoystickController as Joystick, CUSTOM_CATEGORY, 0);
                    InitSections(mapSection, joyMap);
                }

                //// Create a fake label (and space)
                //var lbl = (UnityEngine.UI.Text)At.GetField("m_lblSectionName", mapSection);
                //var sectNameObj = lbl.gameObject;
                //var parent = lbl.transform.parent.parent;

                //var emptyLbl = UnityEngine.Object.Instantiate(sectNameObj, parent);
                //emptyLbl.GetComponent<UnityEngine.UI.Text>().text = "";
                //var fakeLbl = UnityEngine.Object.Instantiate(sectNameObj, parent);
                //fakeLbl.GetComponent<UnityEngine.UI.Text>().text = "Actions";

                // set the template inactive again
                ___m_sectionTemplate.gameObject.SetActive(false);
            }

            // patch to set up actual custom keybinding rows for each UI instance

            internal static void InitSections(ControlMappingSection mapSection, ControllerMap _controllerMap)
            {
                if (_controllerMap == null)
                    return;

                if (_controllerMap.categoryId != 5)
                    return;

                // Loop through the custom actions we defined
                foreach (var entry in s_customKeyDict)
                {
                    var customKey = entry.Value;

                    // check if this controllerMap is actually for the keybind type (if not Both)
                    if (customKey.controllerType != ControlType.Both)
                    {
                        if (customKey.controllerType == ControlType.Keyboard 
                            && (_controllerMap.controllerType == Rewired.ControllerType.Keyboard 
                                || _controllerMap.controllerType == Rewired.ControllerType.Mouse))
                        {
                            if (!(_controllerMap is KeyboardMap) && !(_controllerMap is MouseMap))
                                continue;
                        }
                        else if (customKey.controllerType == ControlType.Gamepad && _controllerMap.controllerType == Rewired.ControllerType.Joystick)
                        {
                            if (_controllerMap is JoystickMap)
                                continue;
                        }
                    }

                    // add the actual keybind mapping to this controller in Rewired
                    _controllerMap.CreateElementMap(customKey.actionID, Pole.Positive, KeyCode.None, ModifierKeyFlags.None);

                    var alreadyKnown = At.GetField(mapSection, "m_actionAlreadyKnown") as List<int>;

                    // see if the UI has already been set up for this keybind
                    if (alreadyKnown.Contains(customKey.actionID))
                        continue;

                    // set up the UI for this keybind (same as how game does it)

                    alreadyKnown.Add(customKey.actionID);

                    var action = ReInput.mapping.GetAction(customKey.actionID);

                    var actTemplate = At.GetField(mapSection, "m_actionTemplate") as ControlMappingAction;

                    actTemplate.gameObject.SetActive(true);
                    if (action.type == InputActionType.Button)
                    {
                        At.Invoke(mapSection, "CreateActionRow", CUSTOM_CATEGORY, action, AxisRange.Positive);
                    }
                    else
                    {
                        if (mapSection.ControllerType == ControlMappingPanel.ControlType.Keyboard)
                        {
                            At.Invoke(mapSection, "CreateActionRow", CUSTOM_CATEGORY, action, AxisRange.Positive);
                            At.Invoke(mapSection, "CreateActionRow", CUSTOM_CATEGORY, action, AxisRange.Negative);
                        }
                        else
                        {
                            At.Invoke(mapSection, "CreateActionRow", CUSTOM_CATEGORY, action, AxisRange.Full);
                        }
                    }
                    actTemplate.gameObject.SetActive(false);

                    // Continue the loop of custom keys...
                }
            }
        }

        [HarmonyPatch(typeof(ControlMappingPanel), "InitSections")]
        public class ControlMappingPanel_InitSections
        {
            [HarmonyPrefix]
            public static bool InitSections(ControllerMap _controllerMap)
            {
                if (_controllerMap.categoryId == CUSTOM_CATEGORY)
                    return false;

                foreach (var keybindInfo in s_customKeyDict.Values)
                {
                    // If the controller map's control type does not match our action
                    if (!(keybindInfo.controllerType == ControlType.Both
                        || keybindInfo.controllerType == ControlType.Keyboard && (_controllerMap is KeyboardMap || _controllerMap is MouseMap)
                        || keybindInfo.controllerType == ControlType.Gamepad && (_controllerMap is JoystickMap)))
                    {
                        // Then skip to next action
                        continue;
                    }

                    if (_controllerMap.categoryId != 5)
                    {
                        // Skip to next action
                        continue;
                    }

                    // If we pass the tests, create & add the action-to-element map for this particular action
                    _controllerMap.CreateElementMap(keybindInfo.actionID, Pole.Positive, KeyCode.None, ModifierKeyFlags.None);

                    // Continue the loop...
                }

                // We're done here. Call original implementation
                return true;
            }
        }

        internal static bool s_doneInit;

        public static void InitLocalization()
        {
            if (s_doneInit)
                return;

            s_doneInit = true;

            var genLoc = References.GENERAL_LOCALIZATION;

            // get the Rewired Category for the category we're using
            var category = ReInput.mapping.GetActionCategory(CUSTOM_CATEGORY);

            // override the internal name
            At.SetField(category, "_name", "CustomKeybindings");

            // add localization for this name, in the format the game will expect to find it
            genLoc.Add("ActionCategory_CustomKeybindings", "Custom Keybindings");

            // Go through the added actions and use the user-created action descriptions to name them
            foreach (var customKey in s_customKeyDict.Values)
            {
                string key = "InputAction_" + customKey.name;
                genLoc.Add(key, customKey.name);
            }
        }
    }
}
