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

        private int SelectedID = 0;
        private int NewID = 0; 
        private bool m_texturesOnly = false;
        private bool m_replaceEffects = true;

        // temp debug
        private string m_enemyName = "";

        internal void Awake()
        {
            Instance = this;

            if (File.Exists(SL.SL_FOLDER + @"\debug.txt"))
            {
                ShowDebug = true;
            }
        }

        internal void OnGUI()
        {
            if (ShowDebug)
            {
                m_rect = GUI.Window(29, m_rect, WindowFunction, "SideLoader Template Generator");
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
            //    CloneEnemy();
            //}

            GUILayout.EndArea();
        }

        private void GenerateTemplate()
        {
            if (ResourcesPrefabManager.Instance.GetItemPrefab(SelectedID) is Item item)
            {
                var item2 = ItemManager.Instance.GenerateItemNetwork(item.ItemID);

                var template = SL_Item.ParseItemToTemplate(item2);

                template.OnlyChangeVisuals = m_texturesOnly;
                template.ReplaceEffects = m_replaceEffects;
                template.New_ItemID = NewID;

                var itemfolder = SL.GENERATED_FOLDER + @"\Items\" + item.gameObject.name;
                Serializer.SaveToXml(itemfolder, item.Name, template);

                CustomItemVisuals.SaveAllItemTextures(item, itemfolder + @"\Textures");

                Destroy(item2.gameObject);
            }
            else
            {
                SL.Log("DEBUG MENU: ID " + SelectedID + " is invalid!");
            }
        }

        //private void CloneEnemy()
        //{
        //    try
        //    {
        //        if (GameObject.Find(m_enemyName) is GameObject target)
        //        {
        //            // base cloning
        //            target.SetActive(false);

        //            var origchar = target.GetComponent<Character>();
        //            bool origsetting = origchar.DisableAfterInit;
        //            origchar.DisableAfterInit = false;

        //            var clone = Instantiate(target);
        //            clone.SetActive(false);

        //            origchar.DisableAfterInit = origsetting;

        //            target.SetActive(true);

        //            // fix clone UIDs, etc
        //            clone.name = "[CLONE] " + target.name;
        //            var character = clone.GetComponent<Character>();
        //            At.SetValue(UID.Generate(), typeof(Character), character, "m_uid");

        //            clone.GetPhotonView().viewID = PhotonNetwork.AllocateSceneViewID();

        //            var oldObjects = new List<GameObject>();
        //            foreach (var item in character.GetComponentsInChildren<Item>())
        //            {
        //                var new_item = ItemManager.Instance.GenerateItemNetwork(item.ItemID);
        //                new_item.transform.parent = item.transform.parent;

        //                oldObjects.Add(item.gameObject);
        //            }
        //            for (int i = 0; i < oldObjects.Count; i++)
        //            {
        //                int j = oldObjects.Count;
        //                var obj = oldObjects[i];
        //                DestroyImmediate(obj);
        //            }

        //            //// todo same for droptable components
        //            //var lootable = clone.GetComponent<LootableOnDeath>();
                    
        //            //var oldTables = new List<GameObject>();
                    

        //            var charAI = clone.GetComponent<CharacterAI>();

        //            var navmeshAgent = clone.GetComponent<UnityEngine.AI.NavMeshAgent>();
        //            At.SetValue(navmeshAgent, typeof(CharacterAI), charAI, "m_navMeshAgent");

        //            var airoot = clone.GetComponentInChildren<AIRoot>();
        //            At.SetValue(charAI, typeof(AIRoot), airoot, "m_charAI");

        //            clone.SetActive(true);
        //            At.Call(character, "Awake", new object[0]);

        //            clone.transform.position = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
        //        }
        //        else
        //        {
        //            throw new Exception("Enemy not found: " + m_enemyName);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        SL.Log("Error cloning enemy: " + e.Message + "\r\nStack: " + e.StackTrace, 1);
        //    }
        //}
    }
}
