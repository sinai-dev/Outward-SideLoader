using System;
using System.Reflection;

namespace SideLoader
{
    /// <summary>
    /// AccessTools 
    /// Some helpers for Reflection (GetValue, SetValue, Call, InheritBaseValues)
    /// </summary>
    public static class At
    {
        public static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        //reflection call
        public static object Call(object obj, string method, params object[] args)
        {
            object ret = null;
            try
            {
                var methodInfo = obj.GetType().GetMethod(method, flags);
                ret = methodInfo.Invoke(obj, args);
            }
            catch
            {
                UnityEngine.Debug.Log($"Exception getting method '{method}'!");
            }

            return ret;
        }

        // set property
        public static void SetProp(object value, Type type, object obj, string property)
        {
            PropertyInfo propInfo = type.GetProperty(property);
            if (propInfo != null && propInfo.CanWrite)
            {
                try
                {
                    propInfo.SetValue(obj, value, flags, null, null, null);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("Exception setting property " + property + ".\r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace);
                }
            }
        }

        // set value
        public static void SetValue<T>(T value, Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        // get value
        public static object GetValue(Type type, object obj, string value)
        {
            FieldInfo fieldInfo = type.GetField(value, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        // inherit base values
        public static void InheritBaseValues(object _derived, object _base)
        {
            foreach (FieldInfo fi in _base.GetType().GetFields(flags))
            {
                try { _derived.GetType().GetField(fi.Name).SetValue(_derived, fi.GetValue(_base)); } catch { }
            }

            return;
        }
    }
}
