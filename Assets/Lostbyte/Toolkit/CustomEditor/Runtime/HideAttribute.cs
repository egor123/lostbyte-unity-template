using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lostbyte.Toolkit.CustomEditor
{
    public class Hide : CombinedAttribute
    {
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
        public override float? GetPropertyHeight(SerializedProperty property, GUIContent label) => 0f;
        public override bool DrawDefaultPropertyField() => false;
#endif
    }
}