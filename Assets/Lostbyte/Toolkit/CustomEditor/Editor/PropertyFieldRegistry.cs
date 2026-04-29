using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    public static class PropertyFieldRegistry
    {
        private static readonly Dictionary<Type, Func<Type, string, BindableElement>> _factories = new();
        private static bool _initialized;

        public static void Register(Type targetType, Func<Type, string, BindableElement> factory)
        {
            _factories[targetType] = factory;
        }

        public static bool TryCreate(Type type, string label, out BindableElement element)
        {
            TryInit();
            if (_factories.TryGetValue(type, out var factory))
            {
                element = factory(type, label);
                return true;
            }

            foreach (var kv in _factories)
            {
                if (kv.Key.IsAssignableFrom(type))
                {
                    Debug.Log(type);

                    element = kv.Value(type, label);
                    return true;
                }
            }

            element = null;
            return false;
        }

        public static void Clear() => _factories.Clear();

        public static void TryInit()
        {
            if (_initialized) return;
            _initialized = true;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t =>
                    typeof(VisualElement).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface);

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CustomFieldAttribute>();
                if (attr == null) continue;

                var ctor = type.GetConstructor(new[] { typeof(string) });
                if (ctor == null)
                {
                    Debug.LogWarning($"{type.Name} missing (string label) constructor");
                    continue;
                }

                Register(attr.TargetType, (fieldType, label) => (BindableElement)Activator.CreateInstance(type, label));
            }
        }
    }
}