using System;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class CombinedAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
        public virtual void OnSceeneGUI(SerializedProperty property) { }
        public virtual GUIContent BuildLabel(GUIContent label) => label;
        public virtual float? GetPropertyHeight(SerializedProperty property, GUIContent label) => null;
        public virtual bool DrawDefaultPropertyField() => true;
#endif
    }
}