using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Lostbyte.Toolkit.Common;
using UnityEngine;

namespace Lostbyte.Toolkit.Editor
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            var hasValueProp = property.FindPropertyRelative("m_hasValue");
            var valueProp = property.FindPropertyRelative("m_value");
            var toggle = new Toggle();
            toggle.BindProperty(hasValueProp);
            toggle.style.alignSelf = Align.Center;
            toggle.style.marginRight = 2;
            var valueField = new PropertyField(valueProp);
            valueField.style.flexGrow = 1;
            valueField.label = property.displayName;
            valueField.SetEnabled(hasValueProp.boolValue);
            toggle.RegisterValueChangedCallback(evt => valueField.SetEnabled(evt.newValue));
            container.Add(toggle);
            container.Add(valueField);
            return container;
        }
        private const float k_toggleWidth = 18f;
        private const float k_spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hasValueProp = property.FindPropertyRelative("m_hasValue");
            var valueProp = property.FindPropertyRelative("m_value");

            var toggleRect = new Rect(position.x, position.y, k_toggleWidth, position.height);
            var valueRect = new Rect(
                position.x + k_toggleWidth + k_spacing,
                position.y,
                position.width - k_toggleWidth - k_spacing,
                position.height);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var hasValue = EditorGUI.Toggle(toggleRect, hasValueProp.boolValue);
            if (EditorGUI.EndChangeCheck()) hasValueProp.boolValue = hasValue;
            using (new EditorGUI.DisabledScope(!hasValueProp.boolValue))
            {
                EditorGUI.PropertyField(valueRect, valueProp, new GUIContent(property.displayName), true);
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("m_value");
            return EditorGUI.GetPropertyHeight(valueProp, new GUIContent(property.displayName), true);
        }
    }
}