using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lostbyte.Toolkit.CustomEditor
{
        public class RequiredAttribute : CombinedAttribute
        {
#if UNITY_EDITOR
                private bool _error;
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                        _error = IsEmpty(property);
                        if (_error)
                        {
                                EditorGUI.HelpBox(position, "Required", MessageType.Error);
                                EditorGUI.DrawRect(position, new Color(1f, 0f, 0f, 0.1f));
                        }
                }
                public override GUIContent BuildLabel(GUIContent label)
                {
                        if (_error) label.text = $"                      {label.text}"; // FIXME???
                        return label;
                        // return new GUIContent(" ");
                }

                public static bool IsEmpty(SerializedProperty property)
                {
                        return property.propertyType switch
                        {
                                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                                SerializedPropertyType.ManagedReference => property.managedReferenceValue == null,
                                SerializedPropertyType.ExposedReference => property.exposedReferenceValue == null,
                                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                                _ => false
                        };
                }
#endif
        }
}
