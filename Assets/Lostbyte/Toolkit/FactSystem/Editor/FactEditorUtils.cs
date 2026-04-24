using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public static class FactEditorUtils
    {
        internal static FactSettings GetOrCreateSettings()
        {
            var settings = FactSettings.TryLoad();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<FactSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Resources/FactSettings.asset");
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        public static FactDatabase Database => GetOrCreateSettings().Database;

        public static bool ValidateIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("Name cannot be empty");
                return false;
            }
            var regex = new Regex(@"^[a-zA-Z0-9_-]+$");
            if (!regex.IsMatch(name))
            {
                Debug.LogWarning("Name must consist only from letters, numbers, dashes or underscores");
                return false;
            }
            if (!char.IsLetter(name[0]))
            {
                Debug.LogWarning("Name must start with letter");
                return false;
            }
            if (!char.IsLetterOrDigit(name[^1]))
            {
                Debug.LogWarning("Name must end with letter or digit");
                return false;
            }
            if (Database.RootKeys.SelectMany(k => k.Children).SelectMany(k => k.Children.Select(c => c.name).Concat(k.Facts.Select(f => f.name))).Any(s => s == name))
            {
                Debug.LogWarning("Name already exists");
                return false;
            }
            return true;
        }
        public static string GenerateValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "default_name";
            name = Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2");
            name = Regex.Replace(name, @"[\s\-]+", "_");
            name = name.ToLowerInvariant();
            name = new string(name
                .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
                .ToArray());
            if (name.Length == 0 || !char.IsLetter(name[0]))
                name = "f" + name;
            if (!char.IsLetterOrDigit(name[^1]))
                name += "0";
            if (name.Length > 64)
                name = name[..64];
            var usedNames = GetAllKeys()
                .Select(k => k.name)
                .Concat(Database.FactStorage.Select(f => f.name))
                .ToHashSet();
            string result = name;
            int suffix = 1;
            while (usedNames.Contains(result))
                result = name + "_" + suffix++;
            return result;
        }
        public static string GenerateGuid(string name)
        {
            string guid = GenerateValidIdentifier(name);
            HashSet<string> guids = GetAllKeys().Select(k => k.Guid)
                .Concat(Database.FactStorage.Select(f => f.Guid))
                .Concat(Database.EventStorage.Select(f => f.Guid))
                .ToHashSet();
            int i = 1;
            string result = guid;
            while (guids.Contains(guid))
                result = $"{guid}_{i++}";
            return result;
        }
        public static List<KeyContainer> GetAllKeys()
        {
            List<KeyContainer> all = new();
            void Recurse(KeyContainer key)
            {
                all.Add(key);
                foreach (var child in key.Children)
                    Recurse(child);
            }
            foreach (var root in Database.RootKeys)
                Recurse(root);
            return all;
        }
        public static KeyContainer GetKey(string name)
        {
            foreach (var key in GetAllKeys())
                if (key.name == name)
                    return key;
            return null;
        }
        public static FactDefinition GetFact(string name)
        {
            foreach (var fact in Database.FactStorage)
                if (fact.name == name)
                    return fact;
            return null;
        }
        public static void ShowAddNewKeyPopup(KeyContainer parentKey, string name = "", Vector2 pos = default, List<FactValueOverride> overrides = null, Action<KeyContainer> callback = null)
        {
            var popup = new PopupWindow((rect, window) =>
            {
                EditorGUILayout.LabelField("Enter Key Name");
                name = EditorGUILayout.TextField(name);

                if (GUILayout.Button("Create"))
                {
                    if (ValidateIdentifier(name))
                    {
                        var key = ScriptableObject.CreateInstance<KeyContainer>();
                        key.name = name;
                        key.Guid = GenerateGuid(name);
                        key.IsSerializable = true;
                        if (overrides != null)
                        {
                            foreach (var o in overrides)
                            {
                                if (o.Fact != null && o.Wrapper != null)
                                {
                                    key.ValueOverrides.Add(o.Copy());
                                    if (!key.Facts.Contains(o.Fact))
                                        key.Facts.Add(o.Fact);
                                }
                            }
                        }

                        AssetDatabase.AddObjectToAsset(key, Database);
                        AssetDatabase.SaveAssets();

                        if (parentKey == null)
                            Database.RootKeys.Add(key);
                        else
                        {
                            parentKey.Children.Add(key);
                            EditorUtility.SetDirty(parentKey);
                        }

                        EditorUtility.SetDirty(Database);
                        AssetDatabase.SaveAssets();
                        callback?.Invoke(key);
                    }
                    window.Close();
                }
            }, new(250, 70));
            UnityEditor.PopupWindow.Show(new Rect(Event.current?.mousePosition ?? pos, Vector2.zero), popup);
        }
        public static void ShowDublicateKeyPopup(KeyContainer originalKey, string name = "", Vector2 pos = default, Action<KeyContainer> callback = null)
        {
            var popup = new PopupWindow((rect, window) =>
            {
                EditorGUILayout.LabelField("Enter Key Name");
                name = EditorGUILayout.TextField(name);

                if (GUILayout.Button("Dublicate"))
                {
                    if (ValidateIdentifier(name))
                    {
                        var key = ScriptableObject.CreateInstance<KeyContainer>();
                        key.name = name;
                        key.IsSerializable = originalKey.IsSerializable;
                        key.Description = originalKey.Description;
                        if (originalKey.ValueOverrides != null)
                            foreach (var o in originalKey.ValueOverrides)
                                if (o.Fact != null && o.Wrapper != null)
                                    key.ValueOverrides.Add(o.Copy());
                        foreach (var fact in originalKey.Facts)
                            key.Facts.Add(fact);
                        AssetDatabase.AddObjectToAsset(key, Database);
                        AssetDatabase.SaveAssets();

                        KeyContainer parent = GetAllKeys().Find(k => k.Children.Contains(originalKey));
                        if (parent == null)
                            Database.RootKeys.Add(key);
                        else
                        {
                            parent.Children.Add(key);
                            EditorUtility.SetDirty(parent);
                        }

                        EditorUtility.SetDirty(Database);
                        AssetDatabase.SaveAssets();
                        callback?.Invoke(key);
                    }
                    window.Close();
                }
            }, new(250, 70));
            UnityEditor.PopupWindow.Show(new Rect(Event.current?.mousePosition ?? pos, Vector2.zero), popup);
        }
        public static void ShowAddNewFactPopup(KeyContainer parentKey, string name = "", bool IsSerializable = true, Vector2 pos = default, Action<FactDefinition> callback = null)
        {
            int type = 0;
            object value = null;
            List<string> enumValues = new();
            Vector2 enumScroll = Vector2.zero;
            Type[] types = { typeof(IntFactDefinition), typeof(FloatFactDefinition), typeof(BoolFactDefinition), typeof(StringFactDifenition), typeof(Vector2FactDifenition), typeof(Vector3FactDefinition), typeof(Vector4FactDefinition), typeof(ColorFactDefinition), typeof(EnumFactDefinition) };
            string[] names = { "int", "float", "bool", "string", "vector2", "vector3", "vector4", "color", "enum" };
            var popup = new PopupWindow(
                (rect, window) =>
                {
                    name = EditorGUILayout.TextField("Name", name);
                    type = EditorGUILayout.Popup("Type", type, names);
                    if (types[type] != typeof(EnumFactDefinition))
                    {
                        value = FieldFactory.FactValueField("Default Value", value, types[type].BaseType.GenericTypeArguments[0]);
                        GUILayout.Space(110);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Enum Values");

                        if (enumValues.Count == 0)
                            enumValues.Add("");

                        float scrollHeight = 100;

                        enumScroll = EditorGUILayout.BeginScrollView(
                            enumScroll,
                            GUILayout.Height(scrollHeight)
                        );

                        for (int i = 0; i < enumValues.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();

                            enumValues[i] = EditorGUILayout.TextField(enumValues[i]);

                            if (GUILayout.Button("-", GUILayout.Width(22)))
                            {
                                enumValues.RemoveAt(i);
                                break;
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndScrollView();

                        GUILayout.Space(4);

                        if (GUILayout.Button("+ Add Value"))
                        {
                            enumValues.Add("");
                        }
                    }

                    GUI.enabled = !string.IsNullOrWhiteSpace(name);
                    if (GUILayout.Button("Create"))
                    {
                        if (ValidateIdentifier(name))
                        {
                            var fact = ScriptableObject.CreateInstance(types[type]) as FactDefinition;
                            if (fact is EnumFactDefinition eFact)
                            {
                                enumValues = enumValues
                                    .Where(v => !string.IsNullOrWhiteSpace(v))
                                    .Distinct()
                                    .ToList();
                                enumValues.ForEach(v => eFact.Values.Add(v));
                            }
                            else
                            {
                                fact.DefaultValueRaw = value;
                            }
                            fact.name = name;
                            fact.Guid = GenerateGuid(name);
                            fact.IsSerializable = IsSerializable;
                            AssetDatabase.AddObjectToAsset(fact, Database);
                            AssetDatabase.SaveAssets();

                            if (parentKey != null)
                                parentKey.Facts.Add(fact);
                            Database.FactStorage.Add(fact);

                            EditorUtility.SetDirty(Database);
                            if (parentKey != null)
                                EditorUtility.SetDirty(parentKey);
                            AssetDatabase.SaveAssets();
                            callback?.Invoke(fact);

                        }
                        window.Close();
                    }
                    GUI.enabled = true;
                },
                new(300, 110 * 2)
            );
            UnityEditor.PopupWindow.Show(new Rect(Event.current?.mousePosition ?? pos, Vector2.zero), popup);
        }
        public static void ShowAddExistingFactPopup(KeyContainer parentKey, string name = "", Vector2 pos = default, Action<FactDefinition> callback = null)
        {
            var dropdown = new Dropdown<FactDefinition>(
                new AdvancedDropdownState(),
                Database.FactStorage.ToList(),
                fact => fact.name,
                fact => fact.GenericType.Name,
                fact =>
                {
                    parentKey.Facts.Add(fact);
                    EditorUtility.SetDirty(Database);
                    EditorUtility.SetDirty(parentKey);
                    AssetDatabase.SaveAssets();
                    callback?.Invoke(fact);
                });
            dropdown.Show(new Rect(Event.current?.mousePosition ?? pos, Vector2.zero));
        }

        public static void ShowAddNewEventPopup(KeyContainer parentKey, string name = "", bool IsSerializable = true, Vector2 pos = default, Action<EventDefinition> callback = null)
        {
            var popup = new PopupWindow(
                (rect, window) =>
                {
                    EditorGUILayout.LabelField("Enter Event Name");
                    name = EditorGUILayout.TextField(name);

                    if (GUILayout.Button("Create"))
                    {
                        if (ValidateIdentifier(name))
                        {
                            var @event = ScriptableObject.CreateInstance<EventDefinition>();
                            @event.name = name;
                            @event.Guid = GenerateGuid(name);
                            AssetDatabase.AddObjectToAsset(@event, Database);
                            AssetDatabase.SaveAssets();

                            if (parentKey != null)
                                parentKey.Events.Add(@event);
                            Database.EventStorage.Add(@event);

                            EditorUtility.SetDirty(Database);
                            if (parentKey != null)
                                EditorUtility.SetDirty(parentKey);
                            AssetDatabase.SaveAssets();
                            callback?.Invoke(@event);
                        }
                        window.Close();
                    }
                },
                new(250, 70)
            );
            UnityEditor.PopupWindow.Show(new Rect(Event.current?.mousePosition ?? pos, Vector2.zero), popup);
        }
        public static void ShowAddExistingEventPopup(KeyContainer parentKey, string name = "", Vector2 pos = default, Action<EventDefinition> callback = null)
        {
            var dropdown = new Dropdown<EventDefinition>(
                new AdvancedDropdownState(),
                Database.EventStorage.ToList(),
                evt => $"{evt.name}",
                evt => "Empty",
                evt =>
                {
                    parentKey.Events.Add(evt);
                    EditorUtility.SetDirty(Database);
                    EditorUtility.SetDirty(parentKey);
                    AssetDatabase.SaveAssets();
                    callback?.Invoke(evt);
                });
            dropdown.Show(new Rect(Event.current?.mousePosition ?? pos, Vector2.zero));
        }
        public static void ShowDeleteKeyModal(KeyContainer key, Action callback = null)
        {
            if (key == null) return;

            string title = "Delete Key";
            string message = $"Are you sure you want to delete the key \"{key.name}\"?\nThis action cannot be undone.";
            string ok = "Delete";
            string cancel = "Cancel";

            bool confirmed = EditorUtility.DisplayDialog(title, message, ok, cancel);
            if (confirmed)
            {
                DeleteKey(key);
                callback?.Invoke();
            }
        }
        public static void DeleteKey(KeyContainer key)
        {
            Database.RootKeys.Remove(key);
            foreach (var parent in GetAllKeys())
            {
                parent.Children.Remove(key);
                EditorUtility.SetDirty(parent);
            }
            foreach (var child in key.Children)
                DeleteKey(child);
            UnityEngine.Object.DestroyImmediate(key, true);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(Database);
            AssetDatabase.SaveAssets();
        }
        public static void ShowDeleteFactModal(FactDefinition fact, Action callback = null)
        {
            if (fact == null) return;

            string title = "Delete Key";
            string message = $"Are you sure you want to delete the \"{fact.name}\"?\nThis action cannot be undone.";
            string ok = "Delete";
            string cancel = "Cancel";

            bool confirmed = EditorUtility.DisplayDialog(title, message, ok, cancel);
            if (confirmed)
            {
                DeleteFact(fact);
                callback?.Invoke();
            }
        }
        public static void DeleteFact(FactDefinition fact)
        {
            foreach (var key in GetAllKeys())
            {
                bool v = key.Facts.Remove(fact);
                v = key.SerializationOverrides.RemoveAll(o => o.Fact == fact) > 0 || v;
                v = key.ValueOverrides.RemoveAll(o => o.Fact == fact) > 0 || v;
                if (v) EditorUtility.SetDirty(key);
            }
            Database.FactStorage.Remove(fact);
            UnityEngine.Object.DestroyImmediate(fact, true);
            EditorUtility.SetDirty(Database);
            AssetDatabase.SaveAssets();
        }

        public static void RemoveFact(KeyContainer key, FactDefinition fact)
        {
            if (key == null) return;
            key.Facts.Remove(fact);
            key.SerializationOverrides.RemoveAll(o => o.Fact == fact);
            key.ValueOverrides.RemoveAll(o => o.Fact == fact);
            EditorUtility.SetDirty(Database);
            EditorUtility.SetDirty(key);
            AssetDatabase.SaveAssets();
        }
        public static void ShowDeleteEventModal(EventDefinition @event, Action callback = null)
        {
            if (@event == null) return;

            string title = "Delete Key";
            string message = $"Are you sure you want to delete the \"{@event.name}\"?\nThis action cannot be undone.";
            string ok = "Delete";
            string cancel = "Cancel";

            bool confirmed = EditorUtility.DisplayDialog(title, message, ok, cancel);
            if (confirmed)
            {
                DeleteEvent(@event);
                callback?.Invoke();
            }
        }
        public static void RemoveEvent(KeyContainer key, EventDefinition @event)
        {
            if (key == null) return;
            key.Events.Remove(@event);
            EditorUtility.SetDirty(Database);
            EditorUtility.SetDirty(key);
            AssetDatabase.SaveAssets();
        }
        public static void DeleteEvent(EventDefinition @event)
        {
            foreach (var key in GetAllKeys())
            {
                bool v = key.Events.Remove(@event);
                if (v) EditorUtility.SetDirty(key);
            }
            Database.EventStorage.Remove(@event);
            UnityEngine.Object.DestroyImmediate(@event, true);
            EditorUtility.SetDirty(Database);
            AssetDatabase.SaveAssets();
        }
        public static void MoveUp(KeyContainer parent, ScriptableObject item)
        {
            if (item is KeyContainer key)
            {
                if (parent != null)
                {
                    int i = parent.Children.IndexOf(key);
                    if (i < 1) return;
                    (parent.Children[i - 1], parent.Children[i]) = (key, parent.Children[i - 1]);
                    EditorUtility.SetDirty(parent);
                }
                else
                {
                    var db = Database;
                    int i = db.RootKeys.IndexOf(key);
                    if (i < 1) return;
                    (db.RootKeys[i - 1], db.RootKeys[i]) = (key, db.RootKeys[i - 1]);
                    EditorUtility.SetDirty(db);
                }
            }
            else if (item is FactDefinition fact)
            {
                if (parent != null)
                {
                    int i = parent.Facts.IndexOf(fact);
                    if (i < 1) return;
                    (parent.Facts[i - 1], parent.Facts[i]) = (fact, parent.Facts[i - 1]);
                    EditorUtility.SetDirty(parent);
                }
                else
                {
                    var db = Database;
                    int i = db.FactStorage.IndexOf(fact);
                    if (i < 1) return;
                    (db.FactStorage[i - 1], db.FactStorage[i]) = (fact, db.FactStorage[i - 1]);
                    EditorUtility.SetDirty(db);
                }
            }
            else if (item is EventDefinition @event)
            {
                if (parent != null)
                {
                    int i = parent.Events.IndexOf(@event);
                    if (i < 1) return;
                    (parent.Events[i - 1], parent.Events[i]) = (@event, parent.Events[i - 1]);
                    EditorUtility.SetDirty(parent);
                }
                else
                {
                    var db = Database;
                    int i = db.EventStorage.IndexOf(@event);
                    if (i < 1) return;
                    (db.EventStorage[i - 1], db.EventStorage[i]) = (@event, db.EventStorage[i - 1]);
                    EditorUtility.SetDirty(db);
                }
            }
            EditorUtility.SetDirty(Database);
            AssetDatabase.SaveAssets();
        }

        public static void MoveDown(KeyContainer parent, ScriptableObject item)
        {
            if (item is KeyContainer key)
            {
                if (parent != null)
                {
                    int i = parent.Children.IndexOf(key);
                    if (i < 0 || i >= parent.Children.Count - 1) return;
                    (parent.Children[i], parent.Children[i + 1]) = (parent.Children[i + 1], key);
                    EditorUtility.SetDirty(parent);

                }
                else
                {
                    var db = Database;
                    int i = db.RootKeys.IndexOf(key);
                    if (i < 0 || i >= db.RootKeys.Count - 1) return;
                    (db.RootKeys[i], db.RootKeys[i + 1]) = (db.RootKeys[i + 1], key);
                    EditorUtility.SetDirty(db);
                }
            }
            else if (item is FactDefinition fact)
            {
                if (parent != null)
                {
                    int i = parent.Facts.IndexOf(fact);
                    if (i < 0 || i >= parent.Facts.Count - 1) return;
                    (parent.Facts[i], parent.Facts[i + 1]) = (parent.Facts[i + 1], fact);
                    EditorUtility.SetDirty(parent);
                }
                else
                {
                    var db = Database;
                    int i = db.FactStorage.IndexOf(fact);
                    if (i < 0 || i >= db.FactStorage.Count - 1) return;
                    (db.FactStorage[i], db.FactStorage[i + 1]) = (db.FactStorage[i + 1], fact);
                    EditorUtility.SetDirty(db);
                }
            }
            else if (item is EventDefinition @event)
            {
                if (parent != null)
                {
                    int i = parent.Events.IndexOf(@event);
                    if (i < 0 || i >= parent.Events.Count - 1) return;
                    (parent.Events[i], parent.Events[i + 1]) = (parent.Events[i + 1], @event);
                    EditorUtility.SetDirty(parent);
                }
                else
                {
                    var db = Database;
                    int i = db.EventStorage.IndexOf(@event);
                    if (i < 0 || i >= db.EventStorage.Count - 1) return;
                    (db.EventStorage[i], db.EventStorage[i + 1]) = (db.EventStorage[i + 1], @event);
                    EditorUtility.SetDirty(db);
                }
            }
            EditorUtility.SetDirty(Database);
            AssetDatabase.SaveAssets();
        }
        public class PopupWindow : PopupWindowContent
        {
            private readonly Action<Rect, EditorWindow> OnDraw;
            private readonly Vector2 _size;
            public PopupWindow(Action<Rect, EditorWindow> onDraw, Vector2 size)
            {
                OnDraw = onDraw;
                _size = size;
            }
            public override Vector2 GetWindowSize() => _size;
            public override void OnGUI(Rect rect)
            {
                OnDraw?.Invoke(rect, editorWindow);
            }
        }
        class Dropdown<T> : AdvancedDropdown
        {
            private readonly List<T> values;
            private readonly Action<T> onSelect;
            private readonly Func<T, string> nameSelector, typeSelector;

            public Dropdown(AdvancedDropdownState state,
                                List<T> values,
                                Func<T, string> nameSelector,
                                Func<T, string> typeSelector,
                                Action<T> onSelect)
                : base(state)
            {
                this.values = values;
                this.onSelect = onSelect;
                this.nameSelector = nameSelector;
                this.typeSelector = typeSelector;
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Facts");
                values.ForEach(i => root.AddChild(new Item(i, nameSelector(i), typeSelector(i))));
                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is Item i) onSelect?.Invoke(i.value);
            }

            class Item : AdvancedDropdownItem
            {
                public T value;

                public Item(T value, string name, string type) : base($"[{type}] {name}")
                {
                    this.value = value;
                }
            }
        }
    }
}