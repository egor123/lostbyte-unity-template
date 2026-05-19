using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.CustomEditor.Editor;

namespace Lostbyte.Toolkit.Localization.Editor
{
    public class LocalizedPathField : VisualElement
    {
        private Label m_LabelElement;
        private Button m_SelectorButton;

        private SerializedProperty m_TableProp;
        private SerializedProperty m_KeyProp;

        public DropdownFilter? Filter { get; set; }
        public string TableId { get; private set; }
        public string KeyId { get; private set; }

        public event Action<string, string, IReadOnlyList<ArgumentDefinition>> OnKeySelected;

        public LocalizedPathField(string label = null)
        {
            AddToClassList("unity-base-field");

            m_LabelElement = new Label(label);
            m_LabelElement.AddToClassList("unity-base-field__label");
            m_LabelElement.AddToClassList("unity-property-field__label");
            m_LabelElement.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;

            m_SelectorButton = new Button()
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
            m_SelectorButton.clicked += OnSelectorClicked;

            Add(m_LabelElement);
            Add(m_SelectorButton);
        }

        public void BindToProperty(SerializedProperty tableProp, SerializedProperty keyProp)
        {
            m_TableProp = tableProp;
            m_KeyProp = keyProp;

            this.TrackPropertyValue(m_TableProp, _ => Update());
            this.TrackPropertyValue(m_KeyProp, _ => Update());
            Update();
        }

        public void ApplyToProperties(string tableId, string keyId, IReadOnlyList<ArgumentDefinition> args)
        {
            TableId = tableId;
            KeyId = keyId;
            if (m_TableProp != null && m_KeyProp != null)
            {
                m_TableProp.stringValue = tableId;
                m_KeyProp.stringValue = keyId;
                m_TableProp.serializedObject.ApplyModifiedProperties();
            }
            UpdateSelector(TableId, KeyId);
            OnKeySelected?.Invoke(tableId, keyId, args);
        }

        private void Update()
        {
            if (m_TableProp != null) TableId = m_TableProp.stringValue;
            if (m_KeyProp != null) KeyId = m_KeyProp.stringValue;
            UpdateSelector(TableId, KeyId);
        }

        private void UpdateSelector(string tableId, string keyId)
        {
            m_SelectorButton.text = (!string.IsNullOrEmpty(tableId) && !string.IsNullOrEmpty(keyId))
                ? $"{tableId} / {keyId}"
                : "Select Localization Key...";
        }

        private void OnSelectorClicked()
        {
            var dropdown = new LocalizationSearchDropdown(new AdvancedDropdownState(), Filter);
            dropdown.OnItemSelected += (tableId, keyId, requiredArgs) =>
            {
                ApplyToProperties(tableId, keyId, requiredArgs);
            };
            dropdown.Show(m_SelectorButton.worldBound);
        }
    }
    public class LocalizedReferenceField : BindableElement
    {
        private LocalizedPathField _pathField;
        private VisualElement _argsContainer;
        private SerializedProperty m_Property;
        private SerializedProperty m_ArgsProperty;

        public LocalizedReferenceField(string label = null)
        {
            style.flexDirection = FlexDirection.Column;

            _pathField = new LocalizedPathField(label);
            _pathField.OnKeySelected += HandleKeySelected;

            _argsContainer = new VisualElement { style = { marginLeft = 15 } };

            Add(_pathField);
            Add(_argsContainer);
        }

        public void BindToProperty(SerializedProperty property)
        {
            m_Property = property;
            bindingPath = property.propertyPath;

            var tableProp = property.FindPropertyRelative($"<{nameof(LocRef.TableId)}>k__BackingField");
            var keyProp = property.FindPropertyRelative($"<{nameof(LocRef.KeyId)}>k__BackingField");
            m_ArgsProperty = property.FindPropertyRelative("m_args");

            _pathField.Filter = new()
            {
                Types = property.GetTargetType().GetGenericArguments(),
                IsArray = false
            };
            _pathField.BindToProperty(tableProp, keyProp);
            var initialArgs = GetArgsFromDatabase(tableProp.stringValue, keyProp.stringValue);
            RebuildArgsUI(initialArgs);
        }

        private void HandleKeySelected(string tableId, string keyId, IReadOnlyList<ArgumentDefinition> args)
        {
            RebuildArgsUI(args);
        }

        private void RebuildArgsUI(IReadOnlyList<ArgumentDefinition> requiredArgs)
        {
            _argsContainer.Clear();
            if (requiredArgs == null || requiredArgs.Count == 0) return;
            m_ArgsProperty.arraySize = requiredArgs.Count;
            for (int i = 0; i < requiredArgs.Count; i++)
            {
                var argDef = requiredArgs[i];
                var elementProp = m_ArgsProperty.GetArrayElementAtIndex(i);

                EnsureCorrectReferenceType(elementProp, argDef.Type);
                var field = new PropertyField(elementProp, argDef.Name) { label = argDef.Name };
                field.Bind(m_ArgsProperty.serializedObject);
                field.label = argDef.Name;
                _argsContainer.Add(field);
                field.label = argDef.Name;
            }
            m_ArgsProperty.serializedObject.ApplyModifiedProperties();
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
            {
                object instance = Activator.CreateInstance(requiredClassType);
                prop.managedReferenceValue = instance;
            }
        }

        private IReadOnlyList<ArgumentDefinition> GetArgsFromDatabase(string tableId, string keyId)
        {
            var db = LocalizationSettings.Database;
            if (db == null || string.IsNullOrEmpty(tableId) || string.IsNullOrEmpty(keyId)) return null;
            var table = db.Schema.Tables.FirstOrDefault(t => t.Id == tableId);
            var key = table.Keys.FirstOrDefault(k => k.Id == keyId);
            return key.Args;
        }
    }

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
                    if (Filter.HasValue && key.IsArray == Filter.Value.IsArray && key.Types.All(t => Filter.Value.Types.Contains(LocalizationKey.AllowedTypes[t])))
                        tableGroup.AddChild(new LocalizedKeyItem(table.Id, key.Id, key.Args));
                if (tableGroup.children.Count() > 0)
                    root.AddChild(tableGroup);
            }
            return root;
        }
        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is LocalizedKeyItem keyItem) OnItemSelected?.Invoke(keyItem.TableName, keyItem.KeyId, keyItem.RequiredArgs);
        }

        private class LocalizedKeyItem : AdvancedDropdownItem
        {
            public string TableName { get; }
            public string KeyId { get; }
            public IReadOnlyList<ArgumentDefinition> RequiredArgs { get; }

            public LocalizedKeyItem(string tableName, string keyId, IReadOnlyList<ArgumentDefinition> requiredArgs) : base($"{tableName} / {keyId}")
            {
                TableName = tableName; KeyId = keyId; RequiredArgs = requiredArgs;
            }

        }
    }

    [CustomPropertyDrawer(typeof(LocRef), true)]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new LocalizedReferenceField(property.displayName);
            field.BindToProperty(property);
            return field;
        }
    }
}
