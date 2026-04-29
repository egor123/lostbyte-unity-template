using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor.Editor;
using Lostbyte.Toolkit.FactSystem;
using Lostbyte.Toolkit.Localization;
using TMPro;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.UI.Editor
{
    [CustomField(typeof(LocalizedString))]
    public class LocalizedStringField : BindableElement, INotifyValueChanged<LocalizedString>
    {
        private Label m_LabelElement;
        private Button m_SelectorButton;
        private VisualElement m_ArgsContainer;

        private SerializedProperty m_Property;
        private SerializedProperty m_TableProp;
        private SerializedProperty m_KeyProp;
        private SerializedProperty m_FactsProp;

        private LocalizedString m_Value;

        public string label
        {
            get => m_LabelElement.text;
            set
            {
                m_LabelElement.text = value;
                m_LabelElement.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public LocalizedString value
        {
            get => m_Value;
            set
            {
                if (ReferenceEquals(m_Value, value)) return;
                using ChangeEvent<LocalizedString> evt = ChangeEvent<LocalizedString>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }

        public void SetValueWithoutNotify(LocalizedString newValue)
        {
            m_Value = newValue;
            UpdateDisplayAndArgs();
        }

        public LocalizedStringField() : this(null) { }

        public LocalizedStringField(string label = null)
        {
            // AddToClassList(BaseField<LocalizedString>.ussClassName);
            style.minWidth = 250;

            var selectionRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            m_LabelElement = new Label(label);
            // m_LabelElement.AddToClassList(BaseField<LocalizedString>.labelUssClassName);
            m_LabelElement.style.flexBasis = new Length(20, LengthUnit.Percent);
            m_LabelElement.style.flexShrink = 0;
            m_LabelElement.style.flexGrow = 0;
            m_LabelElement.style.marginRight = 5;
            m_LabelElement.style.whiteSpace = WhiteSpace.Normal;

            m_LabelElement.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;

            var inputContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };
            inputContainer.AddToClassList(BaseField<LocalizedString>.inputUssClassName);

            m_SelectorButton = new Button() { style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
            m_SelectorButton.clicked += OnSelectorClicked;

            m_ArgsContainer = new VisualElement();

            selectionRow.Add(m_LabelElement);
            selectionRow.Add(m_SelectorButton);

            inputContainer.Add(selectionRow);
            inputContainer.Add(m_ArgsContainer);

            Add(inputContainer);

            SetValueWithoutNotify(null);
        }

        public void BindToProperty(SerializedProperty property)
        {
            m_Property = property;
            bindingPath = property.propertyPath;

            m_TableProp = property.FindPropertyRelative("m_table");
            m_KeyProp = property.FindPropertyRelative("m_key");
            m_FactsProp = property.FindPropertyRelative("m_facts");

            UpdateDisplayAndArgs();

            this.TrackPropertyValue(m_TableProp, _ => UpdateDisplayAndArgs());
            this.TrackPropertyValue(m_KeyProp, _ => UpdateDisplayAndArgs());
        }

        private void UpdateDisplayAndArgs()
        {
            string t = null;
            string k = null;

            if (m_Property != null && m_TableProp != null && m_KeyProp != null)
            {
                t = m_TableProp.stringValue;
                k = m_KeyProp.stringValue;
            }
            else if (m_Value != null)
            {
                var type = typeof(LocalizedString);
                t = type.GetField("m_table", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(m_Value) as string;
                k = type.GetField("m_key", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(m_Value) as string;
            }

            m_SelectorButton.text = (!string.IsNullOrEmpty(t) && !string.IsNullOrEmpty(k))
                ? $"{t} / {k}"
                : "Select Localization Key...";

            string[] requiredArgs = GetRequiredArgs(t, k);
            DrawArguments(requiredArgs);
        }

        private void OnSelectorClicked()
        {
            var dropdown = new LocalizationSearchDropdown(new AdvancedDropdownState());
            dropdown.OnItemSelected += (tableName, keyId, requiredArgs) =>
            {
                int argCount = requiredArgs != null ? requiredArgs.Length : 0;

                if (m_Property != null)
                {
                    m_Property.serializedObject.Update();
                    m_TableProp.stringValue = tableName;
                    m_KeyProp.stringValue = keyId;

                    if (m_FactsProp != null) m_FactsProp.arraySize = argCount;
                    m_Property.serializedObject.ApplyModifiedProperties();
                }

                var newValue = new LocalizedString(keyId, tableName);

                if (argCount > 0)
                {
                    var factsField = typeof(LocalizedString).GetField("m_facts", BindingFlags.Instance | BindingFlags.NonPublic);
                    var elementType = factsField?.FieldType.GetElementType();
                    if (elementType != null)
                    {
                        factsField.SetValue(newValue, Array.CreateInstance(elementType, argCount));
                    }
                }

                value = newValue;
            };

            dropdown.Show(m_SelectorButton.worldBound);
        }

        private void DrawArguments(string[] requiredArgs)
        {
            m_ArgsContainer.Clear();

            int argCount = requiredArgs != null ? requiredArgs.Length : 0;

            if (argCount == 0) return;

            // m_ArgsContainer.Add(new Label("Arguments") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 2 } });


            FieldInfo factsField = null;
            Array factsArray = null;
            Type tupleType = null;
            FieldInfo item1Field = null;
            FieldInfo item2Field = null;

            if (m_Property == null && m_Value != null)
            {
                factsField = typeof(LocalizedString).GetField("m_facts", BindingFlags.Instance | BindingFlags.NonPublic);
                if (factsField != null)
                {
                    tupleType = factsField.FieldType.GetElementType();
                    factsArray = factsField.GetValue(m_Value) as Array;

                    if (factsArray == null || factsArray.Length != argCount)
                    {
                        var newArray = Array.CreateInstance(tupleType, argCount);
                        if (factsArray != null) Array.Copy(factsArray, newArray, Math.Min(factsArray.Length, argCount));
                        factsArray = newArray;
                        factsField.SetValue(m_Value, factsArray);
                    }

                    item1Field = tupleType.GetField("<Item1>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ?? tupleType.GetField("Item1", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    item2Field = tupleType.GetField("<Item2>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ?? tupleType.GetField("Item2", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }
            }

            for (int i = 0; i < argCount; i++)
            {
                var argRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                    }
                };
                var nameTypeSplit = requiredArgs[i].Split(':');
                var argLabel = new Label(nameTypeSplit[0]);
                argLabel.style.flexBasis = new Length(20, LengthUnit.Percent);
                argLabel.style.flexShrink = 0;
                argLabel.style.flexGrow = 0;
                // argLabel.style.marginRight = 10;
                argLabel.style.whiteSpace = WhiteSpace.Normal;
                argRow.style.paddingRight = 5;

                argRow.Add(argLabel);

                var fieldsContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
                fieldsContainer.style.flexBasis = new Length(80, LengthUnit.Percent);
                fieldsContainer.style.flexShrink = 0;
                fieldsContainer.style.flexGrow = 0;

                argRow.Add(fieldsContainer);
                var factType = ResolveType(nameTypeSplit.Length > 1 ? nameTypeSplit[1] : null);

                if (m_Property != null && m_FactsProp != null && i < m_FactsProp.arraySize)
                {
                    // --- BOUND MODE ---
                    var elementProp = m_FactsProp.GetArrayElementAtIndex(i);
                    var key = elementProp.FindPropertyRelative("<Item1>k__BackingField") ?? elementProp.FindPropertyRelative("Item1");
                    var fact = elementProp.FindPropertyRelative("<Item2>k__BackingField") ?? elementProp.FindPropertyRelative("Item2");

                    if (key != null)
                    {
                        var keyField = new ObjectField
                        {
                            objectType = typeof(KeyContainer),
                            allowSceneObjects = false,
                            value = fact.objectReferenceValue
                        };
                        keyField.BindProperty(key);

                        keyField.style.flexGrow = 1;
                        keyField.style.flexShrink = 1;
                        keyField.style.flexBasis = new Length(50, LengthUnit.Percent); ;

                        fieldsContainer.Add(keyField);
                    }
                    if (fact != null)
                    {
                        var factField = new ObjectField
                        {
                            objectType = factType,
                            allowSceneObjects = false,
                            value = fact.objectReferenceValue
                        };
                        factField.BindProperty(fact);
                        factField.style.flexBasis = new Length(50, LengthUnit.Percent); ;

                        factField.style.flexGrow = 1;
                        factField.style.flexShrink = 1;
                        fieldsContainer.Add(factField);
                    }
                }
                else if (factsArray != null)
                {
                    // --- UNBOUND MODE ---
                    object tupleInstance = factsArray.GetValue(i) ?? new SerializedTuple<KeyContainer, FactDefinition>(null, null);
                    factsArray.SetValue(tupleInstance, i);

                    if (item1Field != null)
                    {
                        var keyField = CreateUnboundField(item1Field.FieldType, tupleInstance, item1Field, i, factsArray);
                        keyField.style.flexGrow = 1;
                        keyField.style.flexShrink = 1;
                        fieldsContainer.Add(keyField);
                    }
                    if (item2Field != null)
                    {
                        var factField = CreateUnboundField(factType, tupleInstance, item2Field, i, factsArray);
                        factField.style.flexGrow = 1;
                        factField.style.flexShrink = 1;
                        fieldsContainer.Add(factField);
                    }
                }

                m_ArgsContainer.Add(argRow);
            }

            if (m_Property != null) m_ArgsContainer.Bind(m_Property.serializedObject);
        }
        private static Type ResolveType(string typeName)
        {
            return typeName switch
            {
                "string" => typeof(StringFactDifenition),
                "int" => typeof(IntFactDefinition),
                "float" => typeof(FloatFactDefinition),
                "bool" => typeof(BoolFactDefinition),
                _ => typeof(FactDefinition)
            };
        }
        private VisualElement CreateUnboundField(Type fieldType, object target, FieldInfo fieldInfo, int arrayIndex, Array factsArray)
        {
            VisualElement fieldElement;
            var objField = new ObjectField("") { objectType = fieldType, value = fieldInfo.GetValue(target) as UnityEngine.Object, allowSceneObjects = false };
            objField.RegisterValueChangedCallback(e => UpdateUnboundValue(e.newValue, target, fieldInfo, arrayIndex, factsArray));
            fieldElement = objField;
            fieldElement.style.flexGrow = 1;
            fieldElement.style.flexBasis = 0;
            return fieldElement;
        }

        private void UpdateUnboundValue(object newValue, object targetTuple, FieldInfo fieldInfo, int arrayIndex, Array factsArray)
        {
            fieldInfo.SetValue(targetTuple, newValue);
            factsArray.SetValue(targetTuple, arrayIndex);
            using ChangeEvent<LocalizedString> evt = ChangeEvent<LocalizedString>.GetPooled(m_Value, m_Value);
            evt.target = this;
            SendEvent(evt);
        }

        private string[] GetRequiredArgs(string table, string key)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key)) return null;

            var db = LocalizationSettings.Database;
            if (db == null) return null;

            FieldInfo sourceItemsField = db.GetType().GetField("m_sourceItems", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (sourceItemsField?.GetValue(db) is IList sourceFiles)
            {
                foreach (var fileObj in sourceFiles)
                {
                    string name = fileObj.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(fileObj) as string
                               ?? fileObj.GetType().GetField("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(fileObj) as string;

                    if (name != table) continue;

                    FieldInfo keysField = fileObj.GetType().GetField("keys", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (keysField?.GetValue(fileObj) is IList keys)
                    {
                        foreach (var itemObj in keys)
                        {
                            FieldInfo idField = itemObj.GetType().GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (idField?.GetValue(itemObj) as string == key)
                            {
                                FieldInfo argsField = itemObj.GetType().GetField("args", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                return argsField?.GetValue(itemObj) as string[];
                            }
                        }
                    }
                }
            }
            return null;
        }

        private class LocalizationSearchDropdown : AdvancedDropdown
        {
            public Action<string, string, string[]> OnItemSelected;

            public LocalizationSearchDropdown(AdvancedDropdownState state) : base(state) { minimumSize = new Vector2(250, 300); }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Localization Keys");
                var db = LocalizationSettings.Database;
                if (db == null) return root;

                FieldInfo sourceItemsField = db.GetType().GetField("m_sourceItems", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (sourceItemsField?.GetValue(db) is IList sourceFiles)
                {
                    foreach (var fileObj in sourceFiles)
                    {
                        string tableName =
                            fileObj.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(fileObj) as string
                            ?? fileObj.GetType().GetField("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(fileObj) as string;
                        if (string.IsNullOrEmpty(tableName)) continue;
                        var tableGroup = new AdvancedDropdownItem(tableName);
                        FieldInfo keysField = fileObj.GetType().GetField("keys", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (keysField?.GetValue(fileObj) is IList keys)
                        {
                            foreach (var itemObj in keys)
                            {
                                FieldInfo idField = itemObj.GetType().GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                string keyId = idField?.GetValue(itemObj) as string;
                                if (string.IsNullOrEmpty(keyId)) continue;
                                FieldInfo argsField = itemObj.GetType().GetField("args", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                var args = argsField?.GetValue(itemObj) as string[];
                                tableGroup.AddChild(new LocalizedKeyItem(tableName, keyId, args));
                            }
                        }
                        if (tableGroup.children.Count() > 0) root.AddChild(tableGroup);
                    }
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
                public string[] RequiredArgs { get; }

                public LocalizedKeyItem(string tableName, string keyId, string[] requiredArgs) : base($"{tableName} / {keyId}")
                {
                    TableName = tableName; KeyId = keyId; RequiredArgs = requiredArgs;
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new LocalizedStringField(property.displayName);
            field.BindToProperty(property);
            return field;
        }
    }
    [UnityEditor.CustomEditor(typeof(LocalizedField))]
    public class LocalizedFieldEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var propertyField = new PropertyField(serializedObject.FindProperty("<String>k__BackingField"));
            propertyField.Bind(serializedObject);
            root.Add(propertyField);
            root.TrackSerializedObjectValue(serializedObject, _ => { UpdatePreview(); });
            UpdatePreview();
            return root;
        }

        private void UpdatePreview()
        {
            if (Application.isPlaying) return;
            var localizedField = (LocalizedField)target;
            var so = serializedObject;
            var stringProp = so.FindProperty("<String>k__BackingField");
            if (stringProp == null) return;
            var tableProp = stringProp.FindPropertyRelative("m_table");
            var keyProp = stringProp.FindPropertyRelative("m_key");
            if (tableProp == null || keyProp == null) return;
            string table = tableProp.stringValue;
            string key = keyProp.stringValue;
            if (localizedField.TryGetComponent<TMP_Text>(out var text))
            {
                text.text = $"{table}/{key}";
                EditorUtility.SetDirty(text);
            }
        }
    }
}