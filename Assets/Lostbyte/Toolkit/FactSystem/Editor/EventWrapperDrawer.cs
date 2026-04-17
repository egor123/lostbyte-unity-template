using UnityEditor;
using UnityEngine;
using System;
using Unity.VisualScripting;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    [CustomPropertyDrawer(typeof(EventWrapper))]
    public class EventWrapperDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var keyProp = property.FindPropertyRelative($"<{nameof(EventWrapper.Key)}>k__BackingField");
            var eventProp = property.FindPropertyRelative($"<{nameof(EventWrapper.Event)}>k__BackingField");
            var key = keyProp.objectReferenceValue as KeyContainer;
            var @event = eventProp.objectReferenceValue as EventDefinition;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);
            var w = (position.width - labelRect.width) / 3;
            var fieldPos1 = new Rect(labelRect.x + labelRect.width, labelRect.y, w, position.height);
            var fieldPos2 = new Rect(fieldPos1.x + w, labelRect.y, w, position.height);
            var fieldPos3 = new Rect(fieldPos2.x + w, labelRect.y, w, position.height);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUI.ObjectField(fieldPos1, keyProp, new GUIContent());
            EditorGUI.ObjectField(fieldPos2, eventProp, typeof(EventDefinition), new GUIContent());
            EditorGUI.EndDisabledGroup();


            EditorGUI.BeginDisabledGroup(!Application.isPlaying || @event == null || key == null);
            if (GUI.Button(fieldPos3, "Raise"))
                key.Raise(@event);
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    [CustomPropertyDrawer(typeof(SelfEventWrapper))]
    public class SelfEventWrapperDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var keyProp = property.FindPropertyRelative($"m_key");
            var eventProp = property.FindPropertyRelative($"<{nameof(SelfEventWrapper.Event)}>k__BackingField");
            var keyRef = keyProp.objectReferenceValue as KeyReference;

            if (!Application.isPlaying)
            {
                if (property.serializedObject.targetObject is Component component)
                {
                    var k = component.GetComponentInParent<KeyReference>();
                    if (keyRef != k)
                    {
                        keyProp.objectReferenceValue = keyRef = k;
                    }
                }
            }
            var key = keyRef != null ? keyRef.Key : null;
            var @event = eventProp.objectReferenceValue as EventDefinition;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);
            var w = (position.width - labelRect.width) / 3;
            var fieldPos1 = new Rect(labelRect.x + labelRect.width, labelRect.y, w, position.height);
            var fieldPos2 = new Rect(fieldPos1.x + w, labelRect.y, w, position.height);
            var fieldPos3 = new Rect(fieldPos2.x + w, labelRect.y, w, position.height);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(fieldPos1, keyRef != null ? keyRef.name : "null");
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUI.ObjectField(fieldPos2, eventProp, typeof(EventDefinition), new GUIContent());
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying || @event == null || key == null);
            if (GUI.Button(fieldPos3, "Raise"))
                key.Raise(@event);
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}