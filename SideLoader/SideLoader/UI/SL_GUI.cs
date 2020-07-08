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
            // ItemVisualsHelper
        }

        private Pages m_page = 0;

        public static bool ShowMenu = false;

        private int SelectedItemID = 0;
        private int NewItemID = 0;
        private EffectBehaviours m_templateBehaviour = EffectBehaviours.DestroyEffects;

        private int SelectedStatusID = 0;
        private int NewStatusID = 0;

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
            //SetPageButton("Item Visual Helper", 3);
            GUILayout.EndHorizontal();

            switch (m_page)
            {
                case Pages.Items: ItemPage(); break;
                case Pages.StatusEffects: EffectsPage(); break;
                case Pages.Enchantments: EnchantmentsPage(); break;
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
            GUILayout.Label("Enter a Status Effect Preset ID to generate a template from.");
            GUILayout.Label("Templates are generated to the folder Mods/SideLoader/_GENERATED/StatusEffects/.");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Status ID:");
            var selectedID = GUILayout.TextField(SelectedStatusID.ToString(), GUILayout.Width(150));
            if (int.TryParse(selectedID, out int id))
            {
                SelectedStatusID = id;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("New Status ID:");
            var newID = GUILayout.TextField(NewStatusID.ToString(), GUILayout.Width(150));
            if (int.TryParse(newID, out int id2))
            {
                NewStatusID = id2;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Generate template"))
            {
                GenerateStatusTemplate();
            }
        }

        private void GenerateStatusTemplate()
        {
            if (ResourcesPrefabManager.Instance.GetEffectPreset(SelectedStatusID) is EffectPreset preset)
            {
                SL.Log("Generating template for " + preset.gameObject.name + "...");

                var folder = SL.GENERATED_FOLDER + @"\StatusEffects\" + preset.gameObject.name;

                if (preset is ImbueEffectPreset)
                {
                    var comp = preset as ImbueEffectPreset;
                    var template = SL_ImbueEffect.ParseImbueEffect(comp);
                    template.NewStatusID = NewStatusID;
                    Serializer.SaveToXml(folder, preset.gameObject.name, template);
                    if (comp.ImbueStatusIcon)
                    {
                        CustomTextures.SaveIconAsPNG(comp.ImbueStatusIcon, folder);
                    }
                }
                else
                {
                    var tempObj = Instantiate(preset.gameObject);

                    var comp = tempObj.GetComponent<StatusEffect>();
                    var template = SL_StatusEffect.ParseStatusEffect(comp);
                    template.NewStatusID = NewStatusID;
                    Serializer.SaveToXml(folder, preset.gameObject.name, template);
                    if (comp.StatusIcon)
                    {
                        CustomTextures.SaveIconAsPNG(comp.StatusIcon, folder);
                    }
                    Destroy(tempObj);
                }
            }
            else
            {
                SL.Log("DEBUG MENU: PresetID " + SelectedStatusID + " is invalid!");
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

        #region ITEM VISUALS HELPER (Page 4)
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