﻿using SideLoader.UI.Editor;
using UnityEngine;
using UnityEngine.UI;
using SideLoader.UI.SLPackViewer;

namespace SideLoader.UI.Modules
{
    public class SLPacksPage : MainMenu.Page
    {
        public override string Name => "SL Packs";

        public static SLPacksPage Instance { get; internal set; }

        public override void Init()
        {
            Instance = this;

            ConstructMenu();

            new SLPackListView();

            new InspectorManager();

            SLPackListView.Instance.Init();
        }

        public override void Update()
        {
            //SLPackListView.Instance.Update();
            InspectorManager.Instance.Update();
        }

        private void ConstructMenu()
        {
            GameObject parent = MainMenu.Instance.PageViewport;

            Content = UIFactory.CreateHorizontalGroup(parent);
            var mainGroup = Content.GetComponent<HorizontalLayoutGroup>();
            mainGroup.padding.left = 1;
            mainGroup.padding.right = 1;
            mainGroup.padding.top = 1;
            mainGroup.padding.bottom = 1;
            mainGroup.spacing = 3;
            mainGroup.childForceExpandHeight = true;
            mainGroup.childForceExpandWidth = true;
            mainGroup.childControlHeight = true;
            mainGroup.childControlWidth = true;
        }
    }
}
