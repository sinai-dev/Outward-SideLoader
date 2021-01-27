using UnityEngine;

namespace SideLoader.Helpers
{
    public class ForceUnlockCursor
    {
        internal static bool ForceUnlock => s_unlockLevel > 0;
        internal static int s_unlockLevel = 0;

        internal static bool s_lastUnlockState;

        /// <summary>Call this when you want to unlock the cursor (eg, when your menu opens).</summary>
        public static void AddUnlockSource()
        {
            s_unlockLevel++;
            UpdateCursorControl();
        }

        /// <summary>Call this when you are done with unlocking the cursor (eg, when your menu closes).</summary>
        public static void RemoveUnlockSource()
        {
            if (s_unlockLevel > 0)
                s_unlockLevel--;

            UpdateCursorControl();
        }

        internal static void UpdateCursorControl()
        {
            var mainChar = CharacterManager.Instance?.GetFirstLocalCharacter();
            if (!mainChar)
                return;

            if (s_lastUnlockState != ForceUnlock)
            {
                s_lastUnlockState = ForceUnlock;

                if (mainChar.CharacterUI.PendingDemoCharSelectionScreen is Panel panel)
                {
                    if (s_lastUnlockState)
                        panel.Show();
                    else
                        panel.Hide();
                }
                else if (s_lastUnlockState)
                {
                    GameObject obj = new GameObject();
                    obj.transform.parent = mainChar.transform;
                    obj.SetActive(true);

                    Panel newPanel = obj.AddComponent<Panel>();
                    mainChar.CharacterUI.PendingDemoCharSelectionScreen = newPanel;

                    newPanel.Show();
                }
            }
        }
    }
}
