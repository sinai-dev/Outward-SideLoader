using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SideLoader
{
    /// <summary>At (AccessTools) is a light-weight Reflection helper.</summary>
    public static class At
    {
        /// <summary>
        /// Common BindingFlags, and the only ones that are really needed for Outward.
        /// </summary>
        public static readonly BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        internal static Dictionary<Type, Dictionary<string, FieldInfo>> s_cachedFieldInfos = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            if (!s_cachedFieldInfos.ContainsKey(type))
                s_cachedFieldInfos.Add(type, new Dictionary<string, FieldInfo>());

            if (!s_cachedFieldInfos[type].ContainsKey(fieldName))
                s_cachedFieldInfos[type].Add(fieldName, type.GetField(fieldName, FLAGS));

            return s_cachedFieldInfos[type][fieldName];
        }

        internal static Dictionary<Type, Dictionary<string, PropertyInfo>> s_cachedPropInfos = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            if (!s_cachedPropInfos.ContainsKey(type))
                s_cachedPropInfos.Add(type, new Dictionary<string, PropertyInfo>());

            if (!s_cachedPropInfos[type].ContainsKey(propertyName))
                s_cachedPropInfos[type].Add(propertyName, type.GetProperty(propertyName, FLAGS));

            return s_cachedPropInfos[type][propertyName];
        }

        internal static Dictionary<Type, Dictionary<string, MethodInfo>> s_cachedMethodInfos = new Dictionary<Type, Dictionary<string, MethodInfo>>();

        public static MethodInfo GetMethodInfo(Type type, string methodName, Type[] argumentTypes = null)
        {
            if (!s_cachedMethodInfos.ContainsKey(type))
                s_cachedMethodInfos.Add(type, new Dictionary<string, MethodInfo>());

            var key = methodName;

            if (argumentTypes != null)
                foreach (var argType in argumentTypes)
                    key += argType.FullName;

            if (!s_cachedMethodInfos[type].ContainsKey(key))
            {
                if (argumentTypes != null)
                    s_cachedMethodInfos[type].Add(key, type.GetMethod(methodName, FLAGS, null, argumentTypes, null));
                else
                    s_cachedMethodInfos[type].Add(key, type.GetMethod(methodName, FLAGS));
            }

            return s_cachedMethodInfos[type][key];
        }

        /// <summary>
        /// Helper to set the value to a private or protected field.
        /// </summary>
        /// <param name="value">The value you want to set.</param>
        /// <param name="type">The declaring class Type which contains this field.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>
        /// <param name="field">The name of the field.</param>
        public static void SetValue(object value, Type type, object obj, string field)
        {
            GetFieldInfo(type, field)?.SetValue(obj, value);
        }

        /// <summary>
        /// Helper to set the value to a private or protected field.
        /// </summary>
        /// <typeparam name="T">The declaring class Type which contains this field.</typeparam>
        /// <param name="value">The value you want to set.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="instance">The instance, for non-static members. If the member is static, use "null".</param>
        public static void SetValue<T>(object value, string fieldName, T instance)
        {
            GetFieldInfo(typeof(T), fieldName)?.SetValue(instance, value);
        }

        /// <summary>
        /// Helper to get the value from a private or protected field.
        /// </summary>
        /// <param name="type">The declaring class Type which contains this field.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>
        /// <param name="field">The name of the field you want to get.</param>
        /// <returns>The value of the field provided, if valid.</returns>
        public static object GetValue(Type type, object obj, string field)
        {
            return GetFieldInfo(type, field)?.GetValue(obj);
        }

        /// <summary>
        /// Helper to set the value to a private or protected field.
        /// </summary>
        /// <typeparam name="T">The declaring class Type which contains this field.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="instance">The instance, for non-static members. If the member is static, use "null".</param>
        /// <returns>The value of the field provided, if valid.</returns>
        public static object GetValue<T>(string fieldName, T instance)
        {
            return GetFieldInfo(typeof(T), fieldName)?.GetValue(instance);
        }

        ///// <summary>
        ///// Helper to get the value from a private or protected field.
        ///// </summary>
        ///// <param name="type">The declaring class Type which contains this field.</param>
        ///// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>
        ///// <param name="field">The name of the field you want to get.</param>
        ///// <returns>The value of the field provided, if valid.</returns>
        //public static object GetValue(Type type, object obj, string field) => GetFieldInfo(type, field)?.GetValue(obj);

        /// <summary>
        /// Helper to invoke a private or protected method.
        /// </summary>
        /// <param name="type">The declaring class Type which contains this method.</param>
        /// <param name="obj">The instance to invoke the method from. If method is static, use "null".</param>
        /// <param name="method">The name of the method you want to invoke.</param>
        /// <param name="argumentTypes">[Optional] For ambiguous methods, provide an array corresponding to the Type of each argument, otherwise use "null".</param>
        /// <param name="args">The actual arguments you want to provide for the method, if any.</param>
        /// <returns>The return value of the method (might be void).</returns>
        public static object Call(Type type, object obj, string method, Type[] argumentTypes, params object[] args)
        {
            try
            {
                var methodInfo = GetMethodInfo(type, method, argumentTypes);
                return methodInfo.Invoke(obj, args);
            }
            catch (AmbiguousMatchException)
            {
                SL.Log("Ambiguous match exception on method " + method + "!");
            }
            catch (Exception e)
            {
                SL.LogInnerException(e);
            }
            return null;
        }

        /// <summary>
        /// Generic helper to invoke a private or protected method.
        /// </summary>
        /// <param name="instance">The instance to invoke the method from. If method is static, use "null".</param>
        /// <param name="method">The name of the method you want to invoke.</param>
        /// <param name="argumentTypes">[Optional] For ambiguous methods, provide an array corresponding to the Type of each argument, otherwise use "null".</param>
        /// <param name="args">The actual arguments you want to provide for the method, if any.</param>
        /// <returns>The return value of the method (might be void).</returns>
        public static object Call<T>(T instance, string method, Type[] argumentTypes = null, params object[] args)
        {
            try
            {
                var methodInfo = GetMethodInfo(typeof(T), method, argumentTypes);
                return methodInfo.Invoke(instance, args);
            }
            catch (AmbiguousMatchException)
            {
                SL.Log("Ambiguous match exception on method " + method + "!");
            }
            catch (Exception e)
            {
                SL.LogInnerException(e);
            }
            return null;
        }

        /// <summary>
        /// Helper to set a private property, if possible to set.
        /// </summary>
        /// <param name="value">The value you want to set.</param>
        /// <param name="type">The declaring class Type which contains this property.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>>
        /// <param name="property">The name of the property you want to set.</param>
        public static void SetProp(object value, Type type, object obj, string property)
        {
            var propInfo = GetPropertyInfo(type, property);
            if (propInfo != null && propInfo.CanWrite)
            {
                try
                {
                    propInfo.SetValue(obj, value, FLAGS, null, null, null);
                }
                catch (Exception e)
                {
                    SL.LogInnerException(e);
                }
            }
        }

        /// <summary>
        /// Helper to set a private property, if possible to set.
        /// </summary>
        /// <param name="value">The value you want to set.</param>
        /// <param name="instance">The instance, for non-static members. If the member is static, use "null".</param>>
        /// <param name="property">The name of the property you want to set.</param>
        public static void SetProp<T>(object value, string property, T instance)
        {
            var propInfo = GetPropertyInfo(typeof(T), property);
            if (propInfo != null && propInfo.CanWrite)
            {
                try
                {
                    propInfo.SetValue(instance, value, FLAGS, null, null, null);
                }
                catch (Exception e)
                {
                    SL.LogInnerException(e);
                }
            }
        }

        /// <summary>
        /// Helper to get the value from a private property.
        /// </summary>
        /// <param name="type">The declaring class Type which contains this property.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>>
        /// <param name="property">The name of the property you want to get.</param>
        /// <returns>The value from the property.</returns>
        public static object GetProp(Type type, object obj, string property)
        {
            var propInfo = GetPropertyInfo(type, property);
            if (propInfo != null && propInfo.CanRead)
            {
                try
                {
                    return propInfo.GetValue(obj, FLAGS, null, null, null);
                }
                catch (Exception e)
                {
                    SL.LogInnerException(e);
                }
            }
            return null;
        }

        /// <summary>
        /// Helper to get the value from a private property.
        /// </summary>
        /// <param name="instance">The instance, for non-static members. If the member is static, use "null".</param>>
        /// <param name="property">The name of the property you want to get.</param>
        /// <returns>The value from the property.</returns>
        public static object GetProp<T>(string property, T instance)
        {
            var propInfo = GetPropertyInfo(typeof(T), property);
            if (propInfo != null && propInfo.CanRead)
            {
                try
                {
                    return propInfo.GetValue(instance, FLAGS, null, null, null);
                }
                catch (Exception e)
                {
                    SL.LogInnerException(e);
                }
            }
            return null;
        }

        /// <summary>
        /// A helper to get all the fields from one class instance, and set them to another.
        /// </summary>
        /// <param name="copyTo">The object which you are setting values to.</param>
        /// <param name="copyFrom">The object which you are getting values from.</param>
        /// <param name="declaringType">Optional, manually define the declaring class type.</param>
        /// <param name="recursive">Whether to recursively dive into the BaseTypes and copy those fields too</param>
        public static void CopyFields(object copyTo, object copyFrom, Type declaringType = null, bool recursive = false)
        {
            var type = declaringType ?? copyFrom.GetType();

            if (type.IsAssignableFrom(copyTo.GetType()) && type.IsAssignableFrom(copyFrom.GetType()))
            {
                foreach (FieldInfo fi in type.GetFields(FLAGS))
                {
                    try
                    {
                        fi.SetValue(copyTo, fi.GetValue(copyFrom));
                    }
                    catch { }
                }
            }

            if (recursive && type.BaseType is Type baseType)
            {
                // We don't want to copy Unity low-level types, such as MonoBehaviour or Component.
                // Copying these fields causes serious errors.
                if (baseType != typeof(MonoBehaviour) && baseType != typeof(Component))
                {
                    CopyFields(copyTo, copyFrom, type.BaseType, true);
                }
            }

            return;
        }

        /// <summary>
        /// A helper to get all the properties from one class instance, and set them to another.
        /// </summary>
        /// <param name="copyTo">The object which you are setting values to.</param>
        /// <param name="copyFrom">The object which you are getting values from.</param>
        /// <param name="declaringType">Optional, manually define the declaring class type.</param>
        /// <param name="recursive">Whether to recursively dive into the BaseTypes and copy those properties too</param>
        public static void CopyProperties(object copyTo, object copyFrom, Type declaringType = null, bool recursive = false)
        {
            var type = declaringType ?? copyFrom.GetType();

            if (type.IsAssignableFrom(copyTo.GetType()) && type.IsAssignableFrom(copyFrom.GetType()))
            {
                foreach (var pi in type.GetProperties(FLAGS).Where(x => x.CanWrite))
                {
                    try
                    {
                        pi.SetValue(copyTo, pi.GetValue(copyFrom, null), null);
                    }
                    catch { }
                }
            }

            if (recursive && type.BaseType is Type baseType)
            {
                // We don't want to copy Unity low-level types, such as MonoBehaviour or Component.
                // Copying these fields causes serious errors.
                if (baseType != typeof(MonoBehaviour) && baseType != typeof(Component))
                {
                    CopyProperties(copyTo, copyFrom, type.BaseType, true);
                }
            }
        }
    }
}
