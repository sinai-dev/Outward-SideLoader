using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SideLoader.Helpers;
using SideLoader.UI;
using System.Collections;

namespace SideLoader.Inspectors.Reflection
{
    public class InteractiveValue
    {
        public static Type GetIValueForType(Type type)
        {
            if (type.FullName.StartsWith("System.Nullable"))
                return typeof(InteractiveNullable);
            else if (type == typeof(bool))
                return typeof(InteractiveBool);
            else if (type == typeof(string))
                return typeof(InteractiveString);
            else if (type.IsPrimitive)
                return typeof(InteractiveNumber);
            else if (typeof(Enum).IsAssignableFrom(type))
            {
                if (type.GetCustomAttributes(typeof(FlagsAttribute), true) is object[] attributes && attributes.Length > 0)
                    return typeof(InteractiveFlags);
                else
                    return typeof(InteractiveEnum);
            }
            else if (InteractiveUnityStruct.SupportsType(type))
                return typeof(InteractiveUnityStruct);
            else if (typeof(Transform).IsAssignableFrom(type))
                return typeof(InteractiveValue);
            else if (typeof(IDictionary).IsAssignableFrom(type))
                return typeof(InteractiveDictionary);
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return typeof(InteractiveEnumerable);
            else
                return typeof(InteractiveValue);
        }

        public static InteractiveValue Create(object value, Type fallbackType, bool dontUseValueType = false)
        {
            var type = dontUseValueType 
                        ? fallbackType 
                        : value?.GetType() ?? fallbackType;

            var iType = GetIValueForType(type);

            return (InteractiveValue)Activator.CreateInstance(iType, new object[] { value, fallbackType });
        }

        // ~~~~~~~~~ Instance ~~~~~~~~~

        public InteractiveValue(object value, Type valueType)
        {
            this.Value = value;
            this.FallbackType = valueType;
        }

        public CacheObjectBase Owner;

        public object Value;
        public readonly Type FallbackType;

        public virtual bool HasSubContent => false;
        public virtual bool SubContentWanted => false;
        public virtual bool WantInspectBtn => true;
        public virtual bool WantCreateDestroyBtn => true;

        public string DefaultLabel => m_defaultLabel ?? GetDefaultLabel();
        internal string m_defaultLabel;
        internal string m_richValueType;

        public bool m_UIConstructed;

        public virtual void OnDestroy()
        {
            if (this.m_valueContent)
            {
                m_valueContent.transform.SetParent(null, false);
                m_valueContent.SetActive(false); 
                GameObject.Destroy(this.m_valueContent.gameObject);
            }

            DestroySubContent();
        }

        public virtual void DestroySubContent()
        {
            if (this.m_subContentParent && HasSubContent)
            {
                for (int i = 0; i < this.m_subContentParent.transform.childCount; i++)
                {
                    var child = m_subContentParent.transform.GetChild(i);
                    if (child)
                        GameObject.Destroy(child.gameObject);
                }
            }

            m_subContentConstructed = false;
        }

        public virtual void OnValueUpdated()
        {
            if (!m_UIConstructed)
                ConstructUI(m_mainContentParent, m_subContentParent);

            if (Owner is CacheMember ownerMember && !string.IsNullOrEmpty(ownerMember.ReflectionException))
                OnException(ownerMember);
            else 
                RefreshUIForValue();
        }

        public virtual void OnException(CacheMember member)
        {
            if (m_UIConstructed)
                m_baseLabel.text = "<color=red>" + member.ReflectionException + "</color>";
                
            Value = null;
        }

        public virtual void RefreshUIForValue()
        {
            if (this.Value == null)
            {
                m_baseLabel.text = $"<i><color=grey>not editing</color></i> ({UISyntaxHighlight.ParseFullSyntax(FallbackType, false)})";
            }
            else
            {
                GetDefaultLabel();
                m_baseLabel.text = DefaultLabel;
            }

            UpdateCreateDestroyBtnState();
            RefreshElementsAfterUpdate();
        }

        public void RefreshElementsAfterUpdate()
        {
            if (WantInspectBtn)
            {
                bool shouldShowInspect = !Value.IsNullOrDestroyed();

                if (m_inspectButton.activeSelf != shouldShowInspect)
                    m_inspectButton.SetActive(shouldShowInspect);
            }

            bool subContentWanted = SubContentWanted;
            if (Owner is CacheMember cm && (!cm.HasEvaluated || !string.IsNullOrEmpty(cm.ReflectionException)))
                subContentWanted = false;

            if (HasSubContent)
            {
                if (m_subExpandBtn.gameObject.activeSelf != subContentWanted)
                    m_subExpandBtn.gameObject.SetActive(subContentWanted);

                if (!subContentWanted && m_subContentParent.activeSelf)
                    ToggleSubcontent();
            }
        }

        internal virtual void OnCreateDestroyClicked()
        {
            if (Value == null)
            {
                // todo use dropdown type (if valid)
                Value = At.TryCreateDefault(FallbackType);
                SetValueFromThis();
            }
            else
            {
                BeginConfirmDestroy();
            }
        }

        internal void SetValueFromThis()
        {
            this.Owner.SetValue();
            this.OnValueUpdated();
            RefreshElementsAfterUpdate();
            UpdateCreateDestroyBtnState();
        }

        internal void BeginConfirmDestroy()
        {
            bool subWasActive = m_subContentParent?.gameObject.activeSelf ?? false;

            var wasActiveGOs = new List<GameObject>();
            foreach (Transform child in m_valueContent.transform)
            {
                if (!child.gameObject.activeSelf)
                    continue;

                wasActiveGOs.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }

            if (m_subContentConstructed && subWasActive)
                m_subContentParent.gameObject.SetActive(false);

            var cancelBtnObj = UIFactory.CreateButton(m_valueContent, new Color(0.1f, 0.4f, 0.1f));
            var cancelLayout = cancelBtnObj.AddComponent<LayoutElement>();
            cancelLayout.minWidth = 80;
            cancelLayout.minHeight = 25;
            var cancelText = cancelBtnObj.GetComponentInChildren<Text>();
            cancelText.text = "< Cancel";

            var confirmBtnObj = UIFactory.CreateButton(m_valueContent, new Color(0.4f, 0.1f, 0.1f));
            var confirmLayout = confirmBtnObj.AddComponent<LayoutElement>();
            confirmLayout.minWidth = 80;
            confirmLayout.minHeight = 25;
            var confirmText = confirmBtnObj.GetComponentInChildren<Text>();
            confirmText.text = "Delete";
            
            var cancelBtn = cancelBtnObj.GetComponent<Button>();
            cancelBtn.onClick.AddListener(() =>
            {
                Close(false);
            });

            var confirmBtn = confirmBtnObj.GetComponent<Button>();
            confirmBtn.onClick.AddListener(() => 
            {
                Value = null;

                if (m_subContentParent.gameObject.activeSelf)
                    m_subContentParent.gameObject.SetActive(false);

                SetValueFromThis();
                Close(true);
            });

            void Close(bool destroyed)
            {
                GameObject.Destroy(cancelBtnObj);
                GameObject.Destroy(confirmBtnObj);

                foreach (var obj in wasActiveGOs)
                    obj.SetActive(true);

                if (destroyed)
                    OnValueUpdated();
                else if (subWasActive)
                    m_subContentParent.gameObject.SetActive(true);
            }
        }

        internal virtual void UpdateCreateDestroyBtnState()
        {
            if (!WantCreateDestroyBtn || this.Owner is CacheEnumerated)
                return;

            if (Value != null)
            {
                m_createDestroyBtn.GetComponentInChildren<Text>().text = "X";
                var colors = m_createDestroyBtn.colors;
                colors.normalColor = new Color(0.45f, 0.15f, 0.15f);
                colors.pressedColor = new Color(0.45f, 0.15f, 0.15f);
                m_createDestroyBtn.colors = colors;
            }
            else
            {
                m_createDestroyBtn.GetComponentInChildren<Text>().text = "+";
                var colors = m_createDestroyBtn.colors;
                colors.normalColor = new Color(0.15f, 0.45f, 0.15f);
                colors.pressedColor = new Color(0.15f, 0.45f, 0.15f);
                m_createDestroyBtn.colors = colors;
            }
        }

        public virtual void ConstructSubcontent()
        {
            m_subContentConstructed = true;
        }

        public void ToggleSubcontent()
        {
            if (!this.m_subContentParent.activeSelf)
            {
                this.m_subContentParent.SetActive(true);
                this.m_subContentParent.transform.SetAsLastSibling();
                m_subExpandBtn.GetComponentInChildren<Text>().text = "▼";
            }
            else
            {
                this.m_subContentParent.SetActive(false);
                m_subExpandBtn.GetComponentInChildren<Text>().text = "▲";
            }

            OnToggleSubcontent(m_subContentParent.activeSelf);

            RefreshElementsAfterUpdate();
        }

        internal virtual void OnToggleSubcontent(bool toggle)
        {
            if (!m_subContentConstructed)
                ConstructSubcontent();
        }

        public string GetDefaultLabel(bool updateType = true)
        {
            var valueType = Value?.GetType() ?? this.FallbackType;
            if (updateType)
                m_richValueType = UISyntaxHighlight.ParseFullSyntax(valueType, true);

            if (!Owner.HasEvaluated)
                return m_defaultLabel = $"<i><color=grey>Not yet evaluated</color> ({m_richValueType})</i>";

            if (Value.IsNullOrDestroyed())
                return m_defaultLabel = $"<color=grey>null</color> ({m_richValueType})";

            string label;

            if (Value is TextAsset textAsset)
            {
                label = textAsset.text;

                if (label.Length > 10)
                    label = $"{label.Substring(0, 10)}...";

                label = $"\"{label}\" {textAsset.name} ({m_richValueType})";
            }
            else if (Value is EventSystem)
            {
                label = m_richValueType;
            }
            else
            {
                var toString = (string)valueType.GetMethod("ToString", new Type[0])?.Invoke(Value, null) 
                                ?? Value.ToString();

                var fullnametemp = valueType.ToString();
                if (fullnametemp.StartsWith("Il2CppSystem"))
                    fullnametemp = fullnametemp.Substring(6, fullnametemp.Length - 6);

                var temp = toString.Replace(fullnametemp, "").Trim();

                if (string.IsNullOrEmpty(temp))
                {
                    label = m_richValueType;
                }
                else
                {
                    if (toString.Length > 200)
                        toString = toString.Substring(0, 200) + "...";

                    label = toString;

                    var unityType = $"({valueType.FullName})";
                    if (Value is UnityEngine.Object && label.Contains(unityType))
                        label = label.Replace(unityType, $"({m_richValueType})");
                    else
                        label += $" ({m_richValueType})";
                }
            }

            return m_defaultLabel = label;
        }

        internal Button m_createDestroyBtn;

        #region UI CONSTRUCTION

        internal GameObject m_mainContentParent;
        internal GameObject m_subContentParent;

        internal GameObject m_valueContent;
        internal GameObject m_inspectButton;
        internal Text m_baseLabel;

        internal Button m_subExpandBtn;
        internal bool m_subContentConstructed;

        public virtual void ConstructUI(GameObject parent, GameObject subGroup)
        {
            m_UIConstructed = true;

            m_valueContent = UIFactory.CreateHorizontalGroup(parent, new Color(1, 1, 1, 0));
            m_valueContent.name = "InteractiveValue.ValueContent";
            var mainRect = m_valueContent.GetComponent<RectTransform>();
            mainRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
            var mainGroup = m_valueContent.GetComponent<HorizontalLayoutGroup>();
            mainGroup.childForceExpandWidth = false;
            mainGroup.childControlWidth = true;
            mainGroup.childForceExpandHeight = false;
            mainGroup.childControlHeight = true;
            mainGroup.spacing = 4;
            mainGroup.childAlignment = TextAnchor.UpperLeft;
            var mainLayout = m_valueContent.AddComponent<LayoutElement>();
            mainLayout.flexibleWidth = 9000;
            mainLayout.minWidth = 175;
            mainLayout.minHeight = 25;
            mainLayout.flexibleHeight = 0;

            if (WantCreateDestroyBtn && !(this.Owner is CacheEnumerated))
            {
                // make create/destroy button
                var m_btnObj = UIFactory.CreateButton(m_valueContent, new Color(0.15f, 0.45f, 0.15f));
                var btnLayout = m_btnObj.AddComponent<LayoutElement>();
                btnLayout.minWidth = 25;
                btnLayout.minHeight = 25;

                m_btnObj.transform.SetAsFirstSibling();

                var btnText = btnLayout.GetComponentInChildren<Text>();
                btnText.text = "+";

                m_createDestroyBtn = m_btnObj.GetComponent<Button>();
                m_createDestroyBtn.onClick.AddListener(OnCreateDestroyClicked);
            }

            // subcontent expand button
            if (HasSubContent)
            {
                var subBtnObj = UIFactory.CreateButton(m_valueContent, new Color(0.1f, 0.4f, 0.1f));
                var createBtnLayout = subBtnObj.AddComponent<LayoutElement>();
                createBtnLayout.minHeight = 25;
                createBtnLayout.minWidth = 25;
                createBtnLayout.flexibleWidth = 0;
                createBtnLayout.flexibleHeight = 0;
                var createBtnText = subBtnObj.GetComponentInChildren<Text>();
                createBtnText.text = "▲";
                m_subExpandBtn = subBtnObj.GetComponent<Button>();
                m_subExpandBtn.onClick.AddListener(() =>
                {
                    ToggleSubcontent();
                });

                if (!SubContentWanted)
                    subBtnObj.gameObject.SetActive(false);
            }

            // inspect button

            m_inspectButton = UIFactory.CreateButton(m_valueContent, new Color(0.1f, 0.5f, 0.1f, 0.2f));
            var inspectLayout = m_inspectButton.AddComponent<LayoutElement>();
            inspectLayout.minWidth = 60;
            inspectLayout.minHeight = 25;
            inspectLayout.flexibleHeight = 0;
            inspectLayout.flexibleWidth = 0;
            var inspectText = m_inspectButton.GetComponentInChildren<Text>();
            inspectText.text = "Edit";
            var inspectBtn = m_inspectButton.GetComponent<Button>();

            inspectBtn.onClick.AddListener(OnInspectClicked);
            void OnInspectClicked()
            {
                if (!Value.IsNullOrDestroyed(false))
                    InspectorManager.Instance.Inspect(this.Value, null, this.Owner as CacheMember);
            }

            m_inspectButton.SetActive(false);

            // value label

            var labelObj = UIFactory.CreateLabel(m_valueContent, TextAnchor.MiddleLeft);
            m_baseLabel = labelObj.GetComponent<Text>();
            var labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 9000;
            labelLayout.minHeight = 25;

            m_subContentParent = subGroup;
        }

#endregion
    }
}
