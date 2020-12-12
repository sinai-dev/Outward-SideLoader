using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Diagnostics;
using SideLoader.Helpers;
using UnityEngine.SceneManagement;

namespace SideLoader.GUI
{
    public class SLGUI : MonoBehaviour
    {
        public static SLGUI Instance;

        internal const string MENU_TOGGLE_KEY = "SideLoader Menu";

        public static bool ShowMenu = false;

        internal static GameObject s_SLGUICanvas;

        // UI References
        internal GameObject s_generatorsPage;
        internal GameObject s_toolsPage;
        internal GameObject s_slPacksPage;

        internal InputField s_playerPosInput;
        internal InputField s_sceneInput;

        internal void Awake()
        {
            Instance = this;

            CustomKeybindings.AddAction(MENU_TOGGLE_KEY, KeybindingsCategory.CustomKeybindings);

            var filePath = $@"{SL.SL_FOLDER}\_INTERNAL\slgui.bundle";
            if (!File.Exists(filePath))
            {
                SL.LogWarning("Could not find the SL GUI bundle, it should exist at '" + filePath + "'. Try re-installing SideLoader.");
                return;
            }

            var bundle = AssetBundle.LoadFromFile(filePath);

            s_SLGUICanvas = GameObject.Instantiate(bundle.LoadAsset<GameObject>("SL_Canvas"));
            GameObject.DontDestroyOnLoad(s_SLGUICanvas);

            s_SLGUICanvas.SetActive(false);

            SetupUI();
        }

        internal void Update()
        {
            if (CustomKeybindings.GetKeyDown(MENU_TOGGLE_KEY))
            {
                ToggleMenu();
            }

            if (ShowMenu && s_toolsPage && s_toolsPage.activeSelf)
            {
                var player = CharacterManager.Instance.GetFirstLocalCharacter();
                if (player)
                {
                    var pos = player.transform.position;
                    s_playerPosInput.text = $"X: {pos.x:#.##}, Y: {pos.y:#.##}, Z: {pos.z:#.##}";
                }
                else
                {
                    if (!string.IsNullOrEmpty(s_playerPosInput.text))
                        s_playerPosInput.text = "";
                }
            }
        }

        internal void ToggleMenu()
        {
            ShowMenu = !ShowMenu;

            s_SLGUICanvas.SetActive(ShowMenu);

            if (ShowMenu)
                ForceUnlockCursor.AddUnlockSource();
            else
                ForceUnlockCursor.RemoveUnlockSource();
        }

        internal void SetupUI()
        {
            var canvas = s_SLGUICanvas.GetComponent<Canvas>();
            canvas.sortingOrder = 999;

            var pageHolder = canvas.transform.Find("MainBackground/PageMaskContainer/Scroll View/Viewport/Content");

            var closeBtn = canvas.transform.Find("MainBackground/TitleBar/CloseBtn").GetComponent<Button>();
            closeBtn.onClick.AddListener(ToggleMenu);

            // =========== GENERATOR PAGE ===========

            s_generatorsPage = pageHolder.GetChild(0).gameObject;

            // Open _GENERATED folder button
            var openFolderBtn = s_generatorsPage.transform.Find("OpenFolderBtn").GetComponent<Button>();
            openFolderBtn.onClick.AddListener(() => 
            {
                var path = Path.GetFullPath(SL.SL_FOLDER + @"\_GENERATED");
                Process.Start(path);
            });

            // Item generator
            var itemSection = s_generatorsPage.transform.Find("ItemGenerator");
            var itemInput = itemSection.Find("ItemIDInput").GetComponent<InputField>();
            var itemBtn = itemSection.Find("GenerateBtn").GetComponent<Button>();
            itemBtn.onClick.AddListener(() => 
            {
                if (int.TryParse(itemInput.text, out int itemID))
                    GenerateItemTemplate(itemID);
            });

            // Status generator
            var statusSection = s_generatorsPage.transform.Find("StatusGenerator");
            var statusInput = statusSection.Find("StatusInput").GetComponent<InputField>();
            var statusBtn = statusSection.Find("GenerateBtn").GetComponent<Button>();
            statusBtn.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(statusInput.text))
                    GenerateStatusTemplate(statusInput.text);
            });

            // Imbue generator
            var imbueSection = s_generatorsPage.transform.Find("ImbuePresetGenerator");
            var imbueInput = imbueSection.Find("ImbueInput").GetComponent<InputField>();
            var imbueBtn = imbueSection.Find("GenerateBtn").GetComponent<Button>();
            imbueBtn.onClick.AddListener(() =>
            {
                if (int.TryParse(imbueInput.text, out int presetID))
                    GenerateImbueTemplate(presetID);
            });

            // Enchantment generator
            var enchSection = s_generatorsPage.transform.Find("EnchantmentGenerator");
            var enchInput = enchSection.Find("EnchantInput").GetComponent<InputField>();
            var enchBtn = enchSection.Find("GenerateBtn").GetComponent<Button>();
            enchBtn.onClick.AddListener(() =>
            {
                if (int.TryParse(enchInput.text, out int enchID))
                    GenerateEnchantTemplate(enchID);
            });

            // =========== TOOLS PAGE ===========

            s_toolsPage = pageHolder.GetChild(1).gameObject;

            s_playerPosInput = s_toolsPage.transform.Find("PositionDebug/PositionInput").GetComponent<InputField>();

            s_sceneInput = s_toolsPage.transform.Find("Scene Debug/SceneInput").GetComponent<InputField>();
            SceneManager.activeSceneChanged += OnSceneChange;
            s_sceneInput.text = SceneManager.GetActiveScene().name;

            // =========== SL_PACKS PAGE ===========

            s_slPacksPage = pageHolder.GetChild(2).gameObject;

            var hotReloadBtn = s_slPacksPage.transform.Find("HotReloadBtn").GetComponent<Button>();
            hotReloadBtn.onClick.AddListener(() =>
            {
                SL.Setup(false);
            });

            // wip
        }

        private void OnSceneChange(Scene prev, Scene next)
        {
            if (s_sceneInput)
                s_sceneInput.text = next.name;
        }

        // =========== GENERATOR METHODS ===========

        private void GenerateItemTemplate(int itemID)
        {
            if (CustomItems.GetOriginalItemPrefab(itemID) is Item item)
            {
                SL.Log("Generating template for item " + item.Name);

                var template = SL_Item.ParseItemToTemplate(item);

                var itemfolder = SL.GENERATED_FOLDER + @"\Items\" + item.gameObject.name;
                Serializer.SaveToXml(itemfolder, item.Name, template);

                CustomItemVisuals.SaveAllItemTextures(item, itemfolder + @"\Textures");
            }
            else
            {
                SL.Log($"Could not find any item with the ID '{itemID}'!");
            }
        }

        private void GenerateStatusTemplate(string identifier)
        {
            var prefab = CustomStatusEffects.GetOrigStatusEffect(identifier)?.gameObject;

            if (!prefab)
            {
                SL.LogWarning($"Could not find any Status Effect with the identifier '{identifier}'");
                return;
            }

            SL.Log("Generating template for " + prefab.name + "...");

            var tempObj = Instantiate(prefab.gameObject);

            var comp = tempObj.GetComponent<StatusEffect>();
            
            var template = SL_StatusEffect.ParseStatusEffect(comp);

            var folder = SL.GENERATED_FOLDER + @"\StatusEffects\" + prefab.name;
            Serializer.SaveToXml(folder, prefab.name, template);
            
            if (comp.StatusIcon)
                CustomTextures.SaveIconAsPNG(comp.StatusIcon, folder);
            
            Destroy(tempObj);
        }

        private void GenerateImbueTemplate(int presetID)
        {
            var prefab = CustomStatusEffects.GetOrigEffectPreset(presetID).gameObject;

            if (!prefab)
            {
                SL.LogWarning($"Could not find any Effect Preset with the ID '{presetID}'");
                return;
            }

            if (prefab.GetComponent<ImbueEffectPreset>() is ImbueEffectPreset imbueEffect)
            {
                SL.Log("Generating template for effect " + imbueEffect.Name);

                var folder = SL.GENERATED_FOLDER + @"\StatusEffects\" + prefab.name;

                var template = SL_ImbueEffect.ParseImbueEffect(imbueEffect);
                Serializer.SaveToXml(folder, imbueEffect.name, template);

                if (imbueEffect.ImbueStatusIcon)
                    CustomTextures.SaveIconAsPNG(imbueEffect.ImbueStatusIcon, folder);
            }
        }

        private void GenerateEnchantTemplate(int enchantID)
        {
            if (ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantID) is Enchantment enchantment
                && RecipeManager.Instance.GetEnchantmentRecipeForID(enchantID) is EnchantmentRecipe recipe)
            {
                SL.Log("Generating template from Enchantment: " + enchantment.Name);

                var folder = SL.GENERATED_FOLDER + @"\Enchantments";

                var template = SL_EnchantmentRecipe.SerializeEnchantment(recipe, enchantment);
                template.EnchantmentID = enchantID;

                Serializer.SaveToXml(folder, recipe.name, template);
            }
            else
            {
                SL.Log($"Error: Could not find any Enchantment with the ID {enchantID}");
            }
        }




        //#region ITEM VISUALS HELPER (Page ?)
        ////private bool m_aligning = false;

        ////// desired item visuals and hot transform
        ////private int m_currentVisualsID = 5500999;
        ////private Transform m_currentVisuals;

        ////// translate amounts
        ////private float m_posAmount = 1f;
        ////private float m_rotAmount = 30f;

        ////// user input values
        ////private Vector3 m_currentPos;
        ////private Quaternion m_currentRot;

        ////private Vector3 m_cachedPos;
        ////private Vector3 m_cachedRot;

        ////private void ItemVisualsPage()
        ////{
        ////    if (m_aligning)
        ////    {
        ////        var pos = m_currentVisuals.transform.localPosition;
        ////        var rot = m_currentVisuals.transform.localRotation.eulerAngles;

        ////        m_currentVisuals.transform.localPosition = UIStyles.Translate("Pos", pos, ref m_posAmount, true);
        ////        m_currentVisuals.transform.localRotation = Quaternion.Euler(UIStyles.Translate("Rot", rot, ref m_rotAmount, true));

        ////        //var pos = m_currentPos;
        ////        //var rot = m_currentRot.eulerAngles;

        ////        //m_currentPos = UIStyles.Translate("Pos", m_currentPos, ref m_posAmount, true);
        ////        //m_currentRot = Quaternion.Euler(UIStyles.Translate("Rot", m_currentRot.eulerAngles, ref m_rotAmount, true));

        ////        //var posChange = m_currentPos - pos;
        ////        //var rotChange = m_currentRot.eulerAngles - rot;

        ////        //if (posChange != Vector3.zero)
        ////        //{
        ////        //    m_currentVisuals.localPosition += posChange;
        ////        //}
        ////        //if (rotChange != Vector3.zero)
        ////        //{
        ////        //    var localRot = m_currentVisuals.localRotation.eulerAngles;
        ////        //    localRot += rotChange;
        ////        //    m_currentVisuals.localRotation = Quaternion.Euler(localRot);
        ////        //}
        ////    }
        ////    else
        ////    {
        ////        // set ID
        ////        GUILayout.BeginHorizontal(null);
        ////        GUILayout.Label("Item ID:", new GUILayoutOption[] { GUILayout.Width(60) });
        ////        var idString = m_currentVisualsID.ToString();
        ////        idString = GUILayout.TextField(idString, new GUILayoutOption[] { GUILayout.Width(100) });
        ////        if (int.TryParse(idString, out int id))
        ////        {
        ////            m_currentVisualsID = id;
        ////        }
        ////        GUILayout.EndHorizontal();

        ////        // enter current position/rotation
        ////        GUILayout.Label("Enter current pos/rot offsets here:", null);
        ////        m_currentPos = UIStyles.Translate("Pos", m_currentPos, ref m_posAmount, true);
        ////        var rotEuler = m_currentRot.eulerAngles;
        ////        rotEuler = UIStyles.Translate("Rot", rotEuler, ref m_rotAmount, true);
        ////        m_currentRot = Quaternion.Euler(rotEuler);
        ////        GUILayout.Label("After aligning, replace your template values with these values.", null);
        ////    }

        ////    GUI.color = m_aligning ? Color.green : Color.red;
        ////    if (GUILayout.Button((m_aligning ? "Stop" : "Start") + " Aligning", null))
        ////    {
        ////        if (!CharacterManager.Instance.GetFirstLocalCharacter())
        ////        {
        ////            SL.SL_Log("You need to load up a character first!");
        ////        }
        ////        else
        ////        {
        ////            StartStopAligning(!m_aligning);
        ////        }
        ////    }
        ////    GUI.color = Color.white;
        ////}

        ////private void StartStopAligning(bool start)
        ////{
        ////    if (start)
        ////    {
        ////        var character = CharacterManager.Instance.GetFirstLocalCharacter();

        ////        if (ResourcesPrefabManager.Instance.GetItemPrefab(m_currentVisualsID) is Equipment equipment)
        ////        {
        ////            character.transform.rotation = Quaternion.identity;

        ////            SL_Character.TryEquipItem(character, m_currentVisualsID);

        ////            if (character.Inventory.Equipment.GetEquippedItem(equipment.EquipSlot) is Equipment equippedItem)
        ////            {
        ////                var visualTrans = ((ItemVisual)At.GetValue(typeof(Item), equippedItem, "m_loadedVisual")).transform;

        ////                foreach (Transform child in visualTrans)
        ////                {
        ////                    if (!child.gameObject.activeSelf)
        ////                    {
        ////                        continue;
        ////                    }

        ////                    if (child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
        ////                    {
        ////                        SL.Log("Found visuals, ready to align!");

        ////                        m_currentVisuals = child;
        ////                        m_aligning = true;

        ////                        m_cachedPos = m_currentVisuals.transform.localPosition;
        ////                        m_cachedRot = m_currentVisuals.transform.localRotation.eulerAngles;

        ////                        break;
        ////                    }
        ////                }
        ////            }
        ////        }

        ////        if (!m_aligning)
        ////        {
        ////            SL.Log("Couldn't start aligning!");
        ////        }
        ////    }
        ////    else
        ////    {
        ////        var pos = m_currentVisuals.transform.localPosition;
        ////        var rot = m_currentVisuals.transform.localRotation.eulerAngles;

        ////        var posChange = pos - m_cachedPos;
        ////        var rotChange = rot - m_cachedRot;

        ////        m_currentPos += posChange;
        ////        m_currentRot = Quaternion.Euler(m_currentRot.eulerAngles + rotChange);

        ////        m_currentVisuals = null;
        ////        m_cachedPos = Vector3.zero;
        ////        m_cachedRot = Vector3.zero;

        ////        m_aligning = false;
        ////    }
        ////}
        //#endregion
    }
}