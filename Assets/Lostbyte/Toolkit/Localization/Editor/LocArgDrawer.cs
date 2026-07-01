using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.Localization;
using Lostbyte.Toolkit.FactSystem.Editor;
using System;
using Lostbyte.Toolkit.CustomEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.Editor
{
    [CustomPropertyDrawer(typeof(LocArg))]
    [CustomPropertyDrawer(typeof(LocStringArg))]
    [CustomPropertyDrawer(typeof(LocIntArg))]
    [CustomPropertyDrawer(typeof(LocFloatArg))]
    [CustomPropertyDrawer(typeof(LocBoolArg))]
    public class LocArgDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement().MakeRow().SetFlex(0, 0);

            var isDynamicProp = property.FindPropertyRelative("m_isDynamic");
            var staticValueProp = property.FindPropertyRelative("m_staticValue");
            var keyProp = property.FindPropertyRelative("m_key");
            var factProp = property.FindPropertyRelative("m_fact");

            var dynamicToggle = new Button(() =>
            {
                isDynamicProp.boolValue = !isDynamicProp.boolValue;
                isDynamicProp.serializedObject.ApplyModifiedProperties();
            })
            {
                tooltip = "Toggle Type",
                text = "",
                style =
                {
                    alignSelf = Align.Center,
                    marginRight = 3,
                    flexShrink = 0,
                    width = 20,
                    height = 20,
                    backgroundImage = EditorGUIUtility.IconContent("Refresh").image as Texture2D,
                }
            };

            var label = new Label(preferredLabel);
            label.AddToClassList("unity-base-field__label");
            label.AddToClassList("unity-property-field__label");

            VisualElement staticField;
            Type argType;
            if (staticValueProp.propertyType == SerializedPropertyType.ManagedReference)
            {
                var textField = new TextField("") { style = { flexGrow = 1, flexShrink = 1, minWidth = 0 } };
                if (staticValueProp.managedReferenceValue is string strValue)
                    textField.value = strValue;
                else
                    textField.value = staticValueProp.managedReferenceValue?.ToString() ?? string.Empty;

                textField.RegisterValueChangedCallback(evt =>
                {
                    staticValueProp.managedReferenceValue = evt.newValue;
                    staticValueProp.serializedObject.ApplyModifiedProperties();
                });
                textField.TrackPropertyValue(staticValueProp, prop =>
                {
                    if (prop.managedReferenceValue is string updatedStr)
                        if (textField.value != updatedStr)
                            textField.value = updatedStr;
                });
                staticField = textField;
                argType = typeof(object);
            }
            else
            {
                staticField = new PropertyField(staticValueProp, "") { style = { flexGrow = 1, flexShrink = 1, minWidth = 0 } };
                argType = factProp.GetTargetField().FieldType.GetGenericArguments()[0];
            }

            var keyFactField = new KeyFactField() { style = { flexGrow = 1, flexShrink = 1, minWidth = 0 } };
            keyFactField.BindToProperties(keyProp, factProp);

            root.Add(label);
            root.Add(dynamicToggle);
            root.Add(staticField);
            root.Add(keyFactField);

            void UpdateVisibility()
            {
                bool isDynamic = isDynamicProp.boolValue;
                staticField.style.display = isDynamic ? DisplayStyle.None : DisplayStyle.Flex;
                keyFactField.style.display = isDynamic ? DisplayStyle.Flex : DisplayStyle.None;
            }

            UpdateVisibility();
            root.TrackPropertyValue(isDynamicProp, _ => UpdateVisibility());
            root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            void OnAttachToPanel(AttachToPanelEvent e)
            {
                label.text = (root.parent as PropertyField)?.label ?? property.displayName;
                root.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            }

            return root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var isDynamicProp = property.FindPropertyRelative("m_isDynamic");
            var staticValueProp = property.FindPropertyRelative("m_staticValue");
            var keyProp = property.FindPropertyRelative("m_key");
            var factProp = property.FindPropertyRelative("m_fact");

            float buttonWidth = 20f;

            Rect labelRect = new(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            Rect buttonRect = new(labelRect.xMax, position.y, buttonWidth, position.height);
            Rect fieldRect = new(buttonRect.xMax, position.y, position.width - EditorGUIUtility.labelWidth - buttonWidth, position.height);

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            GUIContent refreshIcon = EditorGUIUtility.IconContent("Refresh");
            refreshIcon.tooltip = "Toggle Type";

            if (GUI.Button(buttonRect, refreshIcon, EditorStyles.iconButton))
                isDynamicProp.boolValue = !isDynamicProp.boolValue;

            if (!isDynamicProp.boolValue)
            {
                if (staticValueProp.propertyType == SerializedPropertyType.ManagedReference)
                {
                    string strValue = staticValueProp.managedReferenceValue as string ?? string.Empty;
                    EditorGUI.BeginChangeCheck();
                    string newVal = EditorGUI.TextField(fieldRect, strValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        staticValueProp.managedReferenceValue = newVal;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(fieldRect, staticValueProp, GUIContent.none);
                }
            }
            else
            {
                float halfWidth = fieldRect.width / 2f;
                Rect keyRect = new(fieldRect.x, fieldRect.y, halfWidth, fieldRect.height);
                Rect factRect = new(keyRect.xMax, fieldRect.y, halfWidth, fieldRect.height);
                EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
                EditorGUI.PropertyField(factRect, factProp, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }
    }
}