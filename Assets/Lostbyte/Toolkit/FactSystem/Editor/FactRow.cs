using System;
using System.Linq;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
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

        private readonly SerializedObject _factSO;
        private readonly SerializedObject _keySO;

        private Toggle _serializationField;
        private Image _serializationIcon;
        private VisualElement _valueField;
        private Button _reactionIndicator;
        private Label _reactionCountLabel;

        public FactRow(FactDefinition fact, KeyContainer key)
        {
            Fact = fact;
            Key = key;
            _factSO = new SerializedObject(fact);
            if (Key != null) _keySO = new SerializedObject(Key);

            this.MakeRow().SetAlignItems(Align.Center).SetFlex(1, 0);

            if (Fact is EnumFactDefinition eFact && eFact.DefaultValue == null)
                eFact.DefaultValue = eFact.DefaultEnumValue;

            AddNameField();
            AddSerializationField();
            AddTypeField();
            AddValueField();
            AddReactionIndicator();

            UpdateBindings();

            if (_keySO != null)
            {
                var tracker = new VisualElement { name = "KeyTracker" }.Hide();
                Add(tracker);
                tracker.TrackSerializedObjectValue(_keySO, so => UpdateBindings());
            }

            if (_factSO != null)
            {
                var tracker = new VisualElement { name = "FactTracker" }.Hide();
                Add(tracker);
                tracker.TrackSerializedObjectValue(_factSO, so => UpdateBindings());
            }

            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (Application.isPlaying && Key != null && Fact != null)
                    Key.GetWrapper(Fact)?.Unsubscribe(UpdateCurentValueField);
            });
        }

        private int GetRegistrationIndex() => Key?.FactRegistrations?.FindIndex(r => r.Fact == Fact) ?? -1;
        private bool TryGetSerializationOverride(out int index) => (index = GetRegistrationIndex()) >= 0 && Key.FactRegistrations[index].IsSerializable.HasValue;
        private bool TryGetValueOverride(out int index) => (index = GetRegistrationIndex()) >= 0 && Key.FactRegistrations[index].ValueOverride != null;

        private void UpdateBindings()
        {
            if (_factSO == null) return;
            _factSO.Update();
            _keySO?.Update();

            UpdateSerializationField();
            UpdateValueFieldUI();

            if (_reactionIndicator != null && Key != null)
            {
                int index = GetRegistrationIndex();
                UpdateIndicatorUI(index < 0 || Key.FactRegistrations[index].Reactions == null ? 0 : Key.FactRegistrations[index].Reactions.Count);
            }
        }

        private void UpdateSerializationField()
        {
            if (_serializationField == null) return;
            _serializationField.Unbind();

            if (Key != null && TryGetSerializationOverride(out int sIndex))
            {
                var prop = _keySO.FindProperty($"<{nameof(KeyContainer.FactRegistrations)}>k__BackingField")
                    .GetArrayElementAtIndex(sIndex).FindPropertyRelative(nameof(FactRegistration.IsSerializable));
                var boolProp = prop.FindPropertyRelative("m_value") ?? prop;

                _serializationField.BindProperty(boolProp);
                SetOverrideBorder(_serializationField, true);
                _serializationIcon.image = EditorGUIUtility.IconContent(boolProp.boolValue ? "SaveAs" : "CrossIcon").image;
            }
            else
            {
                var prop = _factSO.FindProperty($"<{nameof(FactDefinition<object>.IsSerializable)}>k__BackingField");
                if (prop != null)
                {
                    _serializationField.BindProperty(prop);
                    _serializationIcon.image = EditorGUIUtility.IconContent(prop.boolValue ? "SaveAs" : "CrossIcon").image;
                }
                SetOverrideBorder(_serializationField, false);
            }
        }

        private void UpdateValueFieldUI()
        {
            if (_valueField == null) return;
            if (_valueField is IBindable bindable) bindable.binding = null;

            SerializedProperty targetProp = null;

            if (Key != null && TryGetValueOverride(out int vIndex))
            {
                var listProp = _keySO.FindProperty($"<{nameof(KeyContainer.FactRegistrations)}>k__BackingField");
                var overrideProp = listProp.GetArrayElementAtIndex(vIndex).FindPropertyRelative(nameof(FactRegistration.ValueOverride));
                targetProp = overrideProp.FindPropertyRelative("<Value>k__BackingField") ?? overrideProp.FindPropertyRelative("Value") ?? overrideProp.FindPropertyRelative("value") ?? overrideProp;

                _valueField.SetEnabledState(true);
                SetOverrideBorder(_valueField, true);
            }
            else
            {
                targetProp = _factSO.FindProperty($"<{nameof(FactDefinition<int>.DefaultValue)}>k__BackingField");
                _valueField.SetEnabledState(Key == null);
                SetOverrideBorder(_valueField, false);
            }
            if (targetProp != null && !Application.isPlaying)
            {
                object val = targetProp.propertyType == SerializedPropertyType.ManagedReference
                    ? targetProp.managedReferenceValue
                    : GetBoxedValueSafely(targetProp);

                var setValueWithoutNotify = _valueField.GetType().GetMethod("SetValueWithoutNotify");
                setValueWithoutNotify?.Invoke(_valueField, new object[] { val });
            }
            if (Application.isPlaying)
            {
                _valueField.SetEnabledState(true);
                SetOverrideBorder(_valueField, false);
            }
        }

        private void OnValueFieldValueChanged(object newValue)
        {
            if (Application.isPlaying && Key != null)
            {
                var wrapper = Key.GetWrapper(Fact);
                var valueProp = wrapper?.GetType().GetProperty("Value");
                if (valueProp != null && valueProp.CanWrite) valueProp.SetValue(wrapper, newValue);
                return;
            }

            _factSO.Update();
            _keySO?.Update();

            SerializedProperty targetProp = null;
            SerializedObject targetSO = null;

            if (Key != null && TryGetValueOverride(out int vIndex))
            {
                var listProp = _keySO.FindProperty($"<{nameof(KeyContainer.FactRegistrations)}>k__BackingField");
                var overrideProp = listProp.GetArrayElementAtIndex(vIndex).FindPropertyRelative(nameof(FactRegistration.ValueOverride));
                targetProp = overrideProp.FindPropertyRelative("<Value>k__BackingField") ?? overrideProp.FindPropertyRelative("Value") ?? overrideProp.FindPropertyRelative("value") ?? overrideProp;
                targetSO = _keySO;
            }
            else
            {
                targetProp = _factSO.FindProperty($"<{nameof(FactDefinition<int>.DefaultValue)}>k__BackingField");
                targetSO = _factSO;
            }

            if (targetProp != null)
            {
                if (targetProp.propertyType == SerializedPropertyType.ManagedReference)
                    targetProp.managedReferenceValue = newValue;
                else
                    SetBoxedValueSafely(targetProp, newValue);

                targetSO.ApplyModifiedProperties();
            }
        }

        private void AddValueField()
        {
            object initialValue = Fact.DefaultValueRaw;
            VisualElement wrapperField = new() { name = "ValueWrapper" };
            if (Application.isPlaying && Key != null)
            {
                var wrapper = Key.GetWrapper(Fact);
                var valueProp = wrapper?.GetType().GetProperty("Value");
                if (valueProp != null && valueProp.CanRead)
                {
                    initialValue = valueProp.GetValue(wrapper);
                    wrapper.Subscribe(UpdateCurentValueField);
                }
            }
            var type = Fact.GenericType;
            if (Fact is EnumFactDefinition eFact) type = eFact.EnumType ?? typeof(Enum);

            _valueField = FieldFactory.CreateFactValueField(type, "", initialValue, OnValueFieldValueChanged);
            wrapperField.Add(_valueField.ClearPaddingAndMargin());

            if (!Application.isPlaying && Key != null)
            {
                wrapperField.AddContextualMenu(evt =>
                {
                    if (TryGetValueOverride(out int i))
                    {
                        evt.menu.AppendAction("Remove Override", (e) =>
                        {
                            var reg = Key.FactRegistrations[i];
                            reg.ValueOverride = null;
                            Key.FactRegistrations[i] = reg;
                            ApplyChanges();
                        });
                    }
                    else
                    {
                        evt.menu.AppendAction("Add Override", (e) =>
                        {
                            IValueHolder newHolder = FieldFactory.CreateValueHolderForFact(Fact);
                            if (newHolder == null) return;

                            int targetIndex = GetRegistrationIndex();
                            if (targetIndex < 0)
                                Key.FactRegistrations.Add(new FactRegistration { Fact = Fact, ValueOverride = newHolder });
                            else
                            {
                                var reg = Key.FactRegistrations[targetIndex];
                                reg.ValueOverride = newHolder;
                                Key.FactRegistrations[targetIndex] = reg;
                            }
                            ApplyChanges();
                        });
                    }
                });
            }
            AddColumnField(wrapperField);
        }

        private void AddColumnField(VisualElement field, float grow = 1f)
        {
            field.SetFlex(grow, 0).SetFlexBasis(0).SetPadding(0).SetMargin(0, 4, 0, 0);
            Add(field);
        }

        private void SetOverrideBorder(VisualElement element, bool value)
        {
            element.SetBorderWidth(0, 0, 0, value ? 2f : 0f)
                   .SetBorderColor(new Color(0.011f, 0.6f, 0.89f, 1f))
                   .SetBorderRadius(0);
        }

        private void AddNameField()
        {
            var nameField = new TextField { label = "Fact", value = Fact.name };
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
            _serializationField = new Toggle().SetMaxSize(23, 16).SetEnabledState(!Application.isPlaying);
            _serializationIcon = new Image().SetTooltip("Toggles save serialization").SetSize(16, 16).SetFlex(0, 1);

            _serializationField.RegisterValueChangedCallback(evt =>
            {
                _serializationIcon.image = EditorGUIUtility.IconContent(evt.newValue ? "SaveAs" : "CrossIcon").image;
            });

            _serializationField.RemoveAt(0);
            _serializationField.Add(_serializationIcon);

            if (Key != null)
            {
                _serializationField.AddContextualMenu(evt =>
                {
                    if (TryGetSerializationOverride(out int i))
                    {
                        evt.menu.AppendAction("Remove Override", (e) =>
                        {
                            var reg = Key.FactRegistrations[i];
                            reg.IsSerializable = default;
                            Key.FactRegistrations[i] = reg;
                            ApplyChanges();
                        });
                    }
                    else
                    {
                        evt.menu.AppendAction("Add Override", (e) =>
                        {
                            int targetIndex = GetRegistrationIndex();
                            if (targetIndex < 0)
                                Key.FactRegistrations.Add(new FactRegistration { Fact = Fact, IsSerializable = new Optional<bool>(true) });
                            else
                            {
                                var reg = Key.FactRegistrations[targetIndex];
                                reg.IsSerializable = new Optional<bool>(true);
                                Key.FactRegistrations[targetIndex] = reg;
                            }
                            ApplyChanges();
                        });
                    }
                });
            }
            AddColumnField(_serializationField);
        }

        private void AddTypeField() => AddColumnField(new TextField { value = Fact.GenericType.Name, isReadOnly = true }, 0.5f);

        private void UpdateCurentValueField(object value)
        {
            if (_valueField == null) return;
            var prop = _valueField.GetType().GetProperty("value");
            if (prop != null) prop.SetValue(_valueField, value);
        }

        private void AddReactionIndicator()
        {
            if (Key == null) return;

            _reactionIndicator = new Button()
                .SetEnabledState(false).MakeRow().SetAlignItems(Align.Center).SetJustifyContent(Justify.Center)
                .SetPadding(2, 4).SetMargin(0).SetTooltip("Right-click to manage reactions")
                .SetMinSize(30, 20);

            var icon = new Image { image = EditorGUIUtility.IconContent("d_EventSystem Icon").image }.SetSize(14, 14).SetMargin(0, 2, 0, 0);
            _reactionCountLabel = new Label("0").SetFontStyle(FontStyle.Normal);

            _reactionIndicator.Add(icon);
            _reactionIndicator.Add(_reactionCountLabel);

            AddColumnField(_reactionIndicator, 0f);
        }
        private void UpdateIndicatorUI(int count)
        {
            if (_reactionCountLabel == null || _reactionIndicator == null) return;
            _reactionCountLabel.text = count.ToString();
            _reactionIndicator.SetOpacity(count > 0 ? 1f : 0.4f);
        }

        private void ApplyChanges()
        {
            EditorUtility.SetDirty(Key);
            _keySO.Update();
            UpdateBindings();
        }

        private object GetBoxedValueSafely(SerializedProperty prop)
        {
            return prop.boxedValue;
        }

        private void SetBoxedValueSafely(SerializedProperty prop, object value)
        {
            prop.boxedValue = value;
        }
    }
}