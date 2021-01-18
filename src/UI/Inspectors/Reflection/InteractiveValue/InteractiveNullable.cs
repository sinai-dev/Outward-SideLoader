using SideLoader.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SideLoader.Inspectors.Reflection
{
    public class InteractiveNullable : InteractiveValue
    {
        public Type ElementType;
        public InteractiveValue SubIValue;

        public object UnderlyingValue
        {
            get
            {
                if (this.Value == null)
                    return null;

                return At.GetFieldInfo(FallbackType, "value").GetValue(Value);
            }
        }

        public GameObject m_uiParent;
        public GameObject m_uiSubGroupParent;

        public override bool WantInspectBtn => false;
        public override bool SubContentWanted => false;

        public InteractiveNullable(object value, Type valueType) : base(value, valueType)
        {
            ElementType = valueType.GetGenericArguments()[0];
        }

        public override void OnValueUpdated()
        {
            base.OnValueUpdated();

            if (UnderlyingValue == null && SubIValue != null)
                SubIValue = null;
            else if (UnderlyingValue != null && SubIValue == null)
            { 
                SubIValue = Create(UnderlyingValue, ElementType);
                SubIValue.Owner = this.Owner;
                SubIValue.m_mainContentParent = this.m_mainContentParent;
                SubIValue.m_subContentParent = this.m_subContentParent;
                SubIValue.ConstructUI(m_valueContent, m_uiSubGroupParent);
            }

            SubIValue?.OnValueUpdated();
            RefreshUIForValue();
        }

        public override void RefreshUIForValue()
        {
            base.RefreshUIForValue();

            if (this.SubIValue == null)
            {
                m_baseLabel.gameObject.SetActive(true);
                m_baseLabel.text = $"<i><color=grey>not editing</color></i> ({UISyntaxHighlight.ParseFullSyntax(ElementType, false)})";
            }
            else
            {
                m_baseLabel.gameObject.SetActive(false);
                SubIValue.RefreshUIForValue();
            }

            UpdateCreateDestroyBtnState();
        }

        internal override void OnCreateDestroyClicked()
        {
            if (SubIValue == null)
            {
                var value = UnderlyingValue ?? Activator.CreateInstance(ElementType);

                SubIValue = Create(value, ElementType);
                SubIValue.Owner = this.Owner;
                SubIValue.m_mainContentParent = this.m_mainContentParent;
                SubIValue.m_subContentParent = this.m_subContentParent;

                SubIValue.ConstructUI(m_valueContent, m_uiSubGroupParent);
            }
            else
            {
                SubIValue.OnDestroy();
                SubIValue = null;
                if (m_subContentParent.gameObject.activeSelf)
                    m_subContentParent.gameObject.SetActive(false);
            }

            this.Owner.SetValue();
            this.OnValueUpdated();
            RefreshElementsAfterUpdate();
            UpdateCreateDestroyBtnState();
        }

        internal override void UpdateCreateDestroyBtnState()
        {
            if (SubIValue != null)
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

        public override void ConstructUI(GameObject parent, GameObject subGroup)
        {
            base.ConstructUI(parent, subGroup);

            m_uiParent = parent;
            m_uiSubGroupParent = subGroup;

            //m_baseLabel.gameObject.SetActive(false);
            //m_inspectButton.gameObject.SetActive(false);


        }
    }
}
