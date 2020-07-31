using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;

namespace SideLoader.UI
{
    public class SL_GUI : MonoBehaviour
    {
        public static SL_GUI Instance;

        private static Rect m_rect = new Rect(5, 5, 350, 450);

        public enum Pages
        {
            Items,
            StatusEffects,
            Enchantments,
            HotReload,
            // ItemVisualsHelper
        }

        private Pages m_page = 0;

        public static bool ShowMenu = false;

        // Items
        private int SelectedItemID = 0;
        private int NewItemID = 0;
        private EffectBehaviours m_templateBehaviour = EffectBehaviours.DestroyEffects;

        // Status/Imbues
        private string TargetStatusIdentifier = "";
        private string NewStatusIdentifier = "";
        private int TargetStatusID = -1;
        private int NewStatusID = -1;

        // Enchants
        private int SelectedEnchantmentID;
        private int NewEnchantmentID;

        //// temp debug
        //private string m_enemyName = "";

        internal void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) 
                && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    ShowMenu = !ShowMenu;
                }
            }
        }

        internal void Awake()
        {
            Instance = this;
        }

        internal void OnGUI()
        {
            if (ShowMenu)
            {
                var orig = GUI.skin;
                GUI.skin = UI.UIStyles.WindowSkin;
                m_rect = GUI.Window(29, m_rect, WindowFunction, "SideLoader Menu (Ctrl+Alt+F6 Toggle)");
                GUI.skin = orig;
            }
        }

        private void WindowFunction(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, m_rect.width, 20));

            GUILayout.BeginArea(new Rect(5, 20, m_rect.width - 10, m_rect.height - 15));

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            SetPageButton("Items", Pages.Items);
            SetPageButton("StatusEffects", Pages.StatusEffects);
            SetPageButton("Enchantments", Pages.Enchantments);
            SetPageButton("Hot Reload", Pages.HotReload);
            //SetPageButton("Item Visual Helper", 3);
            GUILayout.EndHorizontal();

            switch (m_page)
            {
                case Pages.Items: ItemPage(); break;
                case Pages.StatusEffects: EffectsPage(); break;
                case Pages.Enchantments: EnchantmentsPage(); break;
                case Pages.HotReload: HotReloadPage(); break;
                //case 3: ItemVisualsPage(); break;
            }

            GUILayout.EndArea();
        }

        private void SetPageButton(string label, Pages page)
        {
            if (m_page == page)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.red;
            }

            if (GUILayout.Button(label))
            {
                m_page = page;

                //// item visuals page uses a custom size
                //if (id == 2)
                //{
                //    m_rect.width = 575;
                //    m_rect.height = 250;
                //}
                //else
                //{
                //    m_rect.width = 350;
                //    m_rect.height = 450;
                //}
            }

            GUI.color = Color.white;
        }

        #region ITEM GENERATOR (Page 1)
        private void ItemPage()
        {
            GUILayout.Label("Enter an Item ID to generate a template from. This will also save all material textures (if any).");
            GUILayout.Label("Templates are generated to the folder Mods/SideLoader/_GENERATED/Items/.");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item ID:");
            string input = GUILayout.TextField(SelectedItemID.ToString(), GUILayout.Width(150));
            if (int.TryParse(input, out int id))
            {
                SelectedItemID = id;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("New ID:");
            string input2 = GUILayout.TextField(NewItemID.ToString(), GUILayout.Width(150));
            if (int.TryParse(input2, out int id2))
            {
                NewItemID = id2;
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Effects Behaviour:");
            BehaviourButton(EffectBehaviours.DestroyEffects, "Destroy Effects");
            BehaviourButton(EffectBehaviours.OverrideEffects, "Override Effects");
            BehaviourButton(EffectBehaviours.NONE, "None (leave all)");

            GUILayout.Space(15);

            if (GUILayout.Button("Generate template"))
            {
                GenerateItemTemplate();
            }
        }

        private void BehaviourButton(EffectBehaviours _behaviour, string _label)
        {
            string label = "<color=";
            if (m_templateBehaviour == _behaviour)
            {
                label += "lime>";
            }
            else
            {
                label += "orange>";
            }
            label += _label + "</color>";

            if (GUILayout.Button(label))
            {
                m_templateBehaviour = _behaviour;
            }
        }

        private void GenerateItemTemplate()
        {
            if (ResourcesPrefabManager.Instance.GetItemPrefab(SelectedItemID) is Item item)
            {
                var template = SL_Item.ParseItemToTemplate(item);

                template.New_ItemID = NewItemID;
                template.EffectBehaviour = m_templateBehaviour;

                var itemfolder = SL.GENERATED_FOLDER + @"\Items\" + item.gameObject.name;
                Serializer.SaveToXml(itemfolder, item.Name, template);

                CustomItemVisuals.SaveAllItemTextures(item, itemfolder + @"\Textures");
            }
            else
            {
                SL.Log("DEBUG MENU: ID " + SelectedItemID + " is invalid!");
            }
        }
        #endregion

        #region STATUS GENERATOR (Page 2)
        private void EffectsPage()
        {
            GUILayout.Label("Templates are generated to the folder Mods/SideLoader/_GENERATED/StatusEffects/.");

            GUILayout.Label("<b>SL_StatusEffects:</b>");
            GUILayout.Label("Enter a Status Effect Identifier Name to generate a template from.");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Status Identifier Name:");
            TargetStatusIdentifier = GUILayout.TextField(TargetStatusIdentifier.ToString(), GUILayout.Width(150));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("New Status Identifier:");
            NewStatusIdentifier = GUILayout.TextField(NewStatusIdentifier, GUILayout.Width(150));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Generate from Identifier Name"))
            {
                GenerateStatusTemplate(false);
            }

            GUILayout.Space(15);

            GUILayout.Label("<b>SL_ImbueEffects:</b>");
            GUILayout.Label("Enter a Imbue Effect Preset ID (number) to generate a template from.");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Preset ID:");
            var targetIDString = GUILayout.TextField(TargetStatusID.ToString(), GUILayout.Width(150));
            if (int.TryParse(targetIDString, out int targetID))
            {
                TargetStatusID = targetID;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("New Preset ID:");
            var newIDString = GUILayout.TextField(NewStatusID.ToString(), GUILayout.Width(150));
            if (int.TryParse(newIDString, out int newID))
            {
                NewStatusID = newID;
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Generate from Preset ID"))
            {
                GenerateStatusTemplate(true);
            }
        }

        private void GenerateStatusTemplate(bool fromPresetID)
        {
            GameObject prefab;
            if (fromPresetID)
            {
                prefab = ResourcesPrefabManager.Instance.GetEffectPreset(TargetStatusID)?.gameObject;
            }
            else
            {
                prefab = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(TargetStatusIdentifier)?.gameObject;
            }

            if (prefab)
            {
                SL.Log("Generating template for " + prefab.name + "...");

                var folder = SL.GENERATED_FOLDER + @"\StatusEffects\" + prefab.name;

                if (prefab.GetComponent<ImbueEffectPreset>() is ImbueEffectPreset imbueEffect)
                {
                    var template = SL_ImbueEffect.ParseImbueEffect(imbueEffect);
                    template.NewStatusID = NewStatusID;
                    Serializer.SaveToXml(folder, imbueEffect.name, template);
                    if (imbueEffect.ImbueStatusIcon)
                    {
                        CustomTextures.SaveIconAsPNG(imbueEffect.ImbueStatusIcon, folder);
                    }
                }
                else
                {
                    var tempObj = Instantiate(prefab.gameObject);

                    var comp = tempObj.GetComponent<StatusEffect>();
                    var template = SL_StatusEffect.ParseStatusEffect(comp);
                    //template.NewStatusID = NewStatusID;
                    template.StatusIdentifier = NewStatusIdentifier;
                    Serializer.SaveToXml(folder, prefab.name, template);
                    if (comp.StatusIcon)
                    {
                        CustomTextures.SaveIconAsPNG(comp.StatusIcon, folder);
                    }
                    Destroy(tempObj);
                }
            }
            else
            {
                SL.Log("Could not find a Status with this target!");
            }
        }
        #endregion

        #region ENCHANTMENTS GENERATOR (Page 3)

        private void EnchantmentsPage()
        {
            GUILayout.Label("Enter an Enchantment ID to generate a template from.");
            GUILayout.Label("Templates are generated to the folder Mods/SideLoader/_GENERATED/Enchantments/.");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Enchantment ID:");
            var selectedID = GUILayout.TextField(SelectedEnchantmentID.ToString(), GUILayout.Width(150));
            if (int.TryParse(selectedID, out int id))
            {
                SelectedEnchantmentID = id;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("New Enchantment ID:");
            var newID = GUILayout.TextField(NewEnchantmentID.ToString(), GUILayout.Width(150));
            if (int.TryParse(newID, out int id2))
            {
                NewEnchantmentID = id2;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Generate template"))
            {
                GenerateEnchantTemplate();
            }
        }

        private void GenerateEnchantTemplate()
        {
            if (ResourcesPrefabManager.Instance.GetEnchantmentPrefab(SelectedEnchantmentID) is Enchantment enchantment
                && RecipeManager.Instance.GetEnchantmentRecipeForID(SelectedEnchantmentID) is EnchantmentRecipe recipe)
            {
                SL.Log("Generating template from Enchantment: " + enchantment.Name);

                var folder = SL.GENERATED_FOLDER + @"\Enchantments";

                var template = SL_EnchantmentRecipe.SerializeEnchantment(recipe, enchantment);
                template.EnchantmentID = NewEnchantmentID;

                Serializer.SaveToXml(folder, recipe.name, template);
            }
            else
            {
                SL.Log($"Error: Could not find any Enchantment with the ID {SelectedEnchantmentID}", 0);
            }
        }

        #endregion

        #region HOT RELOAD (Page 4)
        private void HotReloadPage()
        {
            GUILayout.Label("You can use this to reload and re-apply all SL Pack folders. " +
                "In some situations this might be buggy, but most things should be fine." +
                "\n\n" +
                "Note: modified Items will need to be re-spawned, and Skills need to be re-learned. Going to menu or reloading the scene will reset everything.");

            if (GUILayout.Button("Hot Reload"))
            {
                SL.Setup(false);
            }
        }
        #endregion

        #region ITEM VISUALS HELPER (Page ?)
        private bool m_aligning = false;

        // desired item visuals and hot transform
        private int m_currentVisualsID = 5500999;
        private Transform m_currentVisuals;

        // translate amounts
        private float m_posAmount = 1f;
        private float m_rotAmount = 30f;

        // user input values
        private Vector3 m_currentPos;
        private Quaternion m_currentRot;

        private Vector3 m_cachedPos;
        private Vector3 m_cachedRot;

        private void ItemVisualsPage()
        {
            if (m_aligning)
            {
                var pos = m_currentVisuals.transform.localPosition;
                var rot = m_currentVisuals.transform.localRotation.eulerAngles;

                m_currentVisuals.transform.localPosition = UIStyles.Translate("Pos", pos, ref m_posAmount, true);
                m_currentVisuals.transform.localRotation = Quaternion.Euler(UIStyles.Translate("Rot", rot, ref m_rotAmount, true));

                //var pos = m_currentPos;
                //var rot = m_currentRot.eulerAngles;

                //m_currentPos = UIStyles.Translate("Pos", m_currentPos, ref m_posAmount, true);
                //m_currentRot = Quaternion.Euler(UIStyles.Translate("Rot", m_currentRot.eulerAngles, ref m_rotAmount, true));

                //var posChange = m_currentPos - pos;
                //var rotChange = m_currentRot.eulerAngles - rot;

                //if (posChange != Vector3.zero)
                //{
                //    m_currentVisuals.localPosition += posChange;
                //}
                //if (rotChange != Vector3.zero)
                //{
                //    var localRot = m_currentVisuals.localRotation.eulerAngles;
                //    localRot += rotChange;
                //    m_currentVisuals.localRotation = Quaternion.Euler(localRot);
                //}
            }
            else
            {
                // set ID
                GUILayout.BeginHorizontal();
                GUILayout.Label("Item ID:", GUILayout.Width(60));
                var idString = m_currentVisualsID.ToString();
                idString = GUILayout.TextField(idString, GUILayout.Width(100));
                if (int.TryParse(idString, out int id))
                {
                    m_currentVisualsID = id;
                }
                GUILayout.EndHorizontal();

                // enter current position/rotation
                GUILayout.Label("Enter current pos/rot offsets here:");
                m_currentPos = UIStyles.Translate("Pos", m_currentPos, ref m_posAmount, true);
                var rotEuler = m_currentRot.eulerAngles;
                rotEuler = UIStyles.Translate("Rot", rotEuler, ref m_rotAmount, true);
                m_currentRot = Quaternion.Euler(rotEuler);
                GUILayout.Label("After aligning, replace your template values with these values.");
            }

            GUI.color = m_aligning ? Color.green : Color.red;
            if (GUILayout.Button((m_aligning ? "Stop" : "Start") + " Aligning"))
            {
                if (!CharacterManager.Instance.GetFirstLocalCharacter())
                {
                    SL.Log("You need to load up a character first!");
                }
                else
                {
                    StartStopAligning(!m_aligning);
                }
            }
            GUI.color = Color.white;
        }

        private void StartStopAligning(bool start)
        {
            if (start)
            {
                var character = CharacterManager.Instance.GetFirstLocalCharacter();

                if (ResourcesPrefabManager.Instance.GetItemPrefab(m_currentVisualsID) is Equipment equipment)
                {
                    character.transform.rotation = Quaternion.identity;

                    SL_Character.TryEquipItem(character, m_currentVisualsID);

                    if (character.Inventory.Equipment.GetEquippedItem(equipment.EquipSlot) is Equipment equippedItem)
                    {
                        var visualTrans = ((ItemVisual)At.GetValue(typeof(Item), equippedItem, "m_loadedVisual")).transform;

                        foreach (Transform child in visualTrans)
                        {
                            if (!child.gameObject.activeSelf)
                            {
                                continue;
                            }

                            if (child.GetComponent<BoxCollider>() && child.GetComponent<MeshRenderer>())
                            {
                                Debug.Log("Found visuals, ready to align!");

                                m_currentVisuals = child;
                                m_aligning = true;

                                m_cachedPos = m_currentVisuals.transform.localPosition;
                                m_cachedRot = m_currentVisuals.transform.localRotation.eulerAngles;

                                break;
                            }
                        }
                    }
                }
                
                if (!m_aligning)
                {
                    SL.Log("Couldn't start aligning!");
                }                
            }
            else
            {
                var pos = m_currentVisuals.transform.localPosition;
                var rot = m_currentVisuals.transform.localRotation.eulerAngles;

                var posChange = pos - m_cachedPos;
                var rotChange = rot - m_cachedRot;

                m_currentPos += posChange;
                m_currentRot = Quaternion.Euler(m_currentRot.eulerAngles + rotChange);

                m_currentVisuals = null;
                m_cachedPos = Vector3.zero;
                m_cachedRot = Vector3.zero;

                m_aligning = false;
            }
        }
        #endregion

        #region HELPERS
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        #endregion
    }
}