using System;
using System.Collections.Generic;
using Lostbyte.Toolkit.FactSystem.Nodes;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, Tuple<string, bool>> _conditions = new();

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