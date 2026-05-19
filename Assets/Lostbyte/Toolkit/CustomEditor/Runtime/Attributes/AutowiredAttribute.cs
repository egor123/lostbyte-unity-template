using System;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class Autowired : CombinedAttribute
    {
        public enum Type
        {
            Self,
            Parent,
            Children
        }
        private readonly Type _type;
        private readonly bool _isForced;

        public Autowired(Type type = Type.Self, bool isForced = false) => (_type, _isForced) = (type, isForced);

#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying || !_isForced && (property.objectReferenceValue != null || !GUI.enabled))
                return;

            var component = (Component)property.serializedObject.targetObject;
            var type = property.GetTargetType();
            var value = _type switch
            {
                Type.Self => component.GetComponent(type),
                Type.Parent => component.GetComponentInParent(type),
                Type.Children => component.GetComponentInChildren(type),
                _ => throw new NotImplementedException()
            };
            if (value != property.objectReferenceValue)
            {
                property.objectReferenceValue = value;
                // property.serializedObject.ApplyModifiedProperties();
                // property.SetTargetObject(value);
            }
        }
#endif
    }
}