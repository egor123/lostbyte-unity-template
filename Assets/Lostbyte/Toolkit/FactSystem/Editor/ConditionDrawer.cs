using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, Tuple<string, bool>> _conditions = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            root.AddToClassList("unity-base-field");
            root.AddToClassList("unity-property-field");
            var rootNodeProp = property.FindPropertyRelative("m_rootNode");
            var label = new Label(property.displayName);
            label.AddToClassList("unity-base-field__label");
            label.AddToClassList("unity-property-field__label");

            var iconContainer = new VisualElement
            {
                style =
                {
                    width = 16,
                    height = 16,
                    alignSelf = Align.Center,
                    marginRight = 3,
                    flexShrink = 0
                }
            };

            Texture2D validIcon = EditorGUIUtility.IconContent("d_Valid@2x").image as Texture2D;
            Texture2D invalidIcon = EditorGUIUtility.IconContent("d_Invalid@2x").image as Texture2D;

            void UpdateIcon(bool hasErrors) => iconContainer.style.backgroundImage = hasErrors ? invalidIcon : validIcon;

            var textField = new TextField() { style = { flexGrow = 1, minWidth = 50 } };
            textField.SetEnabled(!Application.isPlaying);
            var initialNode = rootNodeProp.managedReferenceValue as INode;
            textField.SetValueWithoutNotify(initialNode?.ToString() ?? string.Empty);
            UpdateIcon(false);
            textField.RegisterValueChangedCallback(evt =>
            {
                try
                {
                    var parsedNode = ConditionParser.Parse(evt.newValue);
                    rootNodeProp.managedReferenceValue = parsedNode;
                    rootNodeProp.serializedObject.ApplyModifiedProperties();
                    UpdateIcon(false);
                }
                catch (Exception)
                {
                    UpdateIcon(true);
                }
            });
            root.Add(label);
            root.Add(iconContainer);
            root.Add(textField);
            return root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var prop = property.FindPropertyRelative("m_rootNode");
            var node = prop.managedReferenceValue as INode;
            var p = property.propertyPath;
            string condition;
            bool hasErrors;
            if (_conditions.TryGetValue(p, out var c)) (condition, hasErrors) = c;
            else (condition, hasErrors) = (node?.ToString(), false);
            float iconSize = position.height;
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth - iconSize, position.height);
            var iconRect = new Rect(labelRect.x + labelRect.width, position.y, iconSize, position.height);
            var fieldRect = new Rect(iconRect.x + iconSize, position.y, position.width - labelRect.width - iconSize, position.height);

            EditorGUI.LabelField(labelRect, label);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            condition = EditorGUI.TextField(fieldRect, condition);
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    prop.managedReferenceValue = ConditionParser.Parse(condition);
                    hasErrors = false;
                }
                catch (Exception)
                {
                    hasErrors = true;
                }
            }
            var icon = hasErrors ? EditorGUIUtility.IconContent("d_Invalid@2x") : EditorGUIUtility.IconContent("d_Valid@2x");
            GUI.Label(iconRect, icon);
            _conditions[p] = new(condition, hasErrors);
            EditorGUI.EndProperty();
        }
    }
}