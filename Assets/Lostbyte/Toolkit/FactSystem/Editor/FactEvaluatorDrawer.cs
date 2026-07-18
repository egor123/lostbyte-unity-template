using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(FactEvaluator<,>), true)]
    [CustomPropertyDrawer(typeof(Statement))]
    public class FactEvaluatorDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, Tuple<string, bool>> _conditions = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement().MakeRow();
            root.AddToClassList("unity-base-field");
            root.AddToClassList("unity-property-field");
            var rootNodeProp = property.FindPropertyRelative("m_rootNode");

            if (!string.IsNullOrEmpty(preferredLabel))
            {
                var label = new Label(preferredLabel);
                label.AddToClassList("unity-base-field__label");
                label.AddToClassList("unity-property-field__label");
                label.tooltip = property.tooltip;
                root.Add(label);
            }

            var iconContainer = new VisualElement().SetSize(16, 16).SetAlignSelf(Align.Center).SetFlex(0, 0);

            Texture2D validIcon = EditorGUIUtility.IconContent("d_Valid@2x").image as Texture2D;
            Texture2D invalidIcon = EditorGUIUtility.IconContent("d_Invalid@2x").image as Texture2D;

            void UpdateIcon(bool hasErrors) => iconContainer.style.backgroundImage = hasErrors ? invalidIcon : validIcon;

            var fieldContainer = new VisualElement().MakeColumn(Justify.Center).SetFlex(1, 0);
            var textField = new TextField().Hide().SetEnabledState(!Application.isPlaying);

            var tokenDisplay = new VisualElement().MakeRow(Align.Center).SetFlex(1, 0);
            tokenDisplay.AddToClassList("unity-base-text-field__input");
            Print.Assert(rootNodeProp != null, "!!!!!!!!");
            var initialNode = rootNodeProp.managedReferenceValue as INode;
            string initialText = initialNode?.ToString() ?? string.Empty;
            textField.SetValueWithoutNotify(initialText);
            UpdateIcon(false);

            void buildTokens()
            {
                tokenDisplay.Clear();
                string text = textField.value;
                if (string.IsNullOrWhiteSpace(text))
                {
                    tokenDisplay.AddLabel("Click to edit...").SetBackgroundColor(Color.gray).SetAlignSelf(Align.Center);
                    return;
                }

                var matches = Regex.Matches(text, @"([a-zA-Z_]\w*\[[a-zA-Z_]\w*\])|(\+=|-=|\+\+|--|==|!=|>=|<=|>|<|\+|-|\*|/|%|\^|=|&&|\|\||\band\b|\bor\b|!|\(|\))|(\btrue\b|\bfalse\b|\d+(?:[.,]\d+)?|""[^""]*"")|(\b[a-zA-Z_][a-zA-Z0-9_]*\b)|([^\s]+)");

                foreach (Match match in matches)
                {
                    var lbl = new Label(match.Value).SetMargin(2).SetPadding(1, 4).SetBorderRadius(4).SetColor(new Color(0.9f, 0.9f, 0.9f));

                    if (match.Groups[1].Success) // FACT
                    {
                        lbl.style.backgroundColor = new Color(0.18f, 0.43f, 0.65f); // Blue
                    }
                    else if (match.Groups[2].Success) // OPERATORS & ACTIONS (+, -, +=, ==, etc)
                    {
                        lbl.style.color = new Color(0.7f, 0.7f, 0.7f); // Gray, Bold Text
                        lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                    }
                    else if (match.Groups[3].Success) // VALUES (Numbers, Bools, Strings)
                    {
                        lbl.style.backgroundColor = new Color(0.24f, 0.52f, 0.34f); // Green
                    }
                    else if (match.Groups[4].Success) // ENUMS / IDENTIFIERS
                    {
                        lbl.style.backgroundColor = new Color(0.7f, 0.5f, 0.2f); // Orange-ish
                    }
                    else if (match.Groups[5].Success) // UNKNOWN / ERRORS
                    {
                        lbl.style.backgroundColor = new Color(0.65f, 0.18f, 0.43f); // Red
                    }

                    tokenDisplay.Add(lbl);
                }
            }

            buildTokens();

            tokenDisplay.RegisterCallback<MouseDownEvent>(e =>
            {
                if (Application.isPlaying) return;
                tokenDisplay.style.display = DisplayStyle.None;
                textField.style.display = DisplayStyle.Flex;
                textField.schedule.Execute(() => textField.Q("unity-text-input")?.Focus());
            });
            textField.RegisterCallback<FocusOutEvent>(e =>
            {
                textField.style.display = DisplayStyle.None;
                tokenDisplay.style.display = DisplayStyle.Flex;
                buildTokens();
            });

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

            fieldContainer.Add(tokenDisplay);
            fieldContainer.Add(textField);

            root.Add(iconContainer);
            root.Add(fieldContainer);

            return root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

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