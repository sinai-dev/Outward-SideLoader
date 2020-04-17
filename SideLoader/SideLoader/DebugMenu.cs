using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace SideLoader
{
    public class DebugMenu : MonoBehaviour
    {
        public static DebugMenu Instance;

        private static Rect m_rect = new Rect(5, 5, 250, 300);

        public static bool ShowDebug = false;

        private static bool m_debugFileExists = false;

        private int SelectedID = 0;
        private int NewID = 0; 
        private bool m_texturesOnly = false;
        private bool m_replaceEffects = true;

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
                m_rect = GUI.Window(29, m_rect, WindowFunction, "SideLoader Debug (F6 Toggle)");
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

            m_texturesOnly = GUILayout.Toggle(m_texturesOnly, "Enable 'Only Change Visuals'?");
            m_replaceEffects = GUILayout.Toggle(m_replaceEffects, "Enable 'Replace Effects'?");

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

        private void GenerateTemplate()
        {
            if (ResourcesPrefabManager.Instance.GetItemPrefab(SelectedID) is Item item)
            {
                var template = SL_Item.ParseItemToTemplate(item);

                template.OnlyChangeVisuals = m_texturesOnly;
                template.ReplaceEffects = m_replaceEffects;
                template.New_ItemID = NewID;

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
