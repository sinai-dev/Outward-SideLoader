using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SideLoader.UI.Shared;
using System.IO;
using System.Linq;
using SideLoader.Helpers;

namespace SideLoader.UI.Modules
{
    public class DebugConsole
    {
        public static DebugConsole Instance { get; private set; }

        public static readonly List<string> AllMessages = new List<string>();
        public static readonly List<Text> MessageHolders = new List<Text>();

        // logs that occured before the actual UI was ready.
        // these ones include the hex color codes.
        internal static readonly List<string> s_preInitMessages = new List<string>();

        private InputField m_textInput;
        internal const int MAX_TEXT_LEN = 10000;

        public DebugConsole(GameObject parent)
        {
            Instance = this;

            ConstructUI(parent);

            // append messages that logged before we were set up
            string preAppend = "";
            for (int i = s_preInitMessages.Count - 1; i >= 0; i--)
            {
                var msg = s_preInitMessages[i];
                if (preAppend != "")
                    preAppend += "\r\n";
                preAppend += msg;
            }
            m_textInput.text = preAppend;
        }

        public static void Log(string message)
        {
            Log(message, null);
        }

        public static void Log(string message, Color color)
        {
            Log(message, color.ToHex());
        }

        public static void Log(string message, string hexColor)
        {
            message = $"{AllMessages.Count}: {message}";

            AllMessages.Add(message);
            //s_streamWriter?.WriteLine(message);

            if (hexColor != null)
                message = $"<color=#{hexColor}>{message}</color>";
            
            if (Instance?.m_textInput)
            {
                var input = Instance.m_textInput;
                var wanted = $"{message}\n{input.text}";

                if (wanted.Length > MAX_TEXT_LEN)
                    wanted = wanted.Substring(0, MAX_TEXT_LEN);

                input.text = wanted;
            }
            else
                s_preInitMessages.Add(message);
        }

        public void ConstructUI(GameObject parent)
        {
            var mainObj = UIFactory.CreateVerticalGroup(parent, new Color(0.1f, 0.1f, 0.1f, 1.0f));

            var mainGroup = mainObj.GetComponent<VerticalLayoutGroup>();
            mainGroup.childControlHeight = true;
            mainGroup.childControlWidth = true;
            mainGroup.childForceExpandHeight = true;
            mainGroup.childForceExpandWidth = true;

            var mainImage = mainObj.GetComponent<Image>();
            mainImage.maskable = true;

            var mask = mainObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var mainLayout = mainObj.AddComponent<LayoutElement>();
            mainLayout.minHeight = 190;
            mainLayout.flexibleHeight = 0;

            #region LOG AREA
            var logAreaObj = UIFactory.CreateHorizontalGroup(mainObj);
            var logAreaGroup = logAreaObj.GetComponent<HorizontalLayoutGroup>();
            logAreaGroup.childControlHeight = true;
            logAreaGroup.childControlWidth = true;
            logAreaGroup.childForceExpandHeight = true;
            logAreaGroup.childForceExpandWidth = true;

            var logAreaLayout = logAreaObj.AddComponent<LayoutElement>();
            logAreaLayout.preferredHeight = 190;
            logAreaLayout.flexibleHeight = 0;

            var inputScrollerObj = UIFactory.CreateSrollInputField(logAreaObj, out InputFieldScroller inputScroll, 14, new Color(0.05f, 0.05f, 0.05f));

            inputScroll.inputField.textComponent.font = UIManager.ConsoleFont;
            inputScroll.inputField.readOnly = true;

            m_textInput = inputScroll.inputField;
#endregion

            #region BOTTOM BAR

            var bottomBarObj = UIFactory.CreateHorizontalGroup(mainObj);
            LayoutElement topBarLayout = bottomBarObj.AddComponent<LayoutElement>();
            topBarLayout.minHeight = 30;
            topBarLayout.flexibleHeight = 0;

            var bottomGroup = bottomBarObj.GetComponent<HorizontalLayoutGroup>();
            bottomGroup.padding.left = 10;
            bottomGroup.padding.right = 10;
            bottomGroup.padding.top = 2;
            bottomGroup.padding.bottom = 2;
            bottomGroup.spacing = 10;
            bottomGroup.childForceExpandHeight = true;
            bottomGroup.childForceExpandWidth = false;
            bottomGroup.childControlWidth = true;
            bottomGroup.childControlHeight = true;
            bottomGroup.childAlignment = TextAnchor.MiddleLeft;

            // Debug Console label

            var bottomLabel = UIFactory.CreateLabel(bottomBarObj, TextAnchor.MiddleLeft);
            var topBarLabelLayout = bottomLabel.AddComponent<LayoutElement>();
            topBarLabelLayout.minWidth = 100;
            topBarLabelLayout.flexibleWidth = 0;
            var topBarText = bottomLabel.GetComponent<Text>();
            topBarText.fontStyle = FontStyle.Bold;
            topBarText.text = "Debug Console";
            topBarText.fontSize = 14;

            // Hide button

            var hideButtonObj = UIFactory.CreateButton(bottomBarObj);

            var hideBtnText = hideButtonObj.GetComponentInChildren<Text>();
            hideBtnText.text = "Hide";

            var hideButton = hideButtonObj.GetComponent<Button>();

            hideButton.onClick.AddListener(HideCallback);
            void HideCallback()
            {
                if (logAreaObj.activeSelf)
                {
                    logAreaObj.SetActive(false);
                    hideBtnText.text = "Show";
                    mainLayout.minHeight = 30;
                }
                else
                {
                    logAreaObj.SetActive(true);
                    hideBtnText.text = "Hide";
                    mainLayout.minHeight = 190;
                }
            }

            var hideBtnColors = hideButton.colors;
            //hideBtnColors.normalColor = new Color(160f / 255f, 140f / 255f, 40f / 255f);
            hideButton.colors = hideBtnColors;

            var hideBtnLayout = hideButtonObj.AddComponent<LayoutElement>();
            hideBtnLayout.minWidth = 80;
            hideBtnLayout.flexibleWidth = 0;

            // Clear button

            var clearButtonObj = UIFactory.CreateButton(bottomBarObj);

            var clearBtnText = clearButtonObj.GetComponentInChildren<Text>();
            clearBtnText.text = "Clear";

            var clearButton = clearButtonObj.GetComponent<Button>();

            clearButton.onClick.AddListener(ClearCallback);
            void ClearCallback()
            {
                m_textInput.text = "";
                AllMessages.Clear();
            }

            var clearBtnColors = clearButton.colors;
            //clearBtnColors.normalColor = new Color(160f/255f, 140f/255f, 40f/255f);
            clearButton.colors = clearBtnColors;

            var clearBtnLayout = clearButtonObj.AddComponent<LayoutElement>();
            clearBtnLayout.minWidth = 80;
            clearBtnLayout.flexibleWidth = 0;


            // Hide the log area after init.
            HideCallback();


            #endregion
        }
    }
}
