using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor.Editor
{
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(CombinedAttribute), true)]
    public class CombinedAttributeDrawer : PropertyDrawer
    {
        private CombinedAttribute[] Attributes => fieldInfo.GetCustomAttributes(typeof(CombinedAttribute), true) //??? fslse
                                                              .Select(a => (CombinedAttribute)a)
                                                              .OrderBy(a => a.order)
                                                              .ToArray();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            // property.serializedObject.Update();

            var previousColor = GUI.color;
            var previousBackgrount = GUI.backgroundColor;
            var previousGUIState = GUI.enabled;

            foreach (var atr in Attributes)
            {
                label = atr.BuildLabel(label);
                atr.OnGUI(position, property, label);
            }

            if (Attributes.All(a => a.DrawDefaultPropertyField()))
                EditorGUI.PropertyField(position, property, label, true);

            GUI.color = previousColor;
            GUI.backgroundColor = previousBackgrount;
            GUI.enabled = previousGUIState;
            // property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            foreach (var atr in Attributes)
            {
                var h = atr.GetPropertyHeight(property, label);
                if (h.HasValue)
                    return h.Value;
            }
            // return base.GetPropertyHeight(property, label);
            return EditorGUI.GetPropertyHeight(property);
        }
    }
    #endif
}