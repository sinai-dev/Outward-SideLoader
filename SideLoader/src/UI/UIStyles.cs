using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

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
            GUILayout.Box(GUIContent.none, HorizontalBar, null);
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

            m_nofocusTex = SL_GUI.MakeTex(550, 700, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            m_focusTex = SL_GUI.MakeTex(550, 700, new Color(0.3f, 0.3f, 0.3f, 1f));

            newSkin.window.normal.background = m_nofocusTex;
            newSkin.window.onNormal.background = m_focusTex;

            newSkin.box.normal.textColor = Color.white;
            newSkin.window.normal.textColor = Color.white;
            newSkin.button.normal.textColor = Color.white;
            newSkin.textField.normal.textColor = Color.white;
            newSkin.label.normal.textColor = Color.white;

            return newSkin;
        }



        public static void TranslateControls(Transform t, ref float m_translateAmount, ref float m_rotateAmount)
        {
            t.localPosition = Translate("Pos", t.position, ref m_translateAmount, false);
            t.localRotation = Quaternion.Euler(Translate("Rot", t.rotation.eulerAngles, ref m_rotateAmount, true));
        }

        public static Vector3 Translate(string label, Vector3 vector, ref float amount, bool multByTime)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, new GUILayoutOption[] { GUILayout.Width(50) });
            GUI.skin.label.alignment = TextAnchor.MiddleRight;

            GUILayout.Label("<color=cyan>X:</color>", new GUILayoutOption[] { GUILayout.Width(20) });
            PlusMinusFloat(ref vector.x, amount, multByTime);

            GUILayout.Label("<color=cyan>Y:</color>", new GUILayoutOption[] { GUILayout.Width(20) });
            PlusMinusFloat(ref vector.y, amount, multByTime);

            GUILayout.Label("<color=cyan>Z:</color>", new GUILayoutOption[] {GUILayout.Width(20) });
            PlusMinusFloat(ref vector.z, amount, multByTime);

            GUILayout.Label("+/-:", new GUILayoutOption[] {GUILayout.Width(30) });
            var input = amount.ToString("F2");
            input = GUILayout.TextField(input, new GUILayoutOption[] {GUILayout.Width(60) });
            if (float.TryParse(input, out float f))
            {
                amount = f;
            }

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();

            return vector;
        }

        private static void PlusMinusFloat(ref float f, float amount, bool multByTime)
        {
            string s = f.ToString("F3");
            s = GUILayout.TextField(s, new GUILayoutOption[] { GUILayout.Width(60) });
            if (float.TryParse(s, out float f2))
            {
                f = f2;
            }
            if (GUILayout.RepeatButton("-", new GUILayoutOption[] {GUILayout.Width(20) }))
            {
                f -= multByTime ? amount * Time.deltaTime : amount;
            }
            if (GUILayout.RepeatButton("+", new GUILayoutOption[] {GUILayout.Width(20) }))
            {
                f += multByTime ? amount * Time.deltaTime : amount;
            }
        }
    }
}
