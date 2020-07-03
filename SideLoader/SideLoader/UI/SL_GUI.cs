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

        private int m_page = 0;

        public static bool ShowDebug = false;

        private static bool m_debugFileExists = false;

        private int SelectedItemID = 0;
        private int NewItemID = 0;
        private EffectBehaviours m_templateBehaviour = EffectBehaviours.DestroyEffects;

        private int SelectedStatusID = 0;
        private int NewStatusID = 0;

        //// temp debug
        //private string m_enemyName = "";

        internal void Update()
        {
            if (m_debugFileExists && Input.GetKeyDown(KeyCode.F6))
            {
                ShowDebug = !ShowDebug;
            }
        }

        internal void Awake()
        {
            Instance = this;

            if (File.Exists(SL.SL_FOLDER + @"\debug.txt"))
            {
                m_debugFileExists = true;
                ShowDebug = true;
            }
        }

        internal void OnGUI()
        {
            if (ShowDebug)
            {
                var orig = GUI.skin;
                GUI.skin = UI.UIStyles.WindowSkin;
                m_rect = GUI.Window(29, m_rect, WindowFunction, "SideLoader Menu (F6 Toggle)");
                GUI.skin = orig;
            }
        }

        private void WindowFunction(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, m_rect.width, 20));

            GUILayout.BeginArea(new Rect(5, 20, m_rect.width - 10, m_rect.height - 15));

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            SetPageButton("Items", 0);
            SetPageButton("StatusEffect", 1);
            //SetPageButton("Item Visual Helper", 2);
            GUILayout.EndHorizontal();

            switch (m_page)
            {
                case 0: ItemPage(); break;
                case 1: EffectsPage(); break;
                //case 2: ItemVisualsPage(); break;
            }

            GUILayout.EndArea();
        }

        private void SetPageButton(string label, int id)
        {
            if (m_page == id)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.red;
            }

            if (GUILayout.Button(label))
            {
                m_page = id;

                // item visuals page uses a custom size
                if (id == 2)
                {
                    m_rect.width = 575;
                    m_rect.height = 250;
                }
                else
                {
                    m_rect.width = 350;
                    m_rect.height = 450;
                }
            }

            GUI.color = Color.white;
        }

        #region ITEM GENERATOR
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

        #region STATUS GENERATOR
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

        #region ITEM VISUALS HELPER
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