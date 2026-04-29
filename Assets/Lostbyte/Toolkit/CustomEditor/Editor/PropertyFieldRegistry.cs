using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lostbyte.Toolkit.Common;
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

            TypeCache.GetTypesWithAttribute<CustomFieldAttribute>()
                .Where(t => typeof(VisualElement).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ForEach(t =>
                {
                    var attr = t.GetCustomAttribute<CustomFieldAttribute>();
                    var ctor = t.GetConstructor(new[] { typeof(string) });
                    if (ctor == null) Debug.LogWarning($"{t.Name} missing (string label) constructor");
                    else Register(attr.TargetType, (fieldType, label) => (BindableElement)Activator.CreateInstance(t, label));
                });
        }
    }
}