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

        private static Rect m_rect = new Rect(5, 5, 250, 375);

        public static bool ShowDebug = false;

        private static bool m_debugFileExists = false;

        private int SelectedID = 0;
        private int NewID = 0;

        private SL_Item.TemplateBehaviour m_templateBehaviour = SL_Item.TemplateBehaviour.DestroyEffects;

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

            GUILayout.Label("Enter an Item ID to generate a template from. This will also save all material textures (if any).");
            GUILayout.Label("Templates are generated to the folder Mods/SideLoader/_GENERATED.");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item ID:");
            string input = GUILayout.TextField(SelectedID.ToString(), GUILayout.Width(150));
            if (int.TryParse(input, out int id))
            {
                SelectedID = id;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("New ID:");
            string input2 = GUILayout.TextField(NewID.ToString(), GUILayout.Width(150));
            if (int.TryParse(input2, out int id2))
            {
                NewID = id2;
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Effects Behaviour:");
            BehaviourButton(SL_Item.TemplateBehaviour.DestroyEffects, "Replace All Effects");
            BehaviourButton(SL_Item.TemplateBehaviour.OverrideEffects, "Override By Transform");
            BehaviourButton(SL_Item.TemplateBehaviour.NONE, "Add effects on top");

            GUILayout.Space(15);

            if (GUILayout.Button("Generate template"))
            {
                GenerateTemplate();
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

        private void BehaviourButton(SL_Item.TemplateBehaviour _behaviour, string _label)
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

        private void GenerateTemplate()
        {
            if (ResourcesPrefabManager.Instance.GetItemPrefab(SelectedID) is Item item)
            {
                var template = SL_Item.ParseItemToTemplate(item);

                template.New_ItemID = NewID;
                template.EffectBehaviour = m_templateBehaviour;

                var itemfolder = SL.GENERATED_FOLDER + @"\Items\" + item.gameObject.name;
                Serializer.SaveToXml(itemfolder, item.Name, template);

                CustomItemVisuals.SaveAllItemTextures(item, itemfolder + @"\Textures");
            }
            else
            {
                SL.Log("DEBUG MENU: ID " + SelectedID + " is invalid!");
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
