using System;
using System.Reflection;
using UnityEngine;

namespace SideLoader.UI.Inspectors.Reflection
{
    public class CacheField : CacheMember
    {
        public override bool IsStatic => (MemInfo as FieldInfo).IsStatic;

        public override Type FallbackType => (MemInfo as FieldInfo).FieldType;

        public CacheField(FieldInfo fieldInfo, object declaringInstance, GameObject parent) : base(fieldInfo, declaringInstance, parent)
        {
            CreateIValue(null, fieldInfo.FieldType);
        }

        public override void UpdateReflection()
        {
            var fi = MemInfo as FieldInfo;
            IValue.Value = fi.GetValue(fi.IsStatic ? null : DeclaringInstance);

            m_evaluated = true;
            ReflectionException = null;
        }

        public override void SetValue()
        {
            var fi = MemInfo as FieldInfo;

            if (IValue is InteractiveNullable iNullable)
                iNullable.Value = iNullable.SubIValue?.Value;

            fi.SetValue(fi.IsStatic ? null : DeclaringInstance, IValue.Value);

            if (this.ParentInspector?.ParentMember != null)
            {
                this.ParentInspector.ParentMember.SetValue();
            }
        }
    }
}
