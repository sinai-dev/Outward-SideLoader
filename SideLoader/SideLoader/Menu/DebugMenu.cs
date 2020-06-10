using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;

namespace SideLoader
{
    public class DebugMenu : MonoBehaviour
    {
        public static DebugMenu Instance;

        private static Rect m_rect = new Rect(5, 5, 250, 410);

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
                m_rect = GUI.Window(29, m_rect, WindowFunction, "SideLoader Debug (F6 Toggle)");
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
            GUILayout.EndHorizontal();

            if (m_page == 0)
            {
                ItemPage();
            }
            else if (m_page == 1)
            {
                EffectsPage();
            }

            //GUILayout.BeginHorizontal();
            //GUILayout.Label("Enemy name:");
            //m_enemyName = GUILayout.TextField(m_enemyName, GUILayout.Width(150));
            //GUILayout.EndHorizontal();

            //if (GUILayout.Button("Clone enemy"))
            //{
            //    CloneCharacter(m_enemyName);
            //}

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
            }

            GUI.color = Color.white;
        }

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
    }
}


namespace SideLoader.UI
{
    public class UIStyles
    {
        public static GUISkin WindowSkin
        {
            get
            {
                if (_customSkin == null)
                {
                    try
                    {
                        _customSkin = CreateWindowSkin();
                    }
                    catch
                    {
                        _customSkin = GUI.skin;
                    }
                }

                return _customSkin;
            }
        }

        public static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, HorizontalBar);
            GUI.color = c;
        }

        private static GUISkin _customSkin;

        public static Texture2D m_nofocusTex;
        public static Texture2D m_focusTex;

        private static GUIStyle _horizBarStyle;

        private static GUIStyle HorizontalBar
        {
            get
            {
                if (_horizBarStyle == null)
                {
                    _horizBarStyle = new GUIStyle();
                    _horizBarStyle.normal.background = Texture2D.whiteTexture;
                    _horizBarStyle.margin = new RectOffset(0, 0, 4, 4);
                    _horizBarStyle.fixedHeight = 2;
                }

                return _horizBarStyle;
            }
        }

        private static GUISkin CreateWindowSkin()
        {
            var newSkin = Object.Instantiate(GUI.skin);
            Object.DontDestroyOnLoad(newSkin);

            m_nofocusTex = MakeTex(550, 700, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            m_focusTex = MakeTex(550, 700, new Color(0.3f, 0.3f, 0.3f, 1f));

            newSkin.window.normal.background = m_nofocusTex;
            newSkin.window.onNormal.background = m_focusTex;

            newSkin.box.normal.textColor = Color.white;
            newSkin.window.normal.textColor = Color.white;
            newSkin.button.normal.textColor = Color.white;
            newSkin.textField.normal.textColor = Color.white;
            newSkin.label.normal.textColor = Color.white;

            return newSkin;
        }

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
    }
}
