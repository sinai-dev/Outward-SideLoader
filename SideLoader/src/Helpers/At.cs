using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BF = System.Reflection.BindingFlags;

namespace SideLoader
{
    public static class At
    {
        public static readonly BF FLAGS = BF.Public | BF.Instance | BF.NonPublic | BF.Static;

        // ============ Main public API ============

        /// <summary>Helper to set a static or instance value on a non-static class. Use SetFieldStatic for Static Classes.</summary>
        /// <typeparam name="T">The declaring class with the field in it.</typeparam>
        /// <param name="value">The value you want to set.</param>
        /// <param name="fieldName">The name of the field you want to set.</param>
        /// <param name="instance">The instance to use, or null for static members. Can be used to implicitly declare T if not null.</param>
        public static void SetField<T>(object value, string fieldName, T instance)
            => Internal_SetField(GetFieldInfo(typeof(T), fieldName), instance, value);

        /// <summary>Helper to set a value on a Static Class (not just a static member of a class, use SetField&lt;T&gt; for that).</summary>
        /// <param name="value">The value you want to set.</param>
        /// <param name="type">The declaring class with the field in it.</param>
        /// <param name="fieldName">The name of the field you want to set.</param>
        public static void SetFieldStatic(object value, Type type, string fieldName)
            => Internal_SetField(GetFieldInfo(type, fieldName), null, value);

        internal static void Internal_SetField(FieldInfo fi, object instance, object value)
        {
            if (fi == null)
                return;

            if (fi.IsStatic)
                fi.SetValue(null, value);
            else
                fi.SetValue(instance, value);
        }

        /// <summary>Helper to get a static or instance value on a non-static class. Use GetFieldStatic for Static Classes.</summary>
        /// <typeparam name="T">The declaring class with the field in it.</typeparam>
        /// <param name="fieldName">The name of the field you want to get.</param>
        /// <param name="instance">The instance to use, or null for static members. Can be used to implicitly declare T if not null.</param>
        public static object GetField<T>(string fieldName, T instance)
            => Internal_GetField(GetFieldInfo(typeof(T), fieldName), instance);

        /// <summary>Helper to get a value on a Static Class (not just a static member of a class, use GetField&lt;T&gt; for that).</summary>
        /// <param name="type">The declaring class with the field in it.</param>
        /// <param name="fieldName">The name of the field you want to get.</param>
        public static object GetFieldStatic(Type type, string fieldName)
            => Internal_GetField(GetFieldInfo(type, fieldName), null);

        internal static object Internal_GetField(FieldInfo fi, object instance)
        {
            if (fi == null)
                return null;
            if (fi.IsStatic)
                return fi.GetValue(null);
            else
                return fi.GetValue(instance);
        }

        /// <summary>Helper to set a static or instance value on a non-static class. Use SetPropertyStatic for Static Classes.</summary>
        /// <typeparam name="T">The declaring class with the property in it.</typeparam>
        /// <param name="value">The value you want to set.</param>
        /// <param name="propertyName">The name of the property you want to set.</param>
        /// <param name="instance">The instance to use, or null for static members. Can be used to implicitly declare T if not null.</param>
        public static void SetProperty<T>(object value, string propertyName, T instance)
            => Internal_SetProperty(GetPropertyInfo(typeof(T), propertyName), value, instance);

        /// <summary>Helper to set a value on a Static Class (not just a static member of a class, use SetProperty&lt;T&gt; for that).</summary>
        /// <param name="value">The value you want to set.</param>
        /// <param name="type">The declaring class with the property in it.</param>
        /// <param name="propertyName">The name of the property you want to set.</param>
        public static void SetPropertyStatic(object value, Type type, string propertyName)
            => Internal_SetProperty(GetPropertyInfo(type, propertyName), value, null);

        internal static void Internal_SetProperty(PropertyInfo pi, object value, object instance)
        {
            if (pi == null || !pi.CanWrite)
                return;

            var setter = pi.GetSetMethod(true);
            if (setter.IsStatic)
                setter.Invoke(null, new object[] { value });
            else
                setter.Invoke(instance, new object[] { value });
        }

        /// <summary>Helper to get a static or instance value on a non-static class. Use GetPropertyStatic for Static Classes.</summary>
        /// <typeparam name="T">The declaring class with the property in it.</typeparam>
        /// <param name="propertyName">The name of the property you want to get.</param>
        /// <param name="instance">The instance to use, or null for static members. Can be used to implicitly declare T if not null.</param>
        public static object GetProperty<T>(string propertyName, T instance)
            => Internal_GetProperty(GetPropertyInfo(typeof(T), propertyName), instance);

        /// <summary>Helper to get a value on a Static Class (not just a static member of a class, use GetProperty&lt;T&gt; for that).</summary>
        /// <param name="type">The declaring class with the property in it.</param>
        /// <param name="propertyName">The name of the property you want to get.</param>
        public static object GetPropertyStatic(Type type, string propertyName)
            => Internal_GetProperty(GetPropertyInfo(type, propertyName), null);

        internal static object Internal_GetProperty(PropertyInfo pi, object instance)
        {
            if (pi == null || !pi.CanRead)
                return null;

            var getter = pi.GetGetMethod(true);
            if (getter.IsStatic)
                return getter.Invoke(null, null);
            else
                return getter.Invoke(instance, null);
        }

        /// <summary>Helper to call a static or instance method on a non-static class. Use CallStatic for Static Classes.</summary>
        /// <typeparam name="T">The declaring class with the method in it.</typeparam>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="instance">The instance to invoke on, or null for static methods.</param>
        /// <param name="argumentTypes">Optional, for ambiguous methods you can provide an array corresponding to the Types of the arguments.</param>
        /// <param name="args">The arguments you want to provide for invocation.</param>
        /// <returns>The return value of the method.</returns>
        public static object Call<T>(string methodName, T instance, Type[] argumentTypes = null, params object[] args)
            => Internal_Call(GetMethodInfo(typeof(T), methodName, argumentTypes), instance, args);

        /// <summary>Helper to call a method on a Static Class (not just a static member of a class, use Call&lt;T&gt; for that).</summary>
        /// <param name="type">The declaring class with the method in it.</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="argumentTypes">Optional, for ambiguous methods you can provide an array corresponding to the Types of the arguments.</param>
        /// <param name="args">The arguments you want to provide for invocation.</param>
        /// <returns>The return value of the method.</returns>
        public static object CallStatic(Type type, string methodName, Type[] argumentTypes, params object[] args)
            => Internal_Call(GetMethodInfo(type, methodName, argumentTypes), null, args);

        internal static object Internal_Call(MethodInfo mi, object instance, params object[] args)
        {
            if (mi == null)
                return null;

            try
            {
                if (mi.IsStatic)
                    return mi.Invoke(null, args);
                else
                    return mi.Invoke(instance, args);
            }
            catch (Exception e)
            {
                SL.LogWarning("Exception invoking method: " + mi.ToString());
                SL.LogInnerException(e);
                return null;
            }
        }

        // ========= These methods are used to cache all MemberInfos used by this class =========
        // Can also be used publicly if anyone should want to.

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

        public static MethodInfo GetMethodInfo(Type type, string methodName, Type[] argumentTypes)
        {
            if (!s_cachedMethodInfos.ContainsKey(type))
                s_cachedMethodInfos.Add(type, new Dictionary<string, MethodInfo>());

            var key = methodName;

            if (argumentTypes != null)
            {
                key += "(";
                for (int i = 0; i < argumentTypes.Length; i++)
                {
                    if (i > 0)
                        key += ",";
                    key += argumentTypes[i].FullName;
                }
                key += ")";
            }

            try
            {
                if (!s_cachedMethodInfos[type].ContainsKey(key))
                {
                    if (argumentTypes != null)
                        s_cachedMethodInfos[type].Add(key, type.GetMethod(methodName, FLAGS, null, argumentTypes, null));
                    else
                        s_cachedMethodInfos[type].Add(key, type.GetMethod(methodName, FLAGS));
                }

                return s_cachedMethodInfos[type][key];
            }
            catch (AmbiguousMatchException)
            {
                SL.LogWarning("AmbiguousMatchException trying to get method name '" + methodName + "'");
                return null;
            }
        }

        // ============ MISC TOOLS ============

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

        // ~~~~~~~~~~~~~~~~~ Deprecated ~~~~~~~~~~~~~~~~~ //

        [Obsolete("Use SetField<T> or SetFieldStatic.")]
        public static void SetValue<T>(T value, Type type, object obj, string field)
            => throw new MissingMethodException("Deprecated API");

        [Obsolete("Use GetField<T> or GetFieldStatic.")]
        public static object GetValue(Type type, object obj, string field)
            => throw new MissingMethodException("Deprecated API");

        [Obsolete("Use Call<T> or CallStatic.")]
        public static object Call(Type type, object obj, string method, Type[] argumentTypes, params object[] args)
            => throw new MissingMethodException("Deprecated API");

        [Obsolete("Use SetProperty<T> or SetPropertyStatic.")]
        public static void SetProp<T>(T value, Type type, object obj, string property)
            => throw new MissingMethodException("Deprecated API");

        [Obsolete("Use GetProperty<T> or GetPropertyStatic.")]
        public static object GetProp(Type type, object obj, string property)
            => throw new MissingMethodException("Deprecated API");
    }
}
