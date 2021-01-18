using System;
using System.Collections.Generic;
using System.Linq;
//using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SideLoader.Helpers;
using SideLoader.UI.Shared;

namespace SideLoader.UI.Modules
{
    public class PageThree : MainMenu.Page
    {
        public override string Name => "Todo";


        public override void Init()
        {
            ConstructUI();
        }

        public override void Update()
        {
            // not needed?
        }




        internal void ConstructUI()
        {
            GameObject parent = MainMenu.Instance.PageViewport;

            Content = UIFactory.CreateVerticalGroup(parent, new Color(0.15f, 0.15f, 0.15f));
            var mainGroup = Content.GetComponent<VerticalLayoutGroup>();
            mainGroup.padding.left = 4;
            mainGroup.padding.right = 4;
            mainGroup.padding.top = 4;
            mainGroup.padding.bottom = 4;
            mainGroup.spacing = 5;
            mainGroup.childForceExpandHeight = false;
            mainGroup.childForceExpandWidth = true;
            mainGroup.childControlHeight = true;
            mainGroup.childControlWidth = true;

            // ~~~~~ Title ~~~~~

            GameObject titleObj = UIFactory.CreateLabel(Content, TextAnchor.UpperLeft);
            Text titleLabel = titleObj.GetComponent<Text>();
            titleLabel.text = "TODO";
            titleLabel.fontSize = 20;
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.minHeight = 30;
            titleLayout.flexibleHeight = 0;
        }
    }
}
