using System;
using Lostbyte.Toolkit.Common;
using Lostbyte.Toolkit.CustomEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(ReactionRule<,>))]
    public class ReactionRuleDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var modeProp = property.FindPropertyRelative(nameof(ReactionRule<int, IntCondition>.Mode));
            var targetCountProp = property.FindPropertyRelative(nameof(ReactionRule<int, IntCondition>.TargetCount));
            var conditionProp = property.FindPropertyRelative(nameof(ReactionRule<int, IntCondition>.Condition));
            var actionProp = property.FindPropertyRelative(nameof(ReactionRule<int, IntCondition>.Action));

            var ruleFoldout = new Foldout { value = property.isExpanded };
            ruleFoldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);
            root.Add(ruleFoldout);

            var modeField = new PropertyField(modeProp);
            var targetCountField = new PropertyField(targetCountProp);

            var conditionField = new PropertyField(conditionProp); // Fixme draw unique ref without children (custom label (?)) and if it has child draw them bellow as normal fields
            var actionField = new PropertyField(actionProp);

            void UpdateDynamicUI()
            {
                CountMode currentMode = (CountMode)modeProp.enumValueIndex;
                bool showTargetCount = currentMode == CountMode.Exactly ||
                                       currentMode == CountMode.GreaterThan ||
                                       currentMode == CountMode.LessThan;

                targetCountField.style.display = showTargetCount ? DisplayStyle.Flex : DisplayStyle.None;

                string dynamicLabel = $"Rule: {currentMode}";
                if (showTargetCount) dynamicLabel += $" {targetCountProp.intValue}";
                ruleFoldout.text = dynamicLabel;
            }

            void UpdateEnum()
            {
                if (conditionProp.boxedValue is not EnumCondition) return;
                var fact = GetParentFactDefinition(property);
                if (fact == null) return;
                var valueProp = conditionProp.FindPropertyRelative("TargetValue");
                if (valueProp == null) return;
                if (valueProp.boxedValue != null) return;
                valueProp.boxedValue = fact.DefaultEnumValue;
                property.serializedObject.ApplyModifiedProperties();
            }

            ruleFoldout.TrackPropertyValue(modeProp, _ => UpdateDynamicUI());
            ruleFoldout.TrackPropertyValue(targetCountProp, _ => UpdateDynamicUI());
            ruleFoldout.TrackPropertyValue(conditionProp, _ => UpdateEnum());

            UpdateDynamicUI();
            UpdateEnum();

            ruleFoldout.Add(modeField);
            ruleFoldout.Add(targetCountField);

            ruleFoldout.Add(conditionField);
            ruleFoldout.Add(actionField);

            return root;
        }

        private EnumFactDefinition GetParentFactDefinition(SerializedProperty property)
        {
            string path = property.propertyPath;
            int reactionsIndex = path.IndexOf(".Reactions", StringComparison.Ordinal);
            if (reactionsIndex > 0)
            {
                string registrationPath = path[..reactionsIndex];
                var registrationProp = property.serializedObject.FindProperty(registrationPath);
                if (registrationProp != null)
                {
                    var factProp = registrationProp.FindPropertyRelative(nameof(FactRegistration.Fact));
                    return factProp?.objectReferenceValue as EnumFactDefinition;
                }
            }
            return null;
        }

    }
    [CustomPropertyDrawer(typeof(EnumCondition), true)]
    public class EnumConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect labelRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);
            var targetValueProp = property.FindPropertyRelative("TargetValue");
            if (targetValueProp != null)
            {
                Rect valueRect = new(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                if (targetValueProp.boxedValue is Enum enumValue)
                {
                    EditorGUI.BeginChangeCheck();
                    Enum newValue = EditorGUI.EnumPopup(valueRect, targetValueProp.displayName, enumValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        targetValueProp.boxedValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    GUIStyle italicStyle = new(EditorStyles.label) { fontStyle = FontStyle.Italic };
                    EditorGUI.LabelField(valueRect, "Select a Fact to initialize...", italicStyle);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var targetValueProp = property.FindPropertyRelative("TargetValue");
            if (targetValueProp != null)
                return (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing;
            return EditorGUIUtility.singleLineHeight;
        }
    }
}