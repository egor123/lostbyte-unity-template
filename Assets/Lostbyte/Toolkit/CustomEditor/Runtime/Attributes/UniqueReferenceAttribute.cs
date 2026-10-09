using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif

namespace Lostbyte.Toolkit.CustomEditor
{
    public class UniqueReferenceAttribute : CombinedAttribute
    {
        private readonly Type _type;
        public UniqueReferenceAttribute(Type type) => _type = type;
        public UniqueReferenceAttribute() { }

#if UNITY_EDITOR

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = new(position)
            {
                x = position.x + EditorGUIUtility.labelWidth,
                width = position.width - EditorGUIUtility.labelWidth,
                height = EditorGUIUtility.singleLineHeight
            };

            Type baseType = _type ?? GetManagedReferenceFieldType(property);

            DrawDropDown(rect, property, label, baseType);
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override bool DrawDefaultPropertyField() => false;
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, true);

        public static void DrawDropDown(Rect position, SerializedProperty property, GUIContent label, Type type, Func<Type, bool> condition = null)
        {
            if (type == null) return;

            bool cacheNeedsUpdate = false;
            foreach (var target in property.serializedObject.targetObjects)
            {
                if (SerializationUtility.HasManagedReferencesWithMissingTypes(target))
                {
                    SerializationUtility.ClearAllManagedReferencesWithMissingTypes(target);
                    cacheNeedsUpdate = true;
                }
            }
            if (cacheNeedsUpdate)
            {
                property.serializedObject.Update();
            }
            
            var value = property.managedReferenceValue;

            var validTypes = TypeCache.GetTypesDerivedFrom(type)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericType && (condition == null || condition(t)))
                .ToArray();

            var list = validTypes.Select(GetName).ToList();
            list.Insert(0, "Null");

            var targetObjects = property.serializedObject.targetObjects;
            var propertyPath = property.propertyPath;

            int index = Math.Max(0, list.IndexOf(GetName(value?.GetType())));

            if (GUI.Button(position, list[index].Split('/').Last(), EditorStyles.popup))
            {
                var provider = ScriptableObject.CreateInstance<StringListProvider>();
                provider.List = list.ToArray();

                provider.Types = new Type[validTypes.Length + 1];
                provider.Types[0] = null;
                for (int i = 0; i < validTypes.Length; i++) provider.Types[i + 1] = validTypes[i];

                provider.Callback = selectedType =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        foreach (var target in targetObjects)
                        {
                            var singleSo = new SerializedObject(target);
                            var singleProp = singleSo.FindProperty(propertyPath);

                            if (singleProp != null)
                            {
                                singleProp.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
                                singleSo.ApplyModifiedProperties();
                            }
                        }
                    };
                };

                var pos = GUIUtility.GUIToScreenPoint(position.position) + new Vector2(position.width / 2, EditorGUIUtility.singleLineHeight * 2 - 2);
                SearchWindow.Open(new SearchWindowContext(pos, requestedWidth: position.width), provider);
            }
        }

        private static Type GetManagedReferenceFieldType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            var parts = typeName.Split(' ');
            if (parts.Length == 2)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == parts[0]);
                if (assembly != null) return assembly.GetType(parts[1]);
            }
            return null;
        }

        private static string GetName(Type type)
        {
            if (type == null) return "Null";
            var name = type.ToString().Split('+', '.')[^1];
            name = ObjectNames.NicifyVariableName(name);
            if (type.GetCustomAttributes(typeof(TagAttribute), true).FirstOrDefault() is TagAttribute path)
                name = $"{path.Tag}/{name}";
            return name;
        }

        public class StringListProvider : ScriptableObject, ISearchWindowProvider
        {
            public string[] List;
            public Type[] Types;
            public Action<Type> Callback;

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                List<SearchTreeEntry> list = new() { new SearchTreeGroupEntry(new GUIContent("Search"), 0) };
                List<string> groups = new();

                List.Select((s, i) => new KeyValuePair<int, string>(i, s))
                    .OrderBy(p => p.Value == "Null" ? "a" : p.Value)
                    .ToList()
                    .ForEach(p =>
                    {
                        var path = p.Value.Split('/');
                        var group = "";
                        for (int i = 0; i < path.Length - 1; i++)
                        {
                            group += path[i];
                            if (!groups.Contains(group))
                            {
                                groups.Add(group);
                                list.Add(new SearchTreeGroupEntry(new GUIContent(path[i]), i + 1));
                            }
                            group += '/';
                        }

                        var entry = new SearchTreeEntry(new GUIContent(p.Value.Split('/').Last()))
                        {
                            level = path.Length,
                            userData = Types[p.Key]
                        };
                        list.Add(entry);
                    });
                return list;
            }

            public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
            {
                Callback?.Invoke(SearchTreeEntry.userData as Type);
                return true;
            }
        }
#endif
    }
}