using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SideLoader
{
    public class TempDebugGui : MonoBehaviour
    {
        public static TempDebugGui Instance;

        private static Rect m_rect = new Rect(5, 5, 250, 250);

        private static int SelectedID = 0;
        private bool m_texturesOnly = false;
        private bool m_replaceEffects = true;

        internal void Awake()
        {
            Instance = this;
        }

        internal void OnGUI()
        {
            m_rect = GUI.Window(987123543, m_rect, WindowFunction, "SideLoader 2");
        }

        private void WindowFunction(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, m_rect.width, 20));

            GUILayout.BeginArea(new Rect(5, 20, m_rect.width - 10, m_rect.height - 15));

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Item ID:");
            string input = GUILayout.TextField(SelectedID.ToString(), GUILayout.Width(150));
            if (int.TryParse(input, out int id))
            {
                SelectedID = id;
            }
            GUILayout.EndHorizontal();

            m_texturesOnly = GUILayout.Toggle(m_texturesOnly, "Enable 'Only Change Visuals'?");
            m_replaceEffects = GUILayout.Toggle(m_replaceEffects, "Enable 'Replace Effects'?");

            if (GUILayout.Button("Generate template"))
            {
                if (ResourcesPrefabManager.Instance.GetItemPrefab(SelectedID) is Item item)
                {
                    var template = SL_Item.ParseItemToTemplate(item);

                    template.OnlyChangeVisuals = m_texturesOnly;
                    template.ReplaceEffects = m_replaceEffects;

                    var itemfolder = SL.SL_FOLDER + @"\_GENERATED\Items\" + item.gameObject.name;
                    Serializer.SaveToXml(itemfolder, item.Name, template);

                    CustomItems.SaveAllItemTextures(item, itemfolder + @"\Textures");
                }
                else
                {
                    SL.Log("DEBUG MENU: ID " + SelectedID + " is invalid!");
                }
            }

            GUILayout.EndArea();
        }
    }
}
