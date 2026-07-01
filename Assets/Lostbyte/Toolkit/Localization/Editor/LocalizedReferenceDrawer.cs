using System;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.Localization.Editor
{
    public struct DropdownFilter
    {
        public Type[] Types;
        public bool IsArray;
    }

    public class LocalizationSearchDropdown : AdvancedDropdown
    {
        public event Action<string, string, IReadOnlyList<ArgumentDefinition>> OnItemSelected;
        public readonly DropdownFilter? Filter;

        public LocalizationSearchDropdown(AdvancedDropdownState state, DropdownFilter? filter) : base(state)
        {
            minimumSize = new Vector2(250, 300);
            Filter = filter;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Localization Keys");
            var db = LocalizationSettings.Database;
            if (db == null) return root;

            foreach (var table in db.Schema.Tables)
            {
                var tableGroup = new AdvancedDropdownItem(table.Id);
                foreach (var key in table.Keys)
                {
                    bool typeMatches = !Filter.HasValue || (key.IsArray == Filter.Value.IsArray && key.Types.All(t => Filter.Value.Types.Contains(LocalizationKey.AllowedTypes[t])));

                    if (typeMatches)
                        tableGroup.AddChild(new LocalizedKeyItem(table.Id, key.Id, key.Args));
                }

                if (tableGroup.children.Any())
                    root.AddChild(tableGroup);
            }
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is LocalizedKeyItem keyItem)
                OnItemSelected?.Invoke(keyItem.TableName, keyItem.KeyId, keyItem.RequiredArgs);
        }

        private class LocalizedKeyItem : AdvancedDropdownItem
        {
            public string TableName { get; }
            public string KeyId { get; }
            public IReadOnlyList<ArgumentDefinition> RequiredArgs { get; }

            public LocalizedKeyItem(string tableName, string keyId, IReadOnlyList<ArgumentDefinition> requiredArgs) : base($"{tableName} / {keyId}")
            {
                TableName = tableName;
                KeyId = keyId;
                RequiredArgs = requiredArgs;
            }
        }
    }

    [CustomPropertyDrawer(typeof(LocRef), true)]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement { style = { flexDirection = FlexDirection.Column } };

            var tableProp = property.FindPropertyRelative($"<{nameof(LocRef.TableId)}>k__BackingField");
            var keyProp = property.FindPropertyRelative($"<{nameof(LocRef.KeyId)}>k__BackingField");
            var argsProp = property.FindPropertyRelative("m_args");

            var headerRow = new VisualElement().MakeRow();
            headerRow.AddToClassList("unity-base-field");
            headerRow.AddToClassList("unity-property-field");

            if (!string.IsNullOrEmpty(preferredLabel))
            {
                var label = new Label(preferredLabel);
                label.AddToClassList("unity-base-field__label");
                label.AddToClassList("unity-property-field__label");
                headerRow.Add(label);
            }

            var selectorBtn = new Button
            {
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    textOverflow = TextOverflow.Ellipsis,
                    overflow = Overflow.Hidden,
                    marginRight = 0
                }
            };

            headerRow.Add(selectorBtn);

            var argsContainer = new VisualElement { style = { marginLeft = 15 } };

            root.Add(headerRow);
            root.Add(argsContainer);

            void UpdateUI()
            {
                string tId = tableProp?.stringValue;
                string kId = keyProp?.stringValue;

                selectorBtn.text = !string.IsNullOrEmpty(tId) && !string.IsNullOrEmpty(kId)
                    ? $"{tId} / {kId}"
                    : "Select Localization Key...";

                RebuildArgsUI(argsContainer, argsProp, tId, kId);
            }

            selectorBtn.clicked += () => OpenDropdown(selectorBtn.worldBound, property, UpdateUI);

            root.TrackPropertyValue(tableProp, _ => UpdateUI());
            root.TrackPropertyValue(keyProp, _ => UpdateUI());

            UpdateUI();
            return root;
        }

        private void RebuildArgsUI(VisualElement container, SerializedProperty argsProp, string tableId, string keyId)
        {
            container.Clear();
            var requiredArgs = GetArgsFromDatabase(tableId, keyId);
            if (requiredArgs == null || requiredArgs.Count == 0) return;

            argsProp.arraySize = requiredArgs.Count;
            for (int i = 0; i < requiredArgs.Count; i++)
            {
                var argDef = requiredArgs[i];
                var elementProp = argsProp.GetArrayElementAtIndex(i);
                EnsureCorrectReferenceType(elementProp, argDef.Type);
                var field = new PropertyField(elementProp, argDef.Name) { label = argDef.Name };
                field.Bind(argsProp.serializedObject);
                container.Add(field);
            }
            argsProp.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            var tableProp = property.FindPropertyRelative($"<{nameof(LocRef.TableId)}>k__BackingField");
            var keyProp = property.FindPropertyRelative($"<{nameof(LocRef.KeyId)}>k__BackingField");
            var argsProp = property.FindPropertyRelative("m_args");

            var requiredArgs = GetArgsFromDatabase(tableProp?.stringValue, keyProp?.stringValue);
            if (argsProp != null && requiredArgs != null && requiredArgs.Count > 0)
            {
                for (int i = 0; i < argsProp.arraySize; i++)
                {
                    height += EditorGUIUtility.standardVerticalSpacing;
                    height += EditorGUI.GetPropertyHeight(argsProp.GetArrayElementAtIndex(i), true);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var tableProp = property.FindPropertyRelative($"<{nameof(LocRef.TableId)}>k__BackingField");
            var keyProp = property.FindPropertyRelative($"<{nameof(LocRef.KeyId)}>k__BackingField");
            var argsProp = property.FindPropertyRelative("m_args");

            string tId = tableProp?.stringValue;
            string kId = keyProp?.stringValue;

            Rect mainFieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect dropdownRect = EditorGUI.PrefixLabel(mainFieldRect, GUIUtility.GetControlID(FocusType.Passive), label);

            string btnText = (!string.IsNullOrEmpty(tId) && !string.IsNullOrEmpty(kId))
                ? $"{tId} / {kId}"
                : "Select Localization Key...";

            if (GUI.Button(dropdownRect, btnText, EditorStyles.popup))
                OpenDropdown(dropdownRect, property, null);

            var requiredArgs = GetArgsFromDatabase(tId, kId);
            if (argsProp != null && requiredArgs != null && argsProp.arraySize > 0)
            {
                EditorGUI.indentLevel++;
                float currentY = mainFieldRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                for (int i = 0; i < argsProp.arraySize; i++)
                {
                    var argProp = argsProp.GetArrayElementAtIndex(i);
                    float argHeight = EditorGUI.GetPropertyHeight(argProp, true);
                    Rect argRect = new(position.x, currentY, position.width, argHeight);
                    string argName = (i < requiredArgs.Count) ? requiredArgs[i].Name : $"Arg {i}";
                    EditorGUI.PropertyField(argRect, argProp, new GUIContent(argName), true);
                    currentY += argHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private void OpenDropdown(Rect displayRect, SerializedProperty property, Action onSelectionChanged)
        {
            var filter = new DropdownFilter
            {
                Types = property.GetTargetType().GetGenericArguments(),
                IsArray = false
            };

            var dropdown = new LocalizationSearchDropdown(new AdvancedDropdownState(), filter);
            dropdown.OnItemSelected += (tId, kId, requiredArgs) =>
            {
                property.serializedObject.Update();

                var tableProp = property.FindPropertyRelative($"<{nameof(LocRef.TableId)}>k__BackingField");
                var keyProp = property.FindPropertyRelative($"<{nameof(LocRef.KeyId)}>k__BackingField");
                var argsProp = property.FindPropertyRelative("m_args");

                tableProp.stringValue = tId;
                keyProp.stringValue = kId;

                if (requiredArgs != null)
                {
                    argsProp.arraySize = requiredArgs.Count;
                    for (int i = 0; i < requiredArgs.Count; i++)
                        EnsureCorrectReferenceType(argsProp.GetArrayElementAtIndex(i), requiredArgs[i].Type);
                }
                else
                {
                    argsProp.arraySize = 0;
                }
                property.serializedObject.ApplyModifiedProperties();
                onSelectionChanged?.Invoke();
            };
            dropdown.Show(displayRect);
        }

        private IReadOnlyList<ArgumentDefinition> GetArgsFromDatabase(string tableId, string keyId)
        {
            var db = LocalizationSettings.Database;
            if (db == null || string.IsNullOrEmpty(tableId) || string.IsNullOrEmpty(keyId)) return null;

            var table = db.Schema.Tables.FirstOrDefault(t => t.Id == tableId);
            return table.Keys?.FirstOrDefault(k => k.Id == keyId).Args;
        }

        private void EnsureCorrectReferenceType(SerializedProperty prop, string typeName)
        {
            Type requiredClassType = typeName switch
            {
                "string" => typeof(LocStringArg),
                "int" => typeof(LocIntArg),
                "float" => typeof(LocFloatArg),
                "bool" => typeof(LocBoolArg),
                _ => typeof(LocArg)
            };

            string currentTypeStr = prop.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(currentTypeStr) || !currentTypeStr.Contains(requiredClassType.Name))
                prop.managedReferenceValue = Activator.CreateInstance(requiredClassType);
        }
    }
}