using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SideLoader.UI.Inspectors.Reflection
{
    public class TypeTreeDropdown
    {
        public event Action<Type> OnValueSelected;

        internal readonly Type m_baseType;
        internal readonly List<Type> m_typeOptions;

        internal GameObject m_uiContent;
        internal Dropdown m_dropdown;

        public TypeTreeDropdown(Type type, GameObject parent, Type currentType, Action<Type> listener)
        {
            m_baseType = type;
            m_typeOptions = At.GetImplementationsOf(m_baseType).OrderBy(it => it.Name).ToList();

            OnValueSelected += listener;

            ConstructUI(parent, currentType);
        }

        internal void InvokeOnValueSelected(int value)
        {
            var type = m_typeOptions[value];
            OnValueSelected.Invoke(type);
        }

        private void ConstructUI(GameObject parent, Type currentType)
        {
            m_uiContent = UIFactory.CreateDropdown(parent, out m_dropdown);

            var dropLayout = m_uiContent.AddComponent<LayoutElement>();
            dropLayout.minHeight = 25;
            dropLayout.minWidth = 300;
            dropLayout.flexibleHeight = 0;
            dropLayout.flexibleWidth = 0;

            m_dropdown.options.AddRange(m_typeOptions.Select(it => new Dropdown.OptionData { text = it.Name }));

            int idx = !currentType.IsAbstract && !currentType.IsInterface
                ? idx = m_typeOptions.IndexOf(currentType)
                : idx = 0; 

            m_dropdown.value = idx;
            InvokeOnValueSelected(idx);

            m_dropdown.onValueChanged.AddListener(InvokeOnValueSelected);
        }
    }
}
