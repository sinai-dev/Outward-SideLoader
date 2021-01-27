using System;
using System.Collections.Generic;
using UnityEngine;

namespace SideLoader.Helpers
{
    public static class UnityHelpers
    {
        public static bool IsNullOrDestroyed(this object obj, bool _ = false)
        {
            if (obj is UnityEngine.Object uObj)
                return !uObj;
            else
                return obj == null;
        }

        /// <summary> Small helper for destroying all children on a given Transform 't'. Uses DestroyImmediate(). </summary>
        /// <param name="t">The transform whose children you want to destroy.</param>
        /// <param name="destroyContent">If true, will destroy children called "Content" (used for Bags)</param>
        /// <param name="destroyActivator">If true, will destroy children called "Activator" (used for Deployables / Traps)</param>
        public static void DestroyChildren(Transform t, bool destroyContent = false, bool destroyActivator = false)
        {
            DestroyChildren(t, destroyContent, destroyActivator, false);
        }

        /// <summary> Small helper for destroying all children on a given Transform 't'. Uses DestroyImmediate(). </summary>
        /// <param name="t">The transform whose children you want to destroy.</param>
        /// <param name="destroyContent">If true, will destroy children called "Content" (used for Bags)</param>
        /// <param name="destroyActivator">If true, will destroy children called "Activator" (used for Deployables / Traps)</param>
        /// <param name="destroyVFX">If true, will destroy children whose names begin with "VFX".</param>
        public static void DestroyChildren(Transform t, bool destroyContent, bool destroyActivator, bool destroyVFX)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if ((destroyContent || child.name != "Content")
                    && (destroyActivator || child.name != "Activator")
                    && (destroyVFX || !child.name.StartsWith("VFX")))
                {
                    list.Add(child.gameObject);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                GameObject.DestroyImmediate(list[i]);
            }
        }

        /// <summary>
        /// Replaces existingComponent type with desiredType ONLY if desiredType is not assignable from the existingComponent type.
        /// That means if desiredType is Item and existingComponent type is Weapon, this will do nothing.
        /// If both types are the same, this will do nothing.
        /// Otherwise, this will replace existingComponent with a desiredType component and inherit all possible values.
        /// </summary>
        /// <param name="desiredType">The desired class type (the game type, not the SL type)</param>
        /// <param name="existingComponent">The existing component</param>
        /// <returns>The component left on the transform after the method runs.</returns>
        public static Component FixComponentType(Type desiredType, Component existingComponent)
        {
            if (!existingComponent || !existingComponent.transform || desiredType == null || desiredType.IsAbstract)
            {
                return existingComponent;
            }

            var currentType = existingComponent.GetType();

            // If currentType derives from desiredType (or they are the same type), do nothing
            // This is to allow using basic SL_Item (or whatever) templates on more complex types without replacing them.
            if (desiredType.IsAssignableFrom(currentType))
            {
                return existingComponent;
            }

            var newComp = existingComponent.gameObject.AddComponent(desiredType);

            while (!currentType.IsAssignableFrom(desiredType) && currentType.BaseType != null && currentType.BaseType != typeof(MonoBehaviour))
            {
                // Desired type does not derive from current type.
                // We need to recursively dive through currentType's BaseTypes until we find a type we can assign from.
                // Eg, current is MeleeWeapon and we want a ProjectileWeapon. We need to get the common base class (Weapon, in that case).
                // When currentType reaches Weapon, Weapon.IsAssignableFrom(ProjectileWeapon) will return true.
                // We also want to make sure we didnt reach MonoBehaviour, and at least got a game class.
                currentType = currentType.BaseType;
            }

            // Final check if the value copying is valid, after operations above.
            if (currentType.IsAssignableFrom(desiredType))
            {
                // recursively get all the values
                At.CopyProperties(newComp, existingComponent, currentType, true);
                At.CopyFields(newComp, existingComponent, currentType, true);
            }
            else
            {
                SL.Log($"FixComponentTypeIfNeeded - could not find a compatible type of {currentType.Name} which is assignable to desired type: {desiredType.Name}!");
            }

            // remove the old component
            GameObject.DestroyImmediate(existingComponent);

            return newComp;
        }

        /// <summary>
        /// Gets a copy of Component and adds it to the transform provided.
        /// </summary>
        /// <typeparam name="T">The Type of Component which will be added to the transform.</typeparam>
        /// <param name="component">The existing component to copy from (and the T if not directly supplied)</param>
        /// <param name="transform">The Transform to add to</param>
        /// <returns></returns>
        public static T GetCopyOf<T>(T component, Transform transform) where T : Component
        {
            var comp = transform.gameObject.AddComponent<T>();

            At.CopyProperties(comp, component, null, true);
            At.CopyFields(comp, component, null, true);

            return comp as T;
        }
    }
}
