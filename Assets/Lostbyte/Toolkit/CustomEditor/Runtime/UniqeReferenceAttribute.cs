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
    public class UniqeReferenceAttribute : CombinedAttribute
    {
        private readonly Type _type;
        public UniqeReferenceAttribute(Type type) => _type = type;
        public UniqeReferenceAttribute() { }

#if UNITY_EDITOR
        // private int _selectedItem = 0;
        private static int _domainId = -1;
        public static Dictionary<Type, Type[]> _typeDictionary = new();
        public static Type[] GetSubClasses(Type type)
        {
            if (_domainId != AppDomain.CurrentDomain.Id)
            {
                _domainId = AppDomain.CurrentDomain.Id;
                _typeDictionary = new Dictionary<Type, Type[]>();
            }
            if (!_typeDictionary.TryGetValue(type, out var types))
            {
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface || t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type) //FIXME? IsSubclassOf or IsAssignableFrom
                    .Where(t => !t.ContainsGenericParameters)
                    .ToArray();
                _typeDictionary.Add(type, types);
            }
            return types;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = new(position)
            {
                x = position.x + EditorGUIUtility.labelWidth,
                width = position.width - EditorGUIUtility.labelWidth,
                height = EditorGUIUtility.singleLineHeight
            };
            DrawDropDown(rect, property, label, _type ?? property.GetTargetType());
            EditorGUI.PropertyField(position, property, label, true);
        }
        public override bool DrawDefaultPropertyField() => false;
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, true);

        public static void DrawDropDown(Rect position, SerializedProperty property, GUIContent label, Type type, Func<Type, bool> condition = null)
        {
            if (SerializationUtility.HasManagedReferencesWithMissingTypes(property.serializedObject.targetObject))
                SerializationUtility.ClearAllManagedReferencesWithMissingTypes(property.serializedObject.targetObject);

            var value = property.managedReferenceValue;
            var subClasses = GetSubClasses(type);
            var list = subClasses.Where((t) => condition == null || condition.Invoke(t)).Select(GetName).ToList();
            list.Insert(0, "Null");

            int index = Math.Max(0, list.IndexOf(GetName(value?.GetType())));

            if (GUI.Button(position, list[index].Split('/').Last(), EditorStyles.popup))
            {
                var provider = ScriptableObject.CreateInstance<StringListProvider>();
                provider.List = list.ToArray();
                provider.Callback = i =>
                {
                    property.managedReferenceValue = i == 0 ? null : Activator.CreateInstance(subClasses[i - 1]);
                    property.serializedObject.ApplyModifiedProperties();
                };
                var pos = GUIUtility.GUIToScreenPoint(position.position) + new Vector2(position.width / 2, EditorGUIUtility.singleLineHeight * 2 - 2);
                SearchWindow.Open(new SearchWindowContext(pos, requestedWidth: position.width), provider);
            }
        }
        private static string GetName(Type type)
        {
            if (type == null) return "Null";
            var name = type.ToString().Split('+', '.')[^1];
            if (type.GetCustomAttributes(typeof(TagAttribute), true).FirstOrDefault() is TagAttribute path) name = $"{path.Tag}/{name}";
            return name;
        }
        public class StringListProvider : ScriptableObject, ISearchWindowProvider
        {
            public string[] List;
            public Action<int> Callback;
            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                List<SearchTreeEntry> list = new() { new SearchTreeGroupEntry(new GUIContent("Search"), 0) };
                List<string> groups = new();
                List.Select((s, i) => new KeyValuePair<int, string>(i, s))
                    .OrderBy(p =>
                    {
                        if (p.Value == "Null") return "a";
                        return p.Value;
                    })
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
                            userData = p.Key //TODO use type
                        };
                        list.Add(entry);
                    });
                return list;
            }

            public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
            {
                Callback.Invoke((int)SearchTreeEntry.userData);
                return true;
            }
        }
#endif
    }
}