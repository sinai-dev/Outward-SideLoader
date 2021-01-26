using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SideLoader.UI.Inspectors.Reflection
{
    public class InteractiveSLDamage : InteractiveValue
    {
        public InteractiveSLDamage(object value, Type valueType) : base(value, valueType)
        {
            RefSLDamage = value as SL_Damage;
        }

        public override bool HasSubContent => false;
        public override bool SubContentWanted => false;
        public override bool WantCreateDestroyBtn => true;
        public override bool WantInspectBtn => false;

        public SL_Damage RefSLDamage;

        public override void OnValueUpdated()
        {
            RefSLDamage = Value as SL_Damage;

            base.OnValueUpdated();
        }

        //internal override void QuickSave()
        //{
        //    if (this.m_subContentConstructed)
        //        this.OnApplyClicked();

        //    base.QuickSave();
        //}

        public override void RefreshUIForValue()
        {
            base.RefreshUIForValue();

            if (RefSLDamage == null)
                return;

            m_valueInput.gameObject.SetActive(true);

            SetUIDamageType(RefSLDamage.Type);
            SetUIDamageValue(RefSLDamage.Damage);
        }

        internal void SetUIDamageType(DamageType.Types type) => m_typeDropdown.value = (int)type;
        internal void SetUIDamageValue(float value) => m_valueInput.text = value.ToString();

        internal void OnApplyClicked()
        {
            try
            {
                if (string.IsNullOrEmpty(m_valueInput.text))
                    m_valueInput.text = "0.0";

                RefSLDamage.Type = (DamageType.Types)m_typeDropdown.value;
                RefSLDamage.Damage = float.Parse(m_valueInput.text);

                Owner.SetValue();
                //RefreshUIForValue();
            }
            catch (Exception e)
            {
                SL.LogWarning($"Could not parse input! {e.GetType()}: {e.Message}");
            }
        }

        // ======= UI Construction ======

        internal Dropdown m_typeDropdown;
        internal InputField m_valueInput;

        internal Button m_applyBtn;

        public override void ConstructUI(GameObject parent, GameObject subGroup)
        {
            base.ConstructUI(parent, subGroup); 

            m_baseLabel.gameObject.SetActive(false);

            var typeDropObj = UIFactory.CreateDropdown(m_valueContent, out m_typeDropdown);
            var typeLayout = typeDropObj.AddComponent<LayoutElement>();
            typeLayout.minWidth = 150;
            typeLayout.minHeight = 25;
            m_typeDropdown.options.AddRange(new List<Dropdown.OptionData> 
            {
                new Dropdown.OptionData{ text = "Physical" },
                new Dropdown.OptionData{ text = "Ethereal" },
                new Dropdown.OptionData{ text = "Decay" },
                new Dropdown.OptionData{ text = "Electric" },
                new Dropdown.OptionData{ text = "Frost" },
                new Dropdown.OptionData{ text = "Fire" },
                new Dropdown.OptionData{ text = "DarkOLD" },
                new Dropdown.OptionData{ text = "LightOLD" },
                new Dropdown.OptionData{ text = "Raw" },
                new Dropdown.OptionData{ text = "COUNT (none)" },
            });

            var inputObj = UIFactory.CreateInputField(m_valueContent);
            var inputLayout = inputObj.AddComponent<LayoutElement>();
            inputLayout.minWidth = 120;
            inputLayout.minHeight = 25;
            inputLayout.flexibleWidth = 0;

            m_valueInput = inputObj.GetComponent<InputField>();
            m_valueInput.gameObject.SetActive(false);

            var applyBtnObj = UIFactory.CreateButton(m_valueContent, new Color(0.2f, 0.2f, 0.2f));
            var applyLayout = applyBtnObj.AddComponent<LayoutElement>();
            applyLayout.minWidth = 50;
            applyLayout.minHeight = 25;
            applyLayout.flexibleWidth = 0;
            m_applyBtn = applyBtnObj.GetComponent<Button>();
            m_applyBtn.onClick.AddListener(OnApplyClicked);

            var applyText = applyBtnObj.GetComponentInChildren<Text>();
            applyText.text = "Apply";
        }
    }
}
