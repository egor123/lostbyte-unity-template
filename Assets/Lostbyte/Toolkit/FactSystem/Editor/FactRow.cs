using System;
using Lostbyte.Toolkit.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public class FactRow : VisualElement
    {
        public FactDefinition Fact { get; private set; }
        public KeyContainer Key { get; private set; }

        // private readonly VisualElement _contentRow;
        private readonly SerializedObject _factSO;
        private readonly SerializedObject _keySO;

        public FactRow(FactDefinition fact, KeyContainer key)
        {
            Fact = fact;
            Key = key;
            _factSO = new SerializedObject(fact);
            if (Key != null) _keySO = new SerializedObject(Key);
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;
            style.alignItems = Align.Center;

            if (Fact is EnumFactDefinition eFact && eFact.DefaultValue == null)
                eFact.DefaultValue = eFact.DefaultEnumValue;

            AddNameField();
            AddSerializationField();
            AddTypeField();
            if (Application.isPlaying && Key != null) AddCurentValueField();
            else AddDefaultValueField();
        }

        private void AddColumnField(VisualElement field, float grow = 1f)
        {
            // var column = new VisualElement();
            // column.style.flexGrow = grow;
            // column.style.flexShrink = 0;
            // column.style.flexBasis = 0;
            // column.style.paddingRight = 4;
            // field.style.flexGrow = 1;
            // column.Add(field);
            // Add(column);

            field.style.flexGrow = grow;
            field.style.flexShrink = 0;
            field.style.flexBasis = 0;
            field.style.paddingRight = 4;
            Add(field);
        }

        private void AddNameField()
        {
            var nameField = new TextField
            {
                label = "Fact",
                value = Fact.name,

            };
            nameField.RegisterCallback<FocusOutEvent>((evt) =>
            {
                if (nameField.value == Fact.name) return;
                if (!FactEditorUtils.ValidateIdentifier(nameField.value)) nameField.value = Fact.name;
                else
                {
                    Fact.name = nameField.value;
                    EditorUtility.SetDirty(FactEditorUtils.Database);
                    AssetDatabase.SaveAssets();
                }
            });
            AddColumnField(nameField);
        }

        private void AddSerializationField()
        {
            // var field = new PropertyField() { label = "Savable" };

            var field = new Toggle();
            field.SetEnabled(!Application.isPlaying);
            var icon = new Image
            {
                image = EditorGUIUtility.IconContent(field.value ? "SaveAs" : "CrossIcon").image,
                tooltip = "Toggles save serialization"
            };
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginLeft = 0;
            field.RegisterValueChangedCallback(evt =>
            {
                icon.image = EditorGUIUtility.IconContent(evt.newValue ? "SaveAs" : "CrossIcon").image;
            });
            field.RemoveAt(0);
            field.Add(icon);


            bool TryGetOverride(out FactSerializationOverride serializationOverride, out int i)
            {
                for (i = 0; i < Key.SerializationOverrides.Count; i++)
                {
                    if (Key.SerializationOverrides[i].Fact == Fact)
                    {
                        serializationOverride = Key.SerializationOverrides[i];
                        return true;
                    }
                }
                serializationOverride = default;
                return false;
            }
            void BindDirectly()
            {
                _factSO.ApplyModifiedProperties();
                _factSO.Update();
                var prop = _factSO.FindProperty($"<{nameof(FactDefinition<object>.IsSerializable)}>k__BackingField");
                field.BindProperty(prop);
                SetOverrideBorder(field, false);
            }
            void BindToOverride(int i)
            {
                _keySO.ApplyModifiedProperties();
                _keySO.Update();
                var overridesProp = _keySO.FindProperty($"<{nameof(KeyContainer.SerializationOverrides)}>k__BackingField");
                var prop = overridesProp.GetArrayElementAtIndex(i).FindPropertyRelative($"<{nameof(FactSerializationOverride.IsSerializable)}>k__BackingField");
                field.BindProperty(prop);
                SetOverrideBorder(field, true);
            }
            if (Key != null && TryGetOverride(out _, out int i)) BindToOverride(i);
            else BindDirectly();

            if (Key != null)
            {
                field.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    if (TryGetOverride(out var serializationOverride, out int i))
                        evt.menu.AppendAction("Remove Override", (e) =>
                        {
                            Key.SerializationOverrides.RemoveAt(i);
                            EditorUtility.SetDirty(Key);
                            BindDirectly();
                        });
                    else
                        evt.menu.AppendAction("Add Override", (e) =>
                        {
                            Key.SerializationOverrides.Add(new() { Fact = Fact, IsSerializable = true });
                            EditorUtility.SetDirty(Key);
                            BindToOverride(Key.SerializationOverrides.Count - 1);
                        });
                }));
            }
            // AddColumnField(field, 0.15f);
            // _contentRow.Add(field);
            Add(field);
        }
        private void AddDefaultValueField()
        {
            var field = new PropertyField { label = "", }; //  label = "Default Value"


            bool TryGetOverride(out FactValueOverride valueOverride, out int i)
            {
                for (i = 0; i < Key.ValueOverrides.Count; i++)
                {
                    if (Key.ValueOverrides[i].Fact == Fact)
                    {
                        valueOverride = Key.ValueOverrides[i];
                        return true;
                    }
                }
                valueOverride = default;
                return false;
            }
            void BindDirectly()
            {
                _factSO.ApplyModifiedProperties();
                _factSO.Update();
                var prop = _factSO.FindProperty($"<{nameof(FactDefinition<int>.DefaultValue)}>k__BackingField");
                if (prop != null) field.BindProperty(prop);
                else field.Unbind();
                field.SetEnabled(Key == null);
                SetOverrideBorder(field, false);
            }
            void BindToOverride(int i)
            {
                _keySO.ApplyModifiedProperties();
                _keySO.Update();
                var overridesProp = _keySO.FindProperty($"<{nameof(KeyContainer.ValueOverrides)}>k__BackingField");
                var prop = overridesProp.GetArrayElementAtIndex(i)
                    .FindPropertyRelative($"<{nameof(FactValueOverride.Wrapper)}>k__BackingField")
                    .FindPropertyRelative($"<{nameof(ValueHolder<object>.Value)}>k__BackingField");
                if (prop != null) field.BindProperty(prop);
                else field.Unbind();
                field.SetEnabled(true);
                SetOverrideBorder(field, true);
            }

            if (Key != null)
            {
                if (TryGetOverride(out _, out int i)) BindToOverride(i);
                else BindDirectly();
                this.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    if (TryGetOverride(out var valueOverride, out int i))
                        evt.menu.AppendAction("Remove Override", (e) =>
                        {
                            Key.ValueOverrides.RemoveAt(i);
                            EditorUtility.SetDirty(Key);
                            BindDirectly();
                        });
                    else
                        evt.menu.AppendAction("Add Override", (e) =>
                        {
                            if (Fact.GenericType == typeof(float))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new FloatValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(int))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new IntValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(bool))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new BoolValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(string))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new StringValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(Vector2))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new Vector2ValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(Vector3))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new Vector3ValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(Vector4))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new Vector4ValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(Color))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new ColorValueHolder() { RawValue = Fact.DefaultValueRaw } });
                            else if (Fact.GenericType == typeof(Enum))
                                Key.ValueOverrides.Add(new FactValueOverride() { Fact = Fact, Wrapper = new EnumValueHolder() { RawValue = ((Enum)Fact.DefaultValueRaw) ?? (Fact as EnumFactDefinition).DefaultEnumValue } });
                            else
                            {
                                DebugLogger.LogWarning("Unknown type!");
                                EditorUtility.SetDirty(Key);
                                BindDirectly();
                                return;
                            }
                            EditorUtility.SetDirty(Key);
                            BindToOverride(Key.ValueOverrides.Count - 1);
                        });
                }));
            }
            else
            {
                BindDirectly();
            }
            AddColumnField(field);
        }

        private void AddCurentValueField()
        {
            if (Key == null) return;
            var wrapper = Key.GetWrapper(Fact);
            var valueProp = wrapper.GetType().GetProperty("Value");
            if (valueProp != null && valueProp.CanRead && valueProp.CanWrite)
            {
                object currentValue = valueProp.GetValue(wrapper);
                _curentValueField = FieldFactory.CreateFactValueField(
                    currentValue.GetType(),
                    "", // "Current Value"
                    currentValue,
                    val => valueProp.SetValue(wrapper, val)
                );
                wrapper.Subscribe(UpdateCurentValueField);
                AddColumnField(_curentValueField);
            }
        }
        private void SetOverrideBorder(VisualElement element, bool value)
        {
            Color borderColor = Color.yellow;
            float borderWidth = value ? 0.5f : 0f;
            float borderRadius = 4f;

            element.style.borderBottomColor = borderColor;
            element.style.borderTopColor = borderColor;
            element.style.borderLeftColor = borderColor;
            element.style.borderRightColor = borderColor;

            element.style.borderBottomWidth = borderWidth;
            element.style.borderTopWidth = borderWidth;
            element.style.borderLeftWidth = borderWidth;
            element.style.borderRightWidth = borderWidth;

            element.style.borderBottomLeftRadius = borderRadius;
            element.style.borderBottomRightRadius = borderRadius;
            element.style.borderTopLeftRadius = borderRadius;
            element.style.borderTopRightRadius = borderRadius;
        }
        private void AddTypeField()
        {
            var typeField = new TextField
            {
                // label = "Type",
                value = Fact.GenericType.Name,
                isReadOnly = true
            };
            AddColumnField(typeField, 0.5f);
        }



        private VisualElement _curentValueField;

        private void UpdateCurentValueField(object value)
        {
            if (_curentValueField == null) return;
            _curentValueField.GetType().GetProperty("value").SetValue(_curentValueField, value);
        }
        ~FactRow()
        {
            if (_curentValueField != null)
                Key.GetWrapper(Fact).Unsubscribe(UpdateCurentValueField);
        }
    }
}