using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace Lostbyte.Toolkit.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class ClearStaticAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatics()
        {
            foreach (FieldInfo field in TypeCache.GetFieldsWithAttribute<ClearStaticAttribute>())
            {
                if (!field.IsStatic)
                {
                    Print.MWarn($"Ignored field '{field.Name}' in '{field.DeclaringType.Name}'. The attribute should only be used on static fields.");
                    continue;
                }
                if (field.IsLiteral)
                {
                    continue;
                }
                try
                {
                    Type declaringType = field.DeclaringType;
                    if (declaringType?.IsGenericTypeDefinition ?? false)
                    {
                        TypeCache.GetTypesDerivedFrom(declaringType)
                            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                            .Select(t => GetConstructedGenericBaseType(t, declaringType))
                            .Select(t => t?.GetField(field.Name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                            .WhereNotNull()
                            .ForEach(ClearFieldValue);
                    }
                    else
                    {
                        ClearFieldValue(field);
                    }
                }
                catch (Exception e)
                {
                    Print.MError($"Failed to clear field '{field.Name}' in '{field.DeclaringType?.Name}': {e.Message}");
                }
            }
        }
        private static void ClearFieldValue(FieldInfo field)
        {
            var currentValue = field.GetValue(null);
            if (currentValue != null)
            {
                var clearMethod = field.FieldType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(currentValue, null);
                    return;
                }
            }
            Type fieldType = field.FieldType;
            object defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;

            if (field.IsInitOnly && defaultValue == null)
            {
                Print.MWarn($"Cleared readonly field '{field.Name}' in '{field.DeclaringType?.Name}' to null. It will not be re-instantiated automatically.");
            }
            field.SetValue(null, defaultValue);
        }

        private static Type GetConstructedGenericBaseType(Type type, Type genericTypeDef)
        {
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == genericTypeDef)
                    return currentType;
                currentType = currentType.BaseType;
            }
            return null;
        }
#endif
    }
}
